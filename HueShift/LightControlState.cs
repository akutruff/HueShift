using System;
using Q42.HueApi;

namespace HueShift
{

    public enum LightControlState
    {
        HueShiftAutomated,
        ManualControl,
        Transitioning,
        Off
    }  

    public class LightControlStatus
    {
        public DateTimeOffset DetectedOnTime;
        public DateTimeOffset HueShiftTookControlTime;
        public DateTimeOffset DetectedManualChangeTime;
        public DateTimeOffset TransitionRequestedTime;
        public TimeSpan RequestedTransitionDuration;
        public LightControlState LightControlState;
        public Light Light;

        public LightControlStatus(DateTimeOffset detectedOnTime, DateTimeOffset hueShiftTookControlTime, DateTimeOffset detectedManualChangeTime, DateTimeOffset transitionRequestedTime, TimeSpan requestedTransitionDuration, LightControlState lightControlState, Light light)
        {
            DetectedOnTime = detectedOnTime;
            HueShiftTookControlTime = hueShiftTookControlTime;
            DetectedManualChangeTime = detectedManualChangeTime;
            TransitionRequestedTime = transitionRequestedTime;
            RequestedTransitionDuration = requestedTransitionDuration;
            LightControlState = lightControlState;
            Light = light;
        }
    }
}
