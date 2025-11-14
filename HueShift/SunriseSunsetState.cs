using System;
using System.Collections.Generic;

namespace HueShift
{
    public class AppState
    {
        public DateTimeOffset LastRunTime = DateTimeOffset.MinValue;
        public DateTimeOffset LastRequestedSlowTransitionTime = DateTimeOffset.MinValue;

        public Dictionary<Guid, LightControlStatus> Lights { get; } = new Dictionary<Guid, LightControlStatus>();
    }
}
