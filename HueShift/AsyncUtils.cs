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
    public static class AsyncUtils
    {
        public static async Task<T> Retry<T>(Func<T> func, double seconds = 30)
        {
            bool hasBeenReported = false;

            do
            {
                try
                {
                    return func();
                }
                catch (Exception e)
                {
                    if (!hasBeenReported)
                    {
                        Console.WriteLine(e);
                        hasBeenReported = true;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(seconds));
            } while (true);
        }

        public static async Task<T> Retry<T>(Func<Task<T>> func, double seconds = 30)
        {
            bool hasBeenReported = false;

            do
            {
                try
                {
                    return await func();
                }
                catch (Exception e)
                {
                    if (!hasBeenReported)
                    {
                        Console.WriteLine(e);
                        hasBeenReported = true;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(seconds));
            } while (true);
        }
    }
}