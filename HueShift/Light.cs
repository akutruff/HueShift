namespace HueShift
{
    public class Light
    {
        public class LightState
        {
            public bool on;
            public bool reachable;

            public int bri;
            public int ct;
        }
        public LightState state;
    }
}
