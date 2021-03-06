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
using System.Numerics;

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

        private static int partition<T>(T[] arr, int left, int right, int pivot_index) where T : IComparable<T>
        {
            T pivot_value = arr[pivot_index];
            (arr[pivot_index], arr[right]) = (arr[right], arr[pivot_index]);
            int store_index = left;
            for(int i = left; i < right; i++)
            {
                if(arr[i].CompareTo(pivot_value) < 0)
                {
                    (arr[store_index], arr[i]) = (arr[i], arr[store_index]);
                    store_index++;
                }
            }
            (arr[right], arr[store_index]) = (arr[store_index], arr[right]);
            return store_index;
        }

        private static T quick_select<T>(T[] arr, int left, int right, int k) where T : IComparable<T>
        {
            int pivot_index;
            Random rand = new Random();
            while(true)
            {
                if (left == right) return arr[left];
                pivot_index = rand.Next(left, right + 1); // rand between left and right
                pivot_index = partition(arr, left, right, pivot_index);
                if(k == pivot_index)
                {
                    return arr[k];
                } else if(k < pivot_index)
                {
                    right = pivot_index - 1;
                } else
                {
                    left = pivot_index + 1;
                }
            }
        }

        public static T QuickSelect<T>(T[] arr, int k) where T : IComparable<T>
        {
            return quick_select(arr, 0, arr.Length - 1, k);
        }

        // radix-2 Cooley-Turkey https://en.wikipedia.org/wiki/Cooley%E2%80%93Tukey_FFT_algorithm
        public static Complex[] ditfft2(Complex[] vec, int N, int s = 1, int mul = 1)
        {
            Complex[] X = new Complex[N];

            if(N == 1)
            {
                //X[0] = new Complex(vec[0], 0);
                X[0] = vec[0];
            } else
            {
                //X[0..(N / 2 - 1)]
                //ditfft2_int(vec, N / 2, 2 * s);
                Complex[] vec1 = vec.Where((com, i) => i % 2 == 0).ToArray();
                Complex[] vec2 = vec.Where((com, i) => i % 2 != 0).ToArray();

                Array.Copy(ditfft2(vec1, N / 2, 2 * s, mul), 0, X, 0, N / 2); 
                Array.Copy(ditfft2(vec2, N / 2, 2 * s, mul), 0, X, N / 2, N / 2);

                for (int k = 0; k < N / 2; k++)
                {
                    Complex p = X[k];
                    double a = mul * -2.0d * Math.PI / N * k;
                    Complex q = new Complex(Math.Cos(a), Math.Sin(a)) * X[k + N / 2];
                    X[k] = p + q;
                    X[k + N / 2] = p - q;
                }
            }

            return X;
        }
    }
}
