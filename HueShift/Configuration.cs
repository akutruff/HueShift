using System;

namespace HueShift
{
    public class Configuration
    {
        public TimeSpan TransitionTimeSpan = TimeSpan.FromSeconds(3);
        public TimeSpan PollingFrequency = TimeSpan.FromSeconds(10);
        public int DayColorTemperature = (int)ColorTemperature.Blue;
        public int NightColorTemperature = (int)ColorTemperature.Red;

        //public string BridgeIP = "10.10.201.6";
        public string BridgeIP = "hue.2k.lan";
        public string BridgeUri = "http://hue.2k.lan/api/";
        public string BridgeApiKey = "FWPwy-Xj2Ww7RSt1SaXrkRdhBc6M79IIKco2WouT";

        public string IpStackUri = "http://api.ipstack.com/check?access_key=";
        public string IpStackApiKey = "35c43096adc9416dab6bdd2d1ad53069";

        public TimeSpan SunriseMustBeAfter = new TimeSpan(0, 0, 0);
        public TimeSpan SunriseMustBeBeBefore = new TimeSpan(10, 0, 0);
        public TimeSpan SunsetMustBeAfter = new TimeSpan(18, 0, 0);
        public TimeSpan SunsetMustBeBeBefore = new TimeSpan(19, 30, 0);

        public double Latitude;
        public double Longitude;
    }
}
