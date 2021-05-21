using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public enum SpatialFilteringMode
    {
        None,
        Linear,
        Median
    }

    public class SpatialFilteringData : ICloneable
    {
        public SpatialFilteringMode mode { get; set; } = SpatialFilteringMode.None;
        public double[,] kernel { get; set; } = new double[0, 0];
        public int median_radius { get; set; } = 5;
        public int mode_view
        {
            get => (int)mode;
            set => mode = (SpatialFilteringMode)value;
        }

        public string kernel_view { get; set; } = "";
        public object Clone()
        {
            SpatialFilteringData data = new SpatialFilteringData();
            data.mode = mode;
            data.kernel = kernel.Clone() as double[,];
            data.median_radius = median_radius;
            data.kernel_view = kernel_view;
            return data;
        }
    }
}
