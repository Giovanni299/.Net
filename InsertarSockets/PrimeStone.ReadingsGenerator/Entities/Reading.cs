using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primestone.ReadingsGenerator.Entities
{
    class Reading
    {
        public string DeviceId { get; set; }

        public DateTime Date { get; set; }

        public double Value { get; set; }

        public int Log { get; set; }

        public int channel { get; set; }
    }
}
