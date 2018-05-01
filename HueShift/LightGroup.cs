using System.Collections.Generic;

namespace HueShift
{
    public class LightGroup
    {
        public string name;
        public string type;
        public Action action;

        public class Action
        {
            public bool on;
            public int bri;
            public int hue;
            public int sat;
            public string effect;
            public List<double> xy;
            public int ct;
            public string alert; //"select"
            public string colormode; //"ct"
        }
    }
}
