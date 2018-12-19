using System;
using System.Collections.Generic;
using System.Numerics;

namespace HueShift
{
    public class BridgeState
    {
        public string BridgeHostname;
        public string BridgeApiKey;
        public string BridgeEntertainmentClientKey;
    }

    public class PositionState
    {
        public double Latitude;
        public double Longitude;
    }

    enum WhiteAmbianceChannel
    {
        IsOn, // 0 value is off otherwise non-zero means the light is on
        Intensity, 
        Temperature,
    }

    public class DMXConfiguration
    {
        public bool IsEnabled = true;
        public string ListeningIPAddress = null;
        public int Universe = 1;
        public int StartingChannel = 1;

        public TimeSpan TransitionTime = TimeSpan.FromMilliseconds(200);
        public TimeSpan MinWaitDurationBetweenApiCalls = TimeSpan.FromSeconds(1);
        public int MillisToSleepBetweenQueueChecks = 12;

        public List<int> LightIdsInDmxGroup = new List<int>();

        public TimeSpan TimeAfterLastDmxPacketToReturnToShifting = TimeSpan.FromMinutes(1);
    }

    public class Configuration
    {
        public TimeSpan TransitionTime = TimeSpan.FromSeconds(5);
        public TimeSpan PollingFrequency = TimeSpan.FromSeconds(6);

        public int DayColorTemperature = (int)ColorTemperature.Blue;
        public int NightColorTemperature = (int)ColorTemperature.Red;

        public BridgeState BridgeState = null;
        public PositionState PositionState = null;

        public string IpStackUri = "http://api.ipstack.com/check?access_key=";
        public string IpStackApiKey = "35c43096adc9416dab6bdd2d1ad53069"; //Yes this is here.  If someone decides to be a jerk, manual entry is supported.

        public TimeSpan SunriseMustBeAfter = new TimeSpan(0, 0, 0);
        public TimeSpan SunriseMustBeBeBefore = new TimeSpan(10, 0, 0);
        public TimeSpan SunsetMustBeAfter = new TimeSpan(18, 0, 0);
        public TimeSpan SunsetMustBeBeBefore = new TimeSpan(19, 30, 0);

        public List<string> IdsOfLightsToExclude = new List<string>();

        public DMXConfiguration DMXConfiguration = null;
    }
}
