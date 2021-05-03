using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public enum FilteringMode
    {
        None,
        Linear,
        Median
    }

    public class FilteringData : ICloneable
    {
        public FilteringMode mode { get; set; } = FilteringMode.None;
        public double[,] kernel { get; set; } = new double[0, 0];
        public object Clone()
        {
            FilteringData data = new FilteringData();
            data.mode = mode;
            data.kernel = kernel.Clone() as double[,];
            return data;
        }
    }
}
