using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.CustomSmartObjectJson
{
    public class MicControlData
    {
        public string gaugeLevel { get; set; }

        public ushort GaugeJoin { get; set; }

        public ushort joinNumber { get; set; }

        public bool state { get; set; }

        public string ActionName { get; set; }

        public string Name { get; set; }
    }
}
