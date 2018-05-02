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

                await LightScheduler.Main();
            });

            return app.Execute(args);
        }
    }
}
