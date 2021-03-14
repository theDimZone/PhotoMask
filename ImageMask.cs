using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace photomask
{
    public enum Method
    {
        None,
        Standart,
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

    public class ImageMask : ICloneable
    {
        private Bitmap bitmap { get; set; }

        private int _height;
        public int height
        {
            get => _height;
            private set
            {
                _height_view = value;
                _height = value;
            }
        }
        private int _width;
        public int width {
            get => _width;
            private set
            {
                _width_view = value;
                _width = value;
            }
        }
        public Pixel[,] pixels_matrix { get; private set; }
        public int opacity { get; set; } = 100; 
        public Method method { get; set; } = Method.None;
        
        public int method_view
        {
            get => (int)method;
            set => method = (Method)value;
        }

        public bool keepAspectRatio { get; set; } = true;

        private int _width_view;
        private int _height_view;
        public int width_view
        {
            get => _width_view;
            set
            {
                if (keepAspectRatio)
                {
                    float ratio = (float)width / (float)height;
                    _height_view = (int)Math.Round(value / ratio);
                }

                _width_view = value;
            }
         }
        public int height_view
        {
            get => _height_view;
            set
            {
                if (keepAspectRatio)
                {
                    float ratio = (float)width / (float)height;
                    _width_view = (int)Math.Round(value * ratio);
                }
                _height_view = value;
            }
        }


        public BitmapImage ImageSource {
            get => Util.GetImageSource(bitmap);
        }

        public ImageMask(string path)
        {
            bitmap = new Bitmap(path);

            width = bitmap.Width;
            height = bitmap.Height;
           
            SetPixelsMatrix(bitmap);
        }

        public ImageMask() { }

        ~ImageMask()
        {
            bitmap?.Dispose();
        }

        // Cloning with missing this.bitmap 
        public object Clone()
        {
            ImageMask mask = new ImageMask();
            mask.opacity = opacity;
            mask.method = method;
            mask.width = width;
            mask.height = height;
            mask.pixels_matrix = pixels_matrix.Clone() as Pixel[,];
            return mask;
        }

        public void Resize(int w, int h)
        {
            var destRect = new Rectangle(0, 0, w, h);
            using var destImage = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            destImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();   
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(bitmap, destRect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, wrapMode);

            width = w;
            height = h;

            SetPixelsMatrix(destImage);
        }

        public void ResizeToOriginal()
        {
            width = bitmap.Width;
            height = bitmap.Height;

            SetPixelsMatrix(bitmap);
        }

        private void SetPixelsMatrix(Bitmap image)
        {
            pixels_matrix = new Pixel[width, height];
            byte[] colors = new byte[width * height * 4];

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            IntPtr Iptr = bitmapData.Scan0;
            Marshal.Copy(Iptr, colors, 0, colors.Length);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int offset = ((y * width) + x) * 4;
                    pixels_matrix[x, y] = new Pixel(colors[offset + 3], colors[offset + 2], colors[offset + 1], colors[offset]);
                }
            }
            image.UnlockBits(bitmapData);

        }
    }
}
