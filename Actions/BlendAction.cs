using photomask.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class BlendAction : IAction
    {
        public IAction next_action { get; set; }

        private delegate int BlendMethod(int a, int b);

        private Dictionary<BlendMode, BlendMethod> BlendMethods { get; set; } = new Dictionary<BlendMode, BlendMethod>();

        public BlendAction()
        {
            BlendMethods[BlendMode.Diff] = Diff;
            BlendMethods[BlendMode.Multiply] = Multiply;
            BlendMethods[BlendMode.Divide] = Divide;
            BlendMethods[BlendMode.Overlay] = Overlay;
            BlendMethods[BlendMode.Exclusion] = Exclusion;
            BlendMethods[BlendMode.Screen] = Screen;
            BlendMethods[BlendMode.Normal] = Normal;
            BlendMethods[BlendMode.Subtraction] = Subtraction;
            BlendMethods[BlendMode.Sum] = Sum;
            BlendMethods[BlendMode.SoftLight] = SoftLight;
            BlendMethods[BlendMode.HardLight] = HardLight;
            BlendMethods[BlendMode.VividLight] = VividLight;
            BlendMethods[BlendMode.LinearLight] = LinearLight;
            BlendMethods[BlendMode.PinLight] = PinLight;
            BlendMethods[BlendMode.Jackal] = Jackal;
            BlendMethods[BlendMode.HardMix] = HardMix;
            BlendMethods[BlendMode.DarkenOnly] = DarkenOnly;
            BlendMethods[BlendMode.LightenOnly] = LightenOnly;
            BlendMethods[BlendMode.ColorDodge] = ColorDodge;
            BlendMethods[BlendMode.ColorBurn] = ColorBurn;
            BlendMethods[BlendMode.LinearBurn] = LinearBurn;
        }

        private int AlphaBlend(int a, int b, int alpha)
        {
            //float offset = (1.0f - alpha / 255.0f) * (a - b);
            //return Util.Clamp(b + (int)Math.Round(offset), 0, 255); // mb clamp isnt necessary
            int offset = (255 - alpha) * (a - b) / 255;
            return b + offset;
        }

        // for LighterColor, DarkerColor, Luminosity and Color I have to rewrite these function to support Pixel argument
        private int Overlay(int a, int b) => (a <= 127) ? (2 * a * b / 255) : (255 - 2 * (255 - a) * (255 - b) / 255);
        private int Diff(int a, int b) => Math.Abs(a - b);
        private int Exclusion(int a, int b) => Util.Clamp(a + b - 2 * a * b / 255, 0, 255);
        private int Screen(int a, int b) => 255 - (255 - a) * (255 - b) / 255;
        private int Subtraction(int a, int b) => Util.Clamp(a - b, 0, 255);
        private int Sum(int a, int b) => Util.Clamp(a + b, 0, 255); // same as linear dodge
        private int Multiply(int a, int b) => a * b / 255;
        private int Divide(int a, int b) => b == 0 ? 255 : Util.Clamp((int)(a / 255.0f / (b / 255.0f) * 255), 0, 255);
        private int Normal(int a, int b) => b;
        private int Jackal(int a, int b) => (255 - 2 * b) * a * a / 255; // шакаливание WIP
        private int SoftLight(int a, int b) => (255 - 2 * b) * a * a / 65025 + 2 * b * a / 255;
        private int HardLight(int a, int b) => Overlay(b, a);
        private int VividLight(int a, int b) => b <= 127 ? ColorBurn(a, 2 * b) : Divide(a, 2 * (255 - b));
        private int LinearLight(int a, int b) => LinearBurn(a, 2 * b);
        private int PinLight(int a, int b) => b <= 127 ? DarkenOnly(a, 2 * b) : Util.Clamp(LightenOnly(a, 2 * (b - 127)), 0, 255);
        private int HardMix(int a, int b) => a + b >= 255 ? 255 : 0;
        private int DarkenOnly(int a, int b) => a > b ? b : a;
        private int LightenOnly(int a, int b) => a > b ? a : b;
        private int ColorDodge(int a, int b) => Divide(a, 255 - b);
        private int LinearBurn(int a, int b) => Util.Clamp(a + b - 255, 0, 255);
        private int ColorBurn(int a, int b) => 255 - Divide(255 - a, b);

        public void DoAction(Img current_img, List<Img> masks)
        {
            BlendMethod currentMethod;
            for (var i = masks.Count() - 1; i >= 1; i--)
            {
                Img mask_top = masks[i];
                Img mask_bottom = masks[i - 1];

                // skip if nothing is changed
                /* WIP
                if (PrevOperationMasks.Count() == masks.Count() && PrevOperationMasks[i].Equals(mask_top) && PrevOperationMasks[i - 1].Equals(mask_bottom))
                {
                    //masks[i - 1] = PrevOperationMasks[i - 1].Clone() as ImageMask;
                    masks[i - 1].pixels_matrix = PrevOperationMasks[i - 1].pixels_matrix.Clone() as Pixel[,];
                    continue;
                }
                */

                int min_h = mask_top.height > mask_bottom.height ? mask_bottom.height : mask_top.height;
                int min_w = mask_top.width > mask_bottom.width ? mask_bottom.width : mask_top.width;

                // align to center
                int h_offset = Math.Abs(mask_top.height - mask_bottom.height) / 2;
                int w_offset = Math.Abs(mask_top.width - mask_bottom.width) / 2;

                int x_top_offset, x_bottom_offset, y_top_offset, y_bottom_offset;
                if (mask_top.width > mask_bottom.width)
                {
                    x_top_offset = w_offset;
                    x_bottom_offset = 0;
                }
                else
                {
                    x_top_offset = 0;
                    x_bottom_offset = w_offset;
                }

                if (mask_top.height > mask_bottom.height)
                {
                    y_top_offset = h_offset;
                    y_bottom_offset = 0;
                }
                else
                {
                    y_top_offset = 0;
                    y_bottom_offset = h_offset;
                }

                currentMethod = BlendMethods[mask_top.blend_data.mode];

                Parallel.For(0, min_w, x =>
                {
                    for (int y = 0; y < min_h; y++)
                    {
                        int x_top = x + x_top_offset;
                        int x_bottom = x + x_bottom_offset;
                        int y_top = y + y_top_offset;
                        int y_bottom = y + y_bottom_offset;

                        Pixel pix_top = mask_top.pixels_matrix[x_top, y_top];
                        Pixel pix_bottom = mask_bottom.pixels_matrix[x_bottom, y_bottom];

                        int R_top = currentMethod(pix_bottom.R, pix_top.R);
                        int G_top = currentMethod(pix_bottom.G, pix_top.G);
                        int B_top = currentMethod(pix_bottom.B, pix_top.B);

                        int alpha = (int)((mask_top.blend_data.opacity / 100.0f) * (pix_top.A / 255.0f) * 255);

                        int R_bottom = AlphaBlend(pix_bottom.R, R_top, alpha);
                        int G_bottom = AlphaBlend(pix_bottom.G, G_top, alpha);
                        int B_bottom = AlphaBlend(pix_bottom.B, B_top, alpha);

                        int new_alpha = pix_bottom.A == 0 ? alpha : pix_bottom.A;

                        masks[i - 1].pixels_matrix[x_bottom, y_bottom] = new Pixel(new_alpha, R_bottom, G_bottom, B_bottom);
                    }
                });
            }

            next_action?.DoAction(current_img, masks);
        }

    }
}
