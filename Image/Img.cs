using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace photomask.Image
{

    public class Img : ICloneable
    {
        private Bitmap bitmap { get; set; }
        public BitmapSource ImageSource { get; private set; }
        public long Id { get; private set; }
        public BlendData blend_data { get; private set; } = new BlendData();
        public CurvingData curving_data { get; private set; } = new CurvingData();
        public FilteringData filtering_data { get; private set; } = new FilteringData();
        public BinarizationData binarization_data { get; private set; } = new BinarizationData();
        public Pixel[,] pixels_matrix { get; set; }
        //public Pixel[,] operated_pixels_matrix { get; set; }
        public bool keep_aspect_ratio { get; set; } = true;

        private int _height;
        private int _width;

        private int _width_view;
        private int _height_view;

        public Img(string path)
        {
            bitmap = new Bitmap(path);
            Id = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            ImageSource = Util.GetImageSourceAnyFormat(bitmap);

            width = bitmap.Width;
            height = bitmap.Height;

            SetPixelsMatrix(bitmap);
            //curving_data.SetGistoPoints(pixels_matrix);
        }

        public Img() { }

        ~Img()
        {
            bitmap?.Dispose();
        }

        public int height
        {
            get => _height;
            private set
            {
                _height_view = value;
                _height = value;
            }
        }
        public int width {
            get => _width;
            private set
            {
                _width_view = value;
                _width = value;
            }
        }
        public int width_view
        {
            get => _width_view;
            set
            {
                if (keep_aspect_ratio)
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
                if (keep_aspect_ratio)
                {
                    float ratio = (float)width / (float)height;
                    _width_view = (int)Math.Round(value * ratio);
                }
                _height_view = value;
            }
        }

        // Cloning with missing this.bitmap 
        public object Clone()
        {
            Img mask = new Img();
            mask.Id = Id;
            mask.blend_data = blend_data.Clone() as BlendData;
            mask.curving_data = curving_data.Clone() as CurvingData;
            mask.filtering_data = filtering_data.Clone() as FilteringData;
            mask.binarization_data = binarization_data.Clone() as BinarizationData;
            mask.width = width;
            mask.height = height;
            mask.pixels_matrix = pixels_matrix.Clone() as Pixel[,];
            return mask;
        }

        // WIP
        /*
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Img another = obj as Img;
            return (another.Id == Id) && (another.method == method) && (another.width == width) && (another.height == height) && (another.opacity == opacity);
        }
        */

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
            
            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; y++)
                {
                    int offset = ((y * width) + x) * 4;
                    pixels_matrix[x, y] = new Pixel(colors[offset + 3], colors[offset + 2], colors[offset + 1], colors[offset]);
                }
            });
            image.UnlockBits(bitmapData);

            //operated_pixels_matrix = pixels_matrix.Clone() as Pixel[,];
        }
    }
}
