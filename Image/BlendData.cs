using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public enum BlendMode
    {
        None,
        Normal,
        Jackal,
        Sum,
        Subtraction,
        Multiply,
        Divide,
        Screen,
        Diff,
        Overlay,
        Exclusion,
        SoftLight,
        HardLight,
        VividLight,
        LinearLight,
        PinLight,
        HardMix,
        DarkenOnly,
        LightenOnly,
        ColorDodge,
        ColorBurn,
        LinearBurn
    }

    public class BlendData : ICloneable
    {
        public int opacity { get; set; } = 100;
        public BlendMode mode { get; set; } = BlendMode.None;
        public int mode_view
        {
            get => (int)mode;
            set => mode = (BlendMode)value;
        }

        public object Clone()
        {
            BlendData data = new BlendData();
            data.opacity = opacity;
            data.mode = mode;

            return data;
        }
    }
}
