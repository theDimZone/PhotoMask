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
        Bradley
    }

    public class BinarizationData : ICloneable
    {
        public BinarizationMode mode { get; set; } = BinarizationMode.None;
        public int windows_size { get; set; } = 15;
        /*
        public float niblack_param { get; set; } = -0.2f;
        public float sauvola_param { get; set; } = 0.25f;
        public float wolf_param { get; set; } = 0.5f;
        public float bradley_param { get; set; } = 0.15f;
        */
        public float param { get; set; } = 0.0f;
        public Dictionary<BinarizationMode, float> default_params { get; private set; } = new Dictionary<BinarizationMode, float>();
        public int mode_view
        {
            get => (int)mode;
            set => mode = (BinarizationMode)value;
        }

        public BinarizationData()
        {
            default_params[BinarizationMode.Niblack] = -0.2f;
            default_params[BinarizationMode.Sauvola] = 0.25f;
            default_params[BinarizationMode.Wolf] = 0.5f;
            default_params[BinarizationMode.Bradley] = 0.15f;
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
