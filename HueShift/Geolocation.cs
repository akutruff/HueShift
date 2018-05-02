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
    public class Geolocation
    {
        public static (double latitude, double longitude) GetLocationFromIPAddress(Configuration configuration)
        {
            var geolocationURi = new Uri(configuration.IpStackUri + configuration.IpStackApiKey);
            var geolocationResponse = Http.Get(geolocationURi);
            dynamic response = JObject.Parse(geolocationResponse);

            return (
                (double)response.latitude,
                (double)response.longitude);
        }
    }
}