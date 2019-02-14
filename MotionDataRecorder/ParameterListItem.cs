using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDataRecorder
{
    public class ParameterListItem
    {
        public int Joint { get; set; }
        public string Type { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }

        public ParameterListItem(int joint, string type, double max, double min)
        {
            Joint = joint;
            Type = type;
            Max = max;
            Min = min;
        }
    }
}
