using photomask.Image;
using photomask.Static;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class WriteAction : IAction
    {
        public IAction next_action { get; set; }

        public void DoAction(Img current_img, List<Img> masks)
        {
            // write result
            int w = masks[0].width;
            int h = masks[0].height;
            byte[] colors = new byte[w * h * 4];

            Rectangle rect = new Rectangle(0, 0, w, h);
            Images.result_bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = Images.result_bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr Iptr = bitmapData.Scan0;

            Parallel.For(0, w, x =>
            {
                for (int y = 0; y < h; y++)
                {
                    Pixel pix = masks[0].pixels_matrix[x, y];
                    byte A = (byte)((masks[0].blend_data.opacity / 100.0f) * (pix.A / 255.0f) * 255);

                    int offset = ((y * w) + x) * 4;
                    colors[offset] = pix.B;
                    colors[offset + 1] = pix.G;
                    colors[offset + 2] = pix.R;
                    colors[offset + 3] = A;
                }
            });
            Marshal.Copy(colors, 0, Iptr, colors.Length);
            Images.result_bitmap.UnlockBits(bitmapData);

            next_action?.DoAction(current_img, masks);
        }
    }
}
