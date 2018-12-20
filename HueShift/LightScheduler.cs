using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArtDotNet;
using Innovative.SolarCalculator;

using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Effects;
using Q42.HueApi.Streaming.Effects.Examples;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;


namespace HueShift
{
    public readonly struct LightState : IEquatable<LightState>
    {
        public readonly bool IsOn;
        public readonly byte Brightness;
        public readonly byte ColorTemperature;

        public LightState(bool isOn, byte brightness, byte colorTemperature)
        {
            IsOn = isOn;
            Brightness = brightness;
            ColorTemperature = colorTemperature;
        }

        public override bool Equals(object obj)
        {
            return obj is LightState && Equals((LightState)obj);
        }

        public bool Equals(LightState other)
        {
            return IsOn == other.IsOn &&
                   Brightness == other.Brightness &&
                   ColorTemperature == other.ColorTemperature;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsOn, Brightness, ColorTemperature);
        }
    }

    public class LightScheduler
    {
        enum RunningState
        {
            DMX,
            HueShift
        }

        public static async Task RunLightControl(Configuration configuration, HueClient hueClient)
        {
            var timeBetweenChecks = new TimeSpan(Math.Max(configuration.TransitionTime.Ticks, configuration.PollingFrequency.Ticks));

            await AsyncUtils.ContinuallyExecuteReportingExceptionsOnlyOnce(async () =>
            {
                var dmxUniverse = configuration.DMXConfiguration.Universe;

                RunningState runningState = RunningState.HueShift;
                using (var controller = new ArtNetController())
                {
                    controller.Address = IPAddress.Parse(configuration.DMXConfiguration.ListeningIPAddress);

                    var groupId = await hueClient.CreateGroupAsync(
                        configuration.DMXConfiguration.LightIdsInDmxGroup.Select(x => x.ToString()).ToList(),
                        "DmxControlGroup",
                        null,
                        Q42.HueApi.Models.Groups.GroupType.LightGroup);

                    var stateQueue = new ConcurrentQueue<LightState>();

                    var startingChannel = configuration.DMXConfiguration.StartingChannel;
                    controller.DmxPacketReceived += (sender, packet) =>
                    {
                        if (packet.SubUniverse != dmxUniverse)
                            return;

                        LightState state = new LightState(
                            packet.Data[startingChannel] > 0,
                            packet.Data[startingChannel + 1],
                            packet.Data[startingChannel + 2]);

                        if (state.IsOn)
                        {
                            //Sanity check to prevent runawayqueu
                            const int maxQueueSize = 10000;
                            if (stateQueue.Count < maxQueueSize)
                            {
                                stateQueue.Enqueue(state);
                            }
                        }
                        else
                        {
                            //Console.Write("D");
                        }
                    };

                    controller.Start();

                    var lastApiSentTime = DateTimeOffset.MinValue;
                    var lastTimeDmxPacketDetected = DateTimeOffset.MinValue;

                    LightState lastSentLightState = new LightState();
                    Console.WriteLine("_------------------STARTING ");

                    while (true)
                    {
                        switch (runningState)
                        {
                            case RunningState.HueShift:
                                var now = DateTimeOffset.Now;

                                int colorTemperature = GetTargetColorTemperature(now, configuration);

                                await SetLightsToColorTemperature(hueClient, colorTemperature, configuration);

                                var timeStartedWaiting = DateTimeOffset.Now;
                                do
                                {
                                    if (!stateQueue.IsEmpty)
                                    {
                                        lastTimeDmxPacketDetected = DateTimeOffset.Now;
                                        runningState = RunningState.DMX;
                                        Console.WriteLine("DMX Mode detected");
                                    }
                                    else
                                    {

                                        Thread.Sleep(configuration.DMXConfiguration.MillisToSleepBetweenQueueChecks);

                                        //await Task.Delay(.0).ConfigureAwait(false);
                                    }
                                } while ((DateTimeOffset.Now - timeStartedWaiting) < timeBetweenChecks) ;

                                break;
                            case RunningState.DMX:
        
                                var transitionDuration = configuration.DMXConfiguration.TransitionTime;
                                var minWaitDurationBetweenApiCalls = configuration.DMXConfiguration.MinWaitDurationBetweenApiCalls;

                                var timeSinceLastSend = DateTimeOffset.Now - lastApiSentTime;

                                var timeSinceLastReceivedDMXPacket = DateTimeOffset.Now - lastTimeDmxPacketDetected;

                                LightState mostCurrentLightState = new LightState();

                                bool haveReceivedNextDMXPacket = false;
                                while (timeSinceLastSend < minWaitDurationBetweenApiCalls)
                                {
                                    Thread.Sleep(configuration.DMXConfiguration.MillisToSleepBetweenQueueChecks);

                                    LightState latestLightState;
                                    while (stateQueue.TryDequeue(out latestLightState))
                                    {
                                        mostCurrentLightState = latestLightState;
                                        haveReceivedNextDMXPacket = true;
                                        lastTimeDmxPacketDetected = DateTime.Now;
                                    }

                                    timeSinceLastSend = DateTimeOffset.Now - lastApiSentTime;
                                }

                                if(!haveReceivedNextDMXPacket)
                                {
                                    LightState latestLightState;
                                    while (!stateQueue.TryDequeue(out latestLightState))
                                    {
                                        Thread.Sleep(configuration.DMXConfiguration.MillisToSleepBetweenQueueChecks);

                                        if ((DateTimeOffset.Now - lastTimeDmxPacketDetected) > configuration.DMXConfiguration.TimeAfterLastDmxPacketToReturnToShifting)
                                        {
                                            runningState = RunningState.HueShift;
                                            break;
                                        }
                                    }

                                    mostCurrentLightState = latestLightState;
                                    haveReceivedNextDMXPacket = true;
                                    lastTimeDmxPacketDetected = DateTime.Now;
                                }               

                                if (runningState == RunningState.DMX)
                                {
                                    if (!mostCurrentLightState.Equals(lastSentLightState))
                                    {
                                        Console.WriteLine();
                                        var command = new LightCommand();

                                        if (mostCurrentLightState.Brightness != lastSentLightState.Brightness)
                                            command.Brightness = mostCurrentLightState.Brightness;

                                        //if (latest.ColorTemperature != last.ColorTemperature)
                                        //    command.ColorTemperature = (int)map(latest.ColorTemperature, 0, 255, (float)ColorTemperature.Blue, (float)ColorTemperature.Red);

                                        command.TransitionTime = transitionDuration;

                                        Console.Write($"Setting: {mostCurrentLightState.IsOn} {mostCurrentLightState.Brightness} {mostCurrentLightState.ColorTemperature}");

                                        var hueResults = await hueClient.SendGroupCommandAsync(command, groupId);

                                        foreach (var error in hueResults.Errors)
                                        {
                                            Console.WriteLine($"Error: {error}");
                                        }

                                        lastApiSentTime = DateTimeOffset.Now;
                                        lastSentLightState = mostCurrentLightState;
                                    }
                                }

                                break;
                        }
                    }
                }
            });
        }

        //public static void RunArtnet(Configuration configuration)
        //{
        //    var subUni = configuration.DMXConfiguration.DMXUniverse;

        //    Console.WriteLine("ArtDotNet Client");
        //    var controller = new ArtNetController();

        //    controller.Address = IPAddress.Parse(configuration.DMXConfiguration.ListeningIPAddress);

        //    controller.DmxPacketReceived += (sender, packet) =>
        //    {
        //        if (packet.SubUniverse != subUni)
        //            return;

        //        Console.Clear();
        //        Console.WriteLine("ArtNet Universe " + subUni);

        //        for (int i = 0; i < packet.Length; i++)
        //        {
        //            if (i % 24 == 0)
        //                Console.WriteLine();

        //            Console.Write(string.Format("{000:00} ", packet.Data[i]));
        //        }
        //    };

        //    controller.Start();

        //}

        static float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        private static async Task SetLightsToColorTemperature(HueClient hueClient, int colorTemperature, Configuration configuration)
        {
            var allLights = await hueClient.GetLightsAsync();
            var lights = allLights.ToList();
            var nameToLights = allLights.ToDictionary(x => x.Name, x => x);
            var namesOfLightsToExclude = configuration.IdsOfLightsToExclude.ToHashSet();
            var lightsToChange = allLights.Where(x => x.State.On && x.State.ColorTemperature != colorTemperature && !namesOfLightsToExclude.Contains(x.Name))
                .ToList();

            foreach (var onLight in lightsToChange)
            {
                Console.WriteLine($"{DateTime.Now}: Light { onLight.Name } switching { onLight.State.ColorTemperature } -> { colorTemperature }");
            }

            if (lightsToChange.Count > 0)
            {
                var command = new LightCommand();
                command.ColorTemperature = colorTemperature;
                command.TransitionTime = configuration.TransitionTime;

                await hueClient.SendCommandAsync(command, lightsToChange.Select(x => x.Id).ToList());
            }
        }

        private static int GetTargetColorTemperature(DateTimeOffset currentTime, Configuration configuration)
        {
            SolarTimes solarTimes = new SolarTimes(currentTime, configuration.PositionState.Latitude, configuration.PositionState.Longitude);

            //These are local DateTimes Kind: Unspecificied 
            DateTimeOffset sunrise = ClampTime(solarTimes.Sunrise, configuration.SunriseMustBeAfter, configuration.SunriseMustBeBeBefore);
            DateTimeOffset sunset = ClampTime(solarTimes.Sunset, configuration.SunsetMustBeAfter, configuration.SunsetMustBeBeBefore);

            int colorTemperature;

            if (currentTime < sunrise || currentTime > sunset)
            {
                colorTemperature = configuration.NightColorTemperature;
            }
            else
            {
                colorTemperature = configuration.DayColorTemperature;
            }

            return colorTemperature;
        }

        public static DateTimeOffset ClampTime(DateTimeOffset time, TimeSpan minTimeOfDay, TimeSpan maxTimeOfDay)
        {
            var clamped = new TimeSpan(Math.Clamp(time.TimeOfDay.Ticks, minTimeOfDay.Ticks, maxTimeOfDay.Ticks));
            return time.Date + clamped;
        }
    }

    class ArtnetBridge
    {
        public static async Task<StreamingGroup> SetupAndReturnGroup(Configuration configuration)
        {
            //string ip = "192.168.0.4";
            //string key = "8JwWAj5J1tSsKLxyUOdAkWmcCQFcNc51AKRhxdH9";
            //string entertainmentKey = "AFFD322C34C993C19503D369481869FD";
            //var useSimulator = false;

            //string ip = "10.70.16.38";
            //string key = "dpzXfw8NvafvCCvtLkQLUET-6Kc4jT4RovPg59Rx";
            //string entertainmentKey = "260FE0B7251DF783CFB9FBAB1D1E8B0C";
            //var useSimulator = false;

            //string ip = "127.0.0.1";
            //string key = "aSimulatedUser";
            //string entertainmentKey = "01234567890123456789012345678901";
            var useSimulator = true;

            //Initialize streaming client
            StreamingHueClient client = new StreamingHueClient(configuration.BridgeState.BridgeHostname, configuration.BridgeState.BridgeApiKey, configuration.BridgeState.BridgeEntertainmentClientKey);

            //Get the entertainment group
            var all = await client.LocalHueClient.GetEntertainmentGroups();
            var group = all.FirstOrDefault();

            if (group == null)
                throw new Exception("No Entertainment Group found. Create one using the Q42.HueApi.UniversalWindows.Sample");
            else
                Console.WriteLine($"Using Entertainment Group {group.Id}");

            //Create a streaming group
            var stream = new StreamingGroup(group.Locations);
            stream.IsForSimulator = useSimulator;


            //Connect to the streaming group
            await client.Connect(group.Id, simulator: useSimulator);

            //Start auto updating this entertainment group
            client.AutoUpdate(stream, new System.Threading.CancellationToken(), 50, onlySendDirtyStates: false);

            //Optional: Check if streaming is currently active
            var bridgeInfo = await client.LocalHueClient.GetBridgeAsync();
            Console.WriteLine(bridgeInfo.IsStreamingActive ? "Streaming is active" : "Streaming is not active");
            return stream;
        }
    }
}
