using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public class FrequencyFilteringData : ICloneable
    {
        public object Clone()
        {
            FrequencyFilteringData data = new FrequencyFilteringData();
            return data;
        }
    }
}
