using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Threading;
using Innovative.SolarCalculator;
using McMaster.Extensions.CommandLineUtils;

using Q42.HueApi;

namespace HueShift
{
    public class LightScheduler
    {
        public static async Task Main()
        {
            var configuration = new Configuration();

            var timeBetweenChecks = new TimeSpan(Math.Max(configuration.TransitionTimeSpan.Ticks, configuration.PollingFrequency.Ticks));

            var coordinates = await AsyncUtils.Retry(() => GetGeolocationFromIPAddress(configuration), 120);

            configuration.Latitude = coordinates.latitude;
            configuration.Longitude = coordinates.longitude;

            await AsyncUtils.Retry(async () =>
            {
                LocalHueClient hueClient = new LocalHueClient(configuration.BridgeIP);
                hueClient.Initialize(configuration.BridgeApiKey);

                while (true)
                {
                    var lastRunTime = DateTimeOffset.Now;

                    int colorTemperature = GetTargetColorTemperature(lastRunTime, configuration);
                    if(colorTemperature == 0)
                        return 1;

                    await SetLightsToColorTemperature(hueClient, colorTemperature, configuration);

                    await Task.Delay(timeBetweenChecks).ConfigureAwait(false);
                }
                //return 1;
            });
        }

        private static async Task SetLightsToColorTemperature(HueClient hueClient, int colorTemperature, Configuration configuration)
        {
            var allLights = await hueClient.GetLightsAsync();
            var onLights = allLights.Where(item => item.State.On);

            var lightIdsToChange = new List<string>();

            foreach (var onLight in onLights)
            {
                var lightState = onLight.State;
                if (lightState.On == true)
                {
                    if (lightState.ColorTemperature != colorTemperature)
                    {
                        Console.WriteLine($"{DateTime.Now}: Light { onLight.Name } switching { lightState.ColorTemperature } -> { colorTemperature }");
                        lightIdsToChange.Add(onLight.Id);
                    }
                }
            }

            if (lightIdsToChange.Count > 0)
            {
                var command = new LightCommand();
                command.ColorTemperature = colorTemperature;
                command.TransitionTime = configuration.TransitionTimeSpan;
                await hueClient.SendCommandAsync(command, lightIdsToChange);
            }
        }

        private static (double latitude, double longitude) GetGeolocationFromIPAddress(Configuration configuration)
        {
            var geolocationURi = new Uri(configuration.IpStackUri + configuration.IpStackApiKey);
            var geolocationResponse = Http.Get(geolocationURi);
            dynamic response = JObject.Parse(geolocationResponse);

            return (
                (double)response.latitude,
                (double)response.longitude);
        }

        private static int GetTargetColorTemperature(DateTimeOffset currentTime, Configuration configuration)
        {
            SolarTimes solarTimes = new SolarTimes(currentTime, configuration.Latitude, configuration.Longitude);

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
}
