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
using System.IO;
using Newtonsoft.Json;

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
            var doNotSaveConfigurationOption = app.Option("--do-not-save-config", "Do not save configuration.", CommandOptionType.NoValue);

            var discoverBridgesOption = app.Option("-d|--discover-bridges", "Discover bridges on network even if you already have.", CommandOptionType.NoValue);
            var bridgeHostnameOption = app.Option<string>("--bridge-hostname <hostname>", "Manually enter bridge hostname or ip address.", CommandOptionType.SingleValue);

            var listDevicesOption = app.Option("-l|--list-devices", "List all known devices on network.", CommandOptionType.NoValue);

            var sunsetMustBeAfterOption = app.Option<TimeSpan>("--sunset-must-be-after <Time>", "Lights will not shift to nightime until at least this time.", CommandOptionType.SingleValue);
            var sunsetMustBeBeforeOption = app.Option<TimeSpan>("--sunset-must-be-before <Time>", "Lights will always shift to nighttime even if the sun is still up.", CommandOptionType.SingleValue);

            var sunriseMustBeAfterOption = app.Option<TimeSpan>("--sunrise-must-be-after <Time>", "Lights will not shift to day until at least this time.", CommandOptionType.SingleValue);
            var sunriseMustBeBeforeOption = app.Option<TimeSpan>("--sunrise-must-be-before <Time>", "Lights will always shift to day even if the sun is still down.", CommandOptionType.SingleValue);

            var latitudeOption = app.Option<double>("--latitude <degrees>", "Latitude for calculating sunrise/sunset. Must have longitude", CommandOptionType.SingleValue);
            var longitudeOption = app.Option<double>("--longitude <degrees>", "Longitude for calculating sunrise/sunset. Must have latitude", CommandOptionType.SingleValue);

            var dayColorTemperatureOption = app.Option<int>("--day-color-temperature <temperature>", "Day color temperature. (250)", CommandOptionType.SingleValue);
            var nightColorTemperatureOption = app.Option<int>("--night-color-temperature <temperature>", "Night color temperature. (454)", CommandOptionType.SingleValue);

            var pollingFrequencyOption = app.Option<TimeSpan>("-p|--polling-frequency <Time>", "How frequently should the lights be checked for the right temperature.", CommandOptionType.SingleValue);
            var transitionTimeOption = app.Option<TimeSpan>("-t|--transition-time <Time>", "How quickly should the lights fade between colors.", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                Configuration configuration = new Configuration();

                Console.WriteLine($"Timezone: {TimeZoneInfo.Local} <--- make sure this is right!!!");

                if (discoverBridgesOption.HasValue())
                {
                    var locatedBridges = await DiscoverBridgesAsync(configuration);

                    if (locatedBridges.Count == 0)
                    {
                        Console.WriteLine("No bridges found");
                    }

                    for (int i = 0; i < locatedBridges.Count; i++)
                    {
                        Console.WriteLine($"ID: {locatedBridges[i].BridgeId,-10} IP: {locatedBridges[i].IpAddress}");
                    }
                    return -1;
                }

                string configurationFileName = @"hueShift-conf.json";
                if (resetOption.HasValue())
                {
                    File.Delete(configurationFileName);
                    return -1;
                }

                if (File.Exists(configurationFileName))
                {
                    configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configurationFileName));
                }

                if (latitudeOption.HasValue() ^ longitudeOption.HasValue())
                    throw new Exception("When supplying latitude or longitude, you must supply both.  Only one of them was given.");

                if (latitudeOption.HasValue())
                {
                    configuration.PositionState = new PositionState
                    {
                        Latitude = latitudeOption.ParsedValue,
                        Longitude = longitudeOption.ParsedValue,
                    };
                }

                if (configuration.PositionState == null)
                {
                    try
                    {
                        configuration.PositionState = new PositionState();
                        (configuration.PositionState.Latitude, configuration.PositionState.Longitude) = await Geolocation.GetLocationFromIPAddress(configuration);
                        TrySaveConfiguration(doNotSaveConfigurationOption, configuration, configurationFileName);
                    }
                    catch
                    {
                        Console.Write("Failed to get geolocation data for your ip address.  Please visit ipstack.com to get your latitude and longitude and rerun the program with --latitude and --longitude command line arguments or put them directly in the configuration file.");
                        return -1;
                    }
                }

                if (configuration.PositionState != null)
                {
                    Console.WriteLine($"Latitude: {configuration.PositionState.Latitude, -10} Logitude: {configuration.PositionState.Longitude, -10}");
                }

                if (sunsetMustBeAfterOption.HasValue())
                    configuration.SunsetMustBeAfter = sunsetMustBeAfterOption.ParsedValue;

                if (sunsetMustBeBeforeOption.HasValue())
                    configuration.SunsetMustBeBeBefore = sunsetMustBeBeforeOption.ParsedValue;

                if (sunriseMustBeAfterOption.HasValue())
                    configuration.SunriseMustBeAfter = sunriseMustBeAfterOption.ParsedValue;

                if (sunriseMustBeBeforeOption.HasValue())
                    configuration.SunriseMustBeBeBefore = sunriseMustBeBeforeOption.ParsedValue;

                if (dayColorTemperatureOption.HasValue())
                    configuration.DayColorTemperature = dayColorTemperatureOption.ParsedValue;

                if (nightColorTemperatureOption.HasValue())
                    configuration.NightColorTemperature = nightColorTemperatureOption.ParsedValue;

                if (pollingFrequencyOption.HasValue())
                    configuration.PollingFrequency = pollingFrequencyOption.ParsedValue;

                if (transitionTimeOption.HasValue())
                    configuration.TransitionTime = transitionTimeOption.ParsedValue;


                if (configuration.BridgeState == null)
                {
                    configuration.BridgeState = new BridgeState();
                }

                if (bridgeHostnameOption.HasValue())
                    configuration.BridgeState.BridgeHostname = bridgeHostnameOption.ParsedValue;

                if (string.IsNullOrEmpty(configuration.BridgeState.BridgeHostname))
                {
                    var locatedBridges = await DiscoverBridgesAsync(configuration);

                    if (locatedBridges.Count == 0)
                    {
                        Console.WriteLine("No bridges discovered on your network.");
                        return -1;
                    }

                    configuration.BridgeState.BridgeHostname = locatedBridges[0].IpAddress;
                }
                Console.WriteLine($"Bridge hostname: {configuration.BridgeState.BridgeHostname}");

                LocalHueClient hueClient = new LocalHueClient(configuration.BridgeState.BridgeHostname);
                if (string.IsNullOrEmpty(configuration.BridgeState.BridgeApiKey))
                {
                    bool hasSucceeded = false;
                    for (int i = 0; i < 10 && !hasSucceeded; i++)
                    {
                        try
                        {
                            configuration.BridgeState.BridgeApiKey = await hueClient.RegisterAsync("HueShift", "Bridge0");
                            hasSucceeded = true;
                        }
                        catch
                        {
                            Console.WriteLine("Failed to connect to Hue bridge!");
                            Console.WriteLine();
                        }

                        if (!hasSucceeded)
                        {
                            const double secondsBeforeRetrying = 10.0;
                            Console.WriteLine($"Failed to authorize. Make sure you pressed the button on the front of the Hue bridge. Trying again in {secondsBeforeRetrying}");
                            await Task.Delay(TimeSpan.FromSeconds(secondsBeforeRetrying));
                        }
                    }

                    if (!hasSucceeded)
                    {
                        Console.WriteLine("Bridge did not register! Rerun program and make sure you hit the button on the hue bridge.");
                        Console.WriteLine("If things still aren't working, then run the program with the --reset argument to clear everything and start fresh.");
                        Console.WriteLine("You can also manually put in the ip address of your bridge in your configuration file.");
                        return -1;
                    }
                }
                else
                {
                    hueClient.Initialize(configuration.BridgeState.BridgeApiKey);
                }
                Console.WriteLine("Saving");
                TrySaveConfiguration(doNotSaveConfigurationOption, configuration, configurationFileName);
                Console.WriteLine("Starting");

                await LightScheduler.ContinuallyEnforceLightTemperature(configuration, hueClient);

                return -1;
            });

            return app.Execute(args);
        }

        private static void TrySaveConfiguration(CommandOption noConfigurationSaveOption, Configuration configuration, string configurationFileName)
        {
            if (!noConfigurationSaveOption.HasValue())
            {
                File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(configuration, Formatting.Indented));
            }
        }

        private static async Task<List<LocatedBridge>> DiscoverBridgesAsync(Configuration configuration)
        {
            List<LocatedBridge> locatedBridges = new List<LocatedBridge>();

            Console.WriteLine("Locating through HTTP.");
            HttpBridgeLocator locator = new HttpBridgeLocator();
            var bridges = (await locator.LocateBridgesAsync(TimeSpan.FromSeconds(10))).ToList();

            locatedBridges.AddRange(bridges);


            if (locatedBridges.Count == 0)
            {
                Console.WriteLine("Http found nothing");
                // try
                // {
                //     Console.WriteLine("SSDP location attempt.");
                //     SSDPBridgeLocator locator = new SSDPBridgeLocator();
                //     var bridges = (await locator.LocateBridgesAsync(TimeSpan.FromSeconds(10))).ToList();

                //     locatedBridges.AddRange(bridges);
                // }
                // catch
                // {
                //     Console.WriteLine("SSDP Failed");
                // }
            }

            return locatedBridges;
        }
    }
}
