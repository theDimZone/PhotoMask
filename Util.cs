using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Diagnostics;

namespace photomask
{
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public static class Util
    {
        // https://stackoverflow.com/questions/1546091/wpf-createbitmapsourcefromhbitmap-memory-leak
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
                
        public static BitmapSource GetImageSourceAnyFormat(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            var hBitmap = bitmap.GetHbitmap();
            var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, 
                IntPtr.Zero, 
                System.Windows.Int32Rect.Empty, 
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);

            return result;
        }
        


        // https://stackoverflow.com/questions/45263691/wpf-bitmapimage-creation-extremely-slow-on-xeon-nvidia-quadro-machine
        public static BitmapSource GetImageSourceResultFormat(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            
            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }
        
        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static bool isNatural(string str)
        {
            int i;
            return Int32.TryParse(str, out i) && !str.Contains("-");
        }
    }
}
