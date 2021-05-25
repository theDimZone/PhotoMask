using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public enum FrequencyFilteringMode
    {
        None,
        Ideal
    }

    public class FrequencyFilteringData : ICloneable
    {
        public FrequencyFilteringMode mode { get; set; } = FrequencyFilteringMode.None;

        //public Complex[,,] fourier_spectrum { get; set; } = null; // for each color channel -> [c, x, y]
        public Complex[][][] fourier_spectrum { get; set; } = new Complex[3][][]; // for each color channel -> [c][y][x]
        public int fourier_width { get; set; } = 0;
        public int fourier_height { get; set; } = 0;
        public int old_width { get; set; } = 0;
        public int old_height { get; set; } = 0;
        public Bitmap fourier_image { get; set; } = null;
        public double inner_multiplier { get; set; } = 1.0d;
        public double rest_multiplier { get; set; } = 0.0d;
        public double[][] circles { get; set; } = new double[0][];
        public string circles_view { get; set; } = "";
        public int mode_view
        {
            get => (int)mode;
            set => mode = (FrequencyFilteringMode)value;
        }

        /*
        public string circles_view
        {
            get => circles.Aggregate("", (acc, line) => acc += line.Aggregate("", (l_acc, dig) => l_acc += dig.ToString() + ";") + Environment.NewLine);
        }
        */

        ~FrequencyFilteringData()
        {
            ClearFourier();
        }

        public void ClearFourier()
        {
            fourier_spectrum = null;
            fourier_image?.Dispose();
            fourier_image = null;
            mode = FrequencyFilteringMode.None;
        }

        public void GenerateSpectrum(Pixel[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            old_width = width;
            old_height = height;
            // new width and height? to radix 2
            fourier_width = (int)Math.Pow(2, Math.Ceiling(Math.Log2(width)));
            fourier_height = (int)Math.Pow(2, Math.Ceiling(Math.Log2(height)));

            // jagged 
            ClearFourier();
            fourier_spectrum = new Complex[3][][];

            byte[] colors = new byte[fourier_width * fourier_height * 3];

            Rectangle rect = new Rectangle(0, 0, fourier_width, fourier_height);
            fourier_image = new Bitmap(fourier_width, fourier_height, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = fourier_image.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr Iptr = bitmapData.Scan0;

            for (int c = 0; c < 3; c++) fourier_spectrum[c] = new Complex[fourier_height][];
            
            Parallel.For(0, fourier_height, y =>
            {
                for (int c = 0; c < 3; c++) fourier_spectrum[c][y] = new Complex[fourier_width];

                for (int x = 0; x < fourier_width; x++)
                {
                    int[] channels;
                    if (x >= width || y >= height)
                    {
                        channels = new int[3];
                    }
                    else
                    {
                        channels = pixels[x, y].ToArray();
                    }

                    double pow = Math.Pow(-1, x + y);

                    for (int c = 0; c < 3; c++)
                    {
                        fourier_spectrum[c][y][x] = new Complex(channels[c] * pow, 0);
                    }
                }

                for (int c = 0; c < 3; c++)
                {
                    Complex[] transformed = Util.ditfft2(fourier_spectrum[c][y], fourier_width);
                    for (int x = 0; x < fourier_width; x++) transformed[x] /= fourier_width;
                    fourier_spectrum[c][y] = transformed;
                }
            });

            double[] max_ln = new double[fourier_width];
            Parallel.For(0, fourier_width, x =>
            {
                Complex[][] col = new Complex[3][];
                for (int c = 0; c < 3; c++) col[c] = new Complex[fourier_height];

                for (int y = 0; y < fourier_height; y++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        col[c][y] = fourier_spectrum[c][y][x];
                    }
                }

                for (int c = 0; c < 3; c++)
                {
                    Complex[] transformed = Util.ditfft2(col[c], fourier_height);
                    for (int y = 0; y < fourier_height; y++)
                    {
                        fourier_spectrum[c][y][x] = transformed[y] / fourier_height;

                        double ln = Math.Log(fourier_spectrum[c][y][x].Magnitude + 1);
                        if (ln > max_ln[x]) max_ln[x] = ln;
                    }
                }

            });

            double max = max_ln.Max();
            double k = 255.0d / max * 5; // TODO: custom input for multiplier
            Debug.WriteLine(max);

            Parallel.For(0, fourier_height, y =>
            {
                for(int x = 0; x < fourier_width; x++)
                {
                    for(int c = 0; c < 3; c++)
                    {
                        int offset = ((y * fourier_width) + x) * 3;
                        colors[offset + c] = (byte)Util.Clamp(Math.Log(fourier_spectrum[c][y][x].Magnitude + 1) * k, 0, 255);
                    }
                }
            });

            Marshal.Copy(colors, 0, Iptr, colors.Length);
            fourier_image.UnlockBits(bitmapData);
        }

        public object Clone()
        {
            FrequencyFilteringData data = new FrequencyFilteringData();
            data.mode = mode;
            data.fourier_spectrum = fourier_spectrum; // <- clone is useless, careful...
            data.inner_multiplier = inner_multiplier;
            data.rest_multiplier = rest_multiplier;
            data.fourier_width = fourier_width;
            data.fourier_height = fourier_height;
            data.circles = circles; // <- same
            data.old_width = old_width;
            data.old_height = old_height;
            return data;
        }
    }
}
