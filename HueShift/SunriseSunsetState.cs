using System;
using System.Collections.Generic;
using Q42.HueApi;

namespace HueShift
{
    public class AppState
    {
        public DateTimeOffset LastRunTime = DateTimeOffset.MinValue;
        public DateTimeOffset LastRequestedSlowTransitionTime = DateTimeOffset.MinValue;

        public Dictionary<string, LightControlStatus> Lights { get; } = new Dictionary<string, LightControlStatus>();
    }
}
