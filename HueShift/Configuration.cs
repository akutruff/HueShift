using System;
using System.Numerics;

namespace HueShift
{
    // public class BridgeState
    // {
    //     public string BridgeHostname = "hue.2k.lan";
    //     public string BridgeUri = "http://hue.2k.lan/api/";
    //     public string BridgeApiKey = "FWPwy-Xj2Ww7RSt1SaXrkRdhBc6M79IIKco2WouT";
    // }

    public class BridgeState
    {
        public string BridgeHostname;
        public string BridgeUri;
        public string BridgeApiKey;
    }

    public class PositionState
    {
        public double Latitude;
        public double Longitude;
    }

    public class Configuration
    {
        public TimeSpan TransitionTime = TimeSpan.FromSeconds(3);
        public TimeSpan PollingFrequency = TimeSpan.FromSeconds(10);
        public int DayColorTemperature = (int)ColorTemperature.Blue;
        public int NightColorTemperature = (int)ColorTemperature.Red;

        public BridgeState BridgeState = null;
        public PositionState PositionState = null;

        public string IpStackUri = "http://api.ipstack.com/check?access_key=";
        public string IpStackApiKey = "35c43096adc9416dab6bdd2d1ad53069";

        public TimeSpan SunriseMustBeAfter = new TimeSpan(0, 0, 0);
        public TimeSpan SunriseMustBeBeBefore = new TimeSpan(10, 0, 0);
        public TimeSpan SunsetMustBeAfter = new TimeSpan(18, 0, 0);
        public TimeSpan SunsetMustBeBeBefore = new TimeSpan(19, 30, 0);
    }
}
