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
        public static async Task ContinuallyEnforceLightTemperature(Configuration configuration, HueClient hueClient)
        {
            var timeBetweenChecks = new TimeSpan(Math.Max(configuration.TransitionTime.Ticks, configuration.PollingFrequency.Ticks));

            await AsyncUtils.Retry(async () =>
            {
                while (true)
                {
                    var now = DateTimeOffset.Now;

                    int colorTemperature = GetTargetColorTemperature(now, configuration);
           
                    await SetLightsToColorTemperature(hueClient, colorTemperature, configuration);

                    await Task.Delay(timeBetweenChecks).ConfigureAwait(false);
                }
            });
        }

        private static async Task SetLightsToColorTemperature(HueClient hueClient, int colorTemperature, Configuration configuration)
        {
            var allLights = await hueClient.GetLightsAsync();
            var onLights = allLights.Where(item => item.State.On).ToList();

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
                command.TransitionTime = configuration.TransitionTime;

                await hueClient.SendCommandAsync(command, lightIdsToChange);
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
}
