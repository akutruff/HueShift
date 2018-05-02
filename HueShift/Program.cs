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
using Q42.HueApi.NET;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi;

namespace HueShift
{
    public partial class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.ValueParsers.Add(new TimeSpanParser());

            app.HelpOption("-h|--help");

            var resetOption = app.Option("-r|--reset", "Clear all saved configuration to defaults.", CommandOptionType.NoValue);

            var discoverDevicesOption = app.Option("-d|--discover-devices", "Discover bridges on network even if you already have.", CommandOptionType.NoValue);
            var bridgeHostnameOption = app.Option<string>("--bridge-hostname <hostname>", "Manually enter bridge hostname or ip address.", CommandOptionType.SingleValue);

            var listDevicesOption = app.Option("-l|--list-devices", "List all known devices on network.", CommandOptionType.NoValue);

            var sunsetMustBeAfterOption = app.Option<TimeSpan>("--sunset-must-be-after <Time>", "Lights will not shift to nightime until at least this time.", CommandOptionType.SingleValue);
            var sunsetMustBeBeforeOption = app.Option<TimeSpan>("--sunset-must-be-before <Time>", "Lights will always shift to nighttime even if the sun is still up.", CommandOptionType.SingleValue);

            var sunriseMustBeAfterOption = app.Option<TimeSpan>("--sunrise-must-be-after <Time>", "Lights will not shift to day until at least this time.", CommandOptionType.SingleValue);
            var sunriseMustBeBeforeOption = app.Option<TimeSpan>("--sunrise-must-be-before <Time>", "Lights will always shift to day even if the sun is still down.", CommandOptionType.SingleValue);

            var latitudeOption = app.Option<double>("--latitude <degrees>", "Latitude for calculating sunrise/sunset. Must have longitude", CommandOptionType.SingleValue);
            var longitudeOption = app.Option<double>("--longitude <degrees>", "Longitude for calculating sunrise/sunset. Must have latitude", CommandOptionType.SingleValue);

            var dayColorTemperatureOption = app.Option<int>("--day-color-temperature <temperature>", "Day color temperature. (250)", CommandOptionType.SingleValue);
            var nightColorTemperatureOption = app.Option<int>("--night-color-temperature <temperature>", "Day color temperature. (454)", CommandOptionType.SingleValue);

            var pollingFrequencyOption = app.Option<TimeSpan>("-f|--polling-frequency <Time>", "How frequently should the lights be checked for the right temperature.", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                Configuration configuration = new Configuration();

                // var subject = optionSubject.HasValue()
                //     ? optionSubject.Value()
                //     : "world";

                // var count = optionRepeat.HasValue() ? optionRepeat.ParsedValue : 1;
                // for (var i = 0; i < count; i++)
                // {
                //     Console.Write($"Hello");

                //     // This pause here is just for indication that some awaitable operation could happens here.
                //     await Task.Delay(5000);
                //     Console.WriteLine($" {subject}!");
                // }
                var coordinates = await AsyncUtils.Retry(() => Geolocation.GetLocationFromIPAddress(configuration), 120);

                configuration.Latitude = coordinates.latitude;
                configuration.Longitude = coordinates.longitude;

                if (string.IsNullOrEmpty(configuration.BridgeIP))
                {
                    var locatedBridges = await DiscoverDevicesAsync(configuration);

                    if(locatedBridges.Count == 0)
                    {
                        throw new Exception("No bridges discovered on your network.");
                    }

                    configuration.BridgeIP = locatedBridges[0].IpAddress;
                }

                LocalHueClient hueClient = new LocalHueClient(configuration.BridgeIP);
                if (string.IsNullOrEmpty(configuration.BridgeApiKey))
                {
                    configuration.BridgeApiKey = await hueClient.RegisterAsync("HueShift", "Bridge0");
                }
                else
                {
                    hueClient.Initialize(configuration.BridgeApiKey);
                }

                await LightScheduler.ContinuallyEnforceLightTemperature(configuration, hueClient);
            });

            return app.Execute(args);
        }

        private static async Task<List<LocatedBridge>> DiscoverDevicesAsync(Configuration configuration)
        {
            List<LocatedBridge> locatedBridges = new List<LocatedBridge>();
            try
            {
                SSDPBridgeLocator locator = new SSDPBridgeLocator();
                var bridges = (await locator.LocateBridgesAsync(TimeSpan.FromSeconds(30))).ToList();
                
                locatedBridges.AddRange(bridges);
            }
            catch
            {

            }

            if(locatedBridges.Count == 0)
            {
                HttpBridgeLocator locator = new HttpBridgeLocator();
                var bridges = (await locator.LocateBridgesAsync(TimeSpan.FromSeconds(30))).ToList();
                
                locatedBridges.AddRange(bridges);
            }

            return locatedBridges;
        }
    }
}
