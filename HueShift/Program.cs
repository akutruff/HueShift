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

namespace HueShift
{
    public enum ColorTemperature
    {
        Blue = 250,
        Red = 454,
    }

    public class Configuration
    {
        public TimeSpan TransitionTimeSpan = TimeSpan.FromSeconds(3);
        public TimeSpan PollingFrequency = TimeSpan.FromSeconds(10);
        public int DayColorTemperature = (int)ColorTemperature.Blue;
        public int NightColorTemperature = (int)ColorTemperature.Red;

        public string BridgeUri = "http://hue.2k.lan/api/";
        public string BridgeApiKey = "FWPwy-Xj2Ww7RSt1SaXrkRdhBc6M79IIKco2WouT";

        public string IpStackUri = "http://api.ipstack.com/check?access_key=";
        public string IpStackApiKey = "35c43096adc9416dab6bdd2d1ad53069";

        public TimeSpan SunriseMustBeAfter = new TimeSpan(10, 0, 0);
        public TimeSpan SunriseMustBeBeBefore = new TimeSpan(10, 0, 0);
        public TimeSpan SunsetMustBeAfter = new TimeSpan(10, 0, 0);
        public TimeSpan SunsetMustBeBeBefore = new TimeSpan(10, 0, 0);

        public double Latitude;
        public double Longitude;
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new Configuration();
            
            var timeBetweenChecks = new TimeSpan(Math.Max(configuration.TransitionTimeSpan.Ticks, configuration.PollingFrequency.Ticks));

            bool hasLastErrorAlreadyBeenLogged = false;

            Exception lastException;
            do
            {
                try
                {
                    var coordinates = GetGeolocationFromIPAddress(configuration);
                    configuration.Latitude = coordinates.latitude;
                    configuration.Longitude = coordinates.latitude;
                    lastException = null;
                }
                catch(Exception e)
                {
                    lastException = e;
                }
            } while (lastException != null);

            while (true)
            {
                try
                {
                    var lastRunTime = DateTimeOffset.Now;

                    int colorTemperature = GetTargetColorTemperature(lastRunTime, configuration);

                    SetLightsToColorTemperature(colorTemperature, configuration);
                    hasLastErrorAlreadyBeenLogged = false;
                }
                catch
                {
                    if (!hasLastErrorAlreadyBeenLogged)
                        Console.WriteLine($"{DateTimeOffset.Now}: Bridge Exception");

                    hasLastErrorAlreadyBeenLogged = true;
                }

                await Task.Delay(timeBetweenChecks);
            }
        }

        private static void SetLightsToColorTemperature(int colorTemperature, Configuration configuration)
        {
            var bridgeUri = new Uri(configuration.BridgeUri + configuration.BridgeApiKey);

            const string allLightsGroupName = "Hue3D";

            var allLights = Http.GetJson<Dictionary<string, Light>>(bridgeUri, "lights");

            var lightsGroupedByOnState = allLights.GroupBy(item => item.Value.state.on)
                .ToDictionary(x => x.Key, g => g.ToDictionary(x => x.Key, x => x.Value));

            Dictionary<string, Light> onLights = null;

            bool areAllLightsInNecessaryState = true;
            if (lightsGroupedByOnState.TryGetValue(true, out onLights))
            {
                foreach (var onLight in onLights)
                {
                    Light.LightState lightState = onLight.Value.state;
                    if (lightState.reachable == true)
                    {
                        if (lightState.ct != colorTemperature)
                        {
                            Console.WriteLine($"{DateTime.Now}: Light { onLight.Key } switching { lightState.ct } -> { colorTemperature }");
                            areAllLightsInNecessaryState = false;
                            break;
                        }
                    }
                }
            }

            if (!areAllLightsInNecessaryState)
            {
                var lightGroups = Http.GetJson<Dictionary<string, LightGroup>>(bridgeUri, "groups");

                string allLightsGroupId = GetOrCreateGroup(bridgeUri, lightGroups, allLights, allLightsGroupName);

                if (allLightsGroupId == null)
                {
                    throw new Exception();
                }

                //var transitionTime = TimeSpan.FromMinutes(15);
                var groupActionResponse = Http.PutJson<object>(bridgeUri, "groups/" + allLightsGroupId + "/action", new
                {
                    ct = colorTemperature,
                    transitiontime = GetTransitionTimeInHueUnits(configuration.TransitionTimeSpan),
                });

                //Thread.Sleep(transitionTime);
            }
        }

        private static int GetTransitionTimeInHueUnits(TimeSpan timespan)
        {
            return (int)(10.0 * timespan.TotalSeconds);
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
            DateTimeOffset sunrise = solarTimes.Sunrise;
            DateTimeOffset sunset = solarTimes.Sunset;

            int colorTemperature;

            // if (sunset > new DateTimeOffset(currentTime.Date + configuration.SunsetMustBeBeBefore))
            // {
            //     sunset = new DateTimeOffset(currentTime.Date + configuration.SunsetMustBeBeBefore);
            // }
            // else if (sunset < new DateTimeOffset(currentTime.Date + configuration.SunsetMustBeAfter))
            // {
            //     sunset = new DateTimeOffset(currentTime.Date + configuration.SunsetMustBeAfter);
            // }

            if (currentTime < sunrise || currentTime > sunset)
            {
                colorTemperature = configuration.DayColorTemperature;
            }
            else
            {
                colorTemperature = configuration.NightColorTemperature;
            }

            return colorTemperature;
        }

        private static string GetOrCreateGroup(Uri bridgeUri, Dictionary<string, LightGroup> lightGroups, Dictionary<string, Light> lights, string lightGroupName)
        {
            string groupForSettingId = null;
            if (lightGroups.Where(x => x.Value.name == lightGroupName).Any())
            {
                var groupForSetting = lightGroups.Where(x => x.Value.name == lightGroupName).First();
                groupForSettingId = groupForSetting.Key;

                var response = Http.PutJson<object>(bridgeUri, "groups/" + groupForSettingId, new
                {
                    lights = lights.Keys.ToList()
                });
            }
            else
            {
                var response = Http.PostJson<List<Dictionary<string, IdResponse>>>(bridgeUri, "groups", new
                {
                    name = lightGroupName,
                    //recycle = true,
                    lights = lights.Keys.ToList()
                });

                foreach (var item in response)
                {
                    IdResponse idResponse;
                    if (item.TryGetValue("success", out idResponse))
                    {
                        groupForSettingId = idResponse.id;
                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                Console.WriteLine(response);
            }

            return groupForSettingId;
        }

        private static void SetLightState(Uri bridgeUri, string lightNumber, object settings)
        {
            var response = Http.PutJson<object>(bridgeUri, "lights/" + lightNumber + "/state", settings);
        }
    }
}
