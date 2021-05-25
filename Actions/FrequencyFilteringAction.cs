using photomask.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class FrequencyFilteringAction : IAction
    {
        public IAction next_action { get; set; }

        public void DoAction(Img current_img, List<Img> images)
        {
            Parallel.For(0, images.Count, i =>
            {
                if (images[i].frequency_filtering_data.mode == FrequencyFilteringMode.None) return;

                if (images[i].frequency_filtering_data.old_height != images[i].pixels_matrix.GetLength(1)
                 || images[i].frequency_filtering_data.old_width != images[i].pixels_matrix.GetLength(0))
                {
                    images[i].frequency_filtering_data.ClearFourier();
                    return;
                }

                Filter(images[i]);
            });

            next_action?.DoAction(current_img, images);
        }

        private void Filter(Img image)
        {
            int spectrum_w = image.frequency_filtering_data.fourier_width;
            int spectrum_h = image.frequency_filtering_data.fourier_height;
            int circles_len = image.frequency_filtering_data.circles.Length;

            double[][] circles = new double[circles_len][];
            for (int i = 0; i < circles_len; i++)
            {
                circles[i] = new double[4];
                circles[i][0] = image.frequency_filtering_data.circles[i][0] + spectrum_w / 2;
                circles[i][1] = spectrum_h / 2 - image.frequency_filtering_data.circles[i][1];
                circles[i][2] = image.frequency_filtering_data.circles[i][2];
                circles[i][3] = image.frequency_filtering_data.circles[i][3];
            }

            Complex[][][] temp = new Complex[3][][];
            for (int c = 0; c < 3; c++) temp[c] = new Complex[spectrum_h][];

            Parallel.For(0, spectrum_h, y =>
            {
                for (int c = 0; c < 3; c++) temp[c][y] = new Complex[spectrum_w];

                for (int x = 0; x < spectrum_w; x++)
                {
                    double mul = 1.0d;
                    for (int i = 0; i < circles_len; i++)
                    {
                        double[] circle = circles[i];

                        // TODO: insert mode functions there
                        if ((x - circle[0]) * (x - circle[0]) + (y - circle[1]) * (y - circle[1]) >= circle[2] * circle[2]
                            && (x - circle[0]) * (x - circle[0]) + (y - circle[1]) * (y - circle[1]) <= circle[3] * circle[3])
                        {
                            mul = image.frequency_filtering_data.inner_multiplier;
                            break;
                        }
                        else
                        {
                            mul = image.frequency_filtering_data.rest_multiplier;
                        }
                    }

                    for (int c = 0; c < 3; c++)
                    {
                        temp[c][y][x] = image.frequency_filtering_data.fourier_spectrum[c][y][x] * mul;
                    }
                }

                for (int c = 0; c < 3; c++)
                {
                    Complex[] transformed = Util.ditfft2(vec: temp[c][y], N: spectrum_w, mul: -1);
                    temp[c][y] = transformed;
                }
            });

            Parallel.For(0, spectrum_w, x =>
            {
                Complex[][] col = new Complex[3][];
                for (int c = 0; c < 3; c++) col[c] = new Complex[spectrum_h];

                for (int y = 0; y < spectrum_h; y++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        col[c][y] = temp[c][y][x];
                    }
                }

                for (int c = 0; c < 3; c++)
                {
                    Complex[] transformed = Util.ditfft2(vec: col[c], N: spectrum_h, mul: -1);
                    for (int y = 0; y < spectrum_h; y++)
                    {
                        temp[c][y][x] = transformed[y];
                    }
                }

            });

            Parallel.For(0, image.height, y =>
            {
                for(int x = 0; x < image.width; x++)
                {
                    double pow = Math.Pow(-1, x + y);

                    byte a = image.pixels_matrix[x, y].A;
                    byte r = (byte)Util.Clamp((temp[2][y][x] * pow).Real, 0, 255);
                    byte g = (byte)Util.Clamp((temp[1][y][x] * pow).Real, 0, 255);
                    byte b = (byte)Util.Clamp((temp[0][y][x] * pow).Real, 0, 255);

                    image.pixels_matrix[x, y] = new Pixel(a, r, g, b);
                }
            });
        }

    }
}
