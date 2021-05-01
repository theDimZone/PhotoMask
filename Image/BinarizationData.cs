using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public enum BinarizationMode
    {
        None,
        Gavrilov,
        Otsu,
        Niblack,
        Sauvola,
        Wolf,
        Bradley,
        Kabarukhin
    }

    public class BinarizationData : ICloneable
    {
        public BinarizationMode mode { get; set; } = BinarizationMode.None;
        public int windows_size { get; set; } = 15;
        public double param { get; set; } = 0.0d;
        public Dictionary<BinarizationMode, double> default_params { get; private set; } = new Dictionary<BinarizationMode, double>();
        public int mode_view
        {
            get => (int)mode;
            set => mode = (BinarizationMode)value;
        }

        public BinarizationData()
        {
            default_params[BinarizationMode.Niblack] = -0.2d;
            default_params[BinarizationMode.Sauvola] = 0.25d;
            default_params[BinarizationMode.Wolf] = 0.5d;
            default_params[BinarizationMode.Bradley] = 0.15d;
        }

        public object Clone()
        {
            BinarizationData data = new BinarizationData();
            data.mode = mode;
            data.param = param;
            data.windows_size = windows_size;
            return data;
        }
    }
}
