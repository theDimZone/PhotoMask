using photomask.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace photomask.Actions
{
    /*
     * TODO: minimize count of boilerplate...
     */
    public class BinarizationAction : IAction
    {
        public IAction next_action { get; set; }

        private delegate void BinarizationMethod(Img editing_image);
        private Dictionary<BinarizationMode, BinarizationMethod> binarization_methods = new Dictionary<BinarizationMode, BinarizationMethod>();
        public BinarizationAction()
        {
            binarization_methods[BinarizationMode.Gavrilov] = Gavrilov;
            binarization_methods[BinarizationMode.Otsu] = Otsu;
            binarization_methods[BinarizationMode.Niblack] = Niblack;
            binarization_methods[BinarizationMode.Sauvola] = Sauvola;
            binarization_methods[BinarizationMode.Wolf] = Wolf;
            binarization_methods[BinarizationMode.Bradley] = Bradley;
        }

        public void DoAction(Img current_img, List<Img> images)
        {
            Parallel.For(0, images.Count, i =>
            {
                if (images[i].binarization_data.mode == BinarizationMode.None) return;

                binarization_methods[images[i].binarization_data.mode](images[i]);
            });

            next_action?.DoAction(current_img, images);
        }

        private int toGreyscale(Pixel pix)
        {
            return (byte)(0.2125 * pix.R + 0.7154 * pix.G + 0.0721 * pix.B);
        }

        private double toGreyscaleDouble(Pixel pix)
        {
            return 0.2125d * pix.R + 0.7154d * pix.G + 0.0721d * pix.B;
        }

        private void GlobalMethod(Img editing_image, int t)
        {
            Parallel.For(0, editing_image.width, x =>
            {
                for (int y = 0; y < editing_image.height; y++)
                {
                    int res = toGreyscale(editing_image.pixels_matrix[x, y]) <= t ? 0 : 255;
                    editing_image.pixels_matrix[x, y] = new Pixel(editing_image.pixels_matrix[x, y].A, res, res, res);
                }
            });
        }

        #region Global Methods
        private void Gavrilov(Img editing_image)
        {
            int[,] integrated_image = getIntegrated(editing_image.pixels_matrix);
            int t = integrated_image[editing_image.width - 1, editing_image.height - 1] / (editing_image.width * editing_image.height);

            GlobalMethod(editing_image, t);
        }

        private void Otsu(Img editing_image)
        {
            double[] norm_gisto = buildNormGisto(editing_image.pixels_matrix);

            int t = 0;
            double sigma_max = 0;

            double l_max = 256;
            for (int i = 255; i >= 0; i--)
            {
                if (norm_gisto[i] == 0.0d)
                    l_max = i;
                else
                    break;
            }

            double u_T = 0;
            for (int i = 0; i < l_max; i++)
            {
                u_T += i * norm_gisto[i];
            }

            double N_sum = 0.0d;
            double Ni_sum = 0.0d;
            for (int i = 0; i < l_max; i++)
            {
                double omega1 = N_sum;
                double omega2 = 1.0d - omega1;

                double u1 = Ni_sum / omega1;
                double u2 = (u_T - u1 * omega1) / omega2;

                double sigma = omega1 * omega2 * Math.Pow(u1 - u2, 2);
                if (sigma > sigma_max)
                {
                    sigma_max = sigma;
                    t = i;
                }

                N_sum += norm_gisto[i];
                Ni_sum += i * norm_gisto[i];
            }

            GlobalMethod(editing_image, t);
        }

        private double[] buildNormGisto(Pixel[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            int[] pix_count = new int[256];
            double[] norm_gisto = new double[256];

            Parallel.For<int[]>(0, w, () => (new int[256]), (x, loop, subtotal) =>
            {
                for (int y = 0; y < h; y++)
                {
                    int a = toGreyscale(matrix[x, y]);

                    subtotal[a]++;
                }
                return subtotal;
            },
                (arr) =>
                {
                    for (int i = 0; i < 256; i++) Interlocked.Add(ref pix_count[i], arr[i]);
                }
            );

            for (int i = 0; i < 256; i++)
            {
                norm_gisto[i] = pix_count[i] * 1.0d / (w * h * 1.0d);
            }

            return norm_gisto;
        }
        #endregion

        private int takerWindow(int[,] integrated, int x, int y, int a, int w, int h)
        {
            int x1 = x - a / 2 - 1;
            int y1 = y - a / 2 - 1;
            int x2 = x + a / 2;
            int y2 = y + a / 2;
            if (x2 >= w) x2 = w - 1;
            if (y2 >= h) y2 = h - 1;

            int s_left_bottom = x1 < 0 || y1 < 0 ? 0 : integrated[x1, y1];
            int s_left_top = x1 < 0 ? 0 : integrated[x1, y2];
            int s_right_bottom = y1 < 0 ? 0 : integrated[x2, y1];
            int sum = (integrated[x2, y2] + s_left_bottom - s_left_top - s_right_bottom);
            int w_a = x2 - (x1 < 0 ? -1 : x1);
            int h_a = y2 - (y1 < 0 ? -1 : y1);
            return sum / (w_a * h_a);
        }

        private Func<int, int, int> buildTakerWindow(int[,] integrated, int w, int h, int a) => (int x, int y) => takerWindow(integrated, x, y, a, w, h);

        private void Niblack(Img editing_image)
        {
            (int[,] integrated_image, int[,] integrated_pow2) = getIntegratedWithSquare(editing_image.pixels_matrix);

            double k = editing_image.binarization_data.param;
            int a = editing_image.binarization_data.windows_size;
            int w = editing_image.width;
            int h = editing_image.height;

            var take = buildTakerWindow(integrated_image, w, h, a);
            var takeSquared = buildTakerWindow(integrated_pow2, w, h, a);

            Parallel.For(0, w, x =>
            {
                for (int y = 0; y < h; y++)
                {
                    int M = take(x, y);
                    int M_of_squared = takeSquared(x, y);

                    int D = M_of_squared - (M * M);
                    double sigma = Math.Sqrt(D);
                    double t = M + k * sigma;

                    int res = toGreyscale(editing_image.pixels_matrix[x, y]) <= (int)t ? 0 : 255;
                    editing_image.pixels_matrix[x, y] = new Pixel(editing_image.pixels_matrix[x, y].A, res, res, res);
                }
            });
        }

        private void Sauvola(Img editing_image)
        {
            (int[,] integrated_image, int[,] integrated_pow2) = getIntegratedWithSquare(editing_image.pixels_matrix);

            double k = editing_image.binarization_data.param;
            int a = editing_image.binarization_data.windows_size;
            int w = editing_image.width;
            int h = editing_image.height;

            var take = buildTakerWindow(integrated_image, w, h, a);
            var takeSquared = buildTakerWindow(integrated_pow2, w, h, a);

            Parallel.For(0, w, x =>
            {
                for (int y = 0; y < h; y++)
                {
                    int M = take(x, y);
                    int M_of_squared = takeSquared(x, y);

                    int D = M_of_squared - (M * M);
                    double sigma = Math.Sqrt(D);
                    double t = M * (1 + k * (sigma / 128 - 1));

                    int res = toGreyscale(editing_image.pixels_matrix[x, y]) <= (int)t ? 0 : 255;
                    editing_image.pixels_matrix[x, y] = new Pixel(editing_image.pixels_matrix[x, y].A, res, res, res);
                }
            });
        }

        private void Wolf(Img editing_image)
        {
            (int[,] integrated_image, int[,] integrated_pow2, int min) = getIntegratedWithSquareAndMin(editing_image.pixels_matrix);

            double param = editing_image.binarization_data.param;
            int a = editing_image.binarization_data.windows_size;
            int w = editing_image.width;
            int h = editing_image.height;

            var take = buildTakerWindow(integrated_image, w, h, a);
            var takeSquared = buildTakerWindow(integrated_pow2, w, h, a);

            double max_sigma = 0.0d;

            Parallel.For<double>(0, w, () => (0.0d), (x, loop, subtotal) =>
            {
                for (int y = 0; y < h; y++)
                {
                    int M = take(x, y);
                    int M_of_squared = takeSquared(x, y);
                    int D = M_of_squared - (M * M);
                    double sigma = (double)Math.Sqrt(D);

                    if (sigma > subtotal) subtotal = sigma;
                }
                return subtotal;
            },
                (t_max) =>
                {
                    lock (max_sigma as Object) {
                        if (t_max > max_sigma) max_sigma = t_max;
                    }
                }
            );

            Parallel.For(0, w, x =>
            {
                for (int y = 0; y < h; y++)
                {
                    int M = take(x, y);
                    int M_of_squared = takeSquared(x, y);

                    int D = M_of_squared - (M * M);
                    double sigma = Math.Sqrt(D);
                    double t = (1.0d - param) * M + param * min + param * (sigma / max_sigma) * (M - min); 

                    int res = toGreyscale(editing_image.pixels_matrix[x, y]) <= (int)t ? 0 : 255;
                    editing_image.pixels_matrix[x, y] = new Pixel(editing_image.pixels_matrix[x, y].A, res, res, res);
                }
            });
        }

        private void Bradley(Img editing_image)
        {
            int[,] integrated_image = getIntegrated(editing_image.pixels_matrix);

            double k = editing_image.binarization_data.param;
            int a = editing_image.binarization_data.windows_size;
            int w = editing_image.width;
            int h = editing_image.height;

            var take = buildTakerWindow(integrated_image, w, h, a);

            Parallel.For(0, w, x =>
            {
                for (int y = 0; y < h; y++)
                {
                    int M = take(x, y);
                    double t = M * (1.0d - k);

                    int res = toGreyscale(editing_image.pixels_matrix[x, y]) < (int)t ? 0 : 255;
                    editing_image.pixels_matrix[x, y] = new Pixel(editing_image.pixels_matrix[x, y].A, res, res, res);
                }
            });
        }

        private int[,] getIntegrated(Pixel[,] matrix)
        {
            (int[,] res, _, _) = buildIntegratedImages(matrix);
            return res;
        }

        private (int[,], int[,]) getIntegratedWithSquare(Pixel[,] matrix)
        {
            (int[,] res, int[,] resSquared, _) = buildIntegratedImages(matrix, true);
            return (res, resSquared);
        }

        private (int[,], int[,], int) getIntegratedWithSquareAndMin(Pixel[,] matrix)
        {
            return buildIntegratedImages(matrix, true, true);
        }

        private (int[,], int[,], int) buildIntegratedImages(Pixel[,] matrix, bool includePow2 = false, bool includeMin = false)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);
            int[,] integrated_image = new int[w, h];
            int[,] integrated_pow2 = null;
            int min = 255;

            if (includePow2) integrated_pow2 = new int[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int prev_left = x == 0 ? 0 : integrated_image[x - 1, y];
                    int prev_top = y == 0 ? 0 : integrated_image[x, y - 1];
                    int prev_left_top = y == 0 || x == 0 ? 0 : integrated_image[x - 1, y - 1];
                    int g = toGreyscale(matrix[x, y]);

                    integrated_image[x, y] = g + prev_left + prev_top - prev_left_top;

                    if(includePow2)
                    {
                        prev_left = x == 0 ? 0 : integrated_pow2[x - 1, y];
                        prev_top = y == 0 ? 0 : integrated_pow2[x, y - 1];
                        prev_left_top = y == 0 || x == 0 ? 0 : integrated_pow2[x - 1, y - 1];

                        integrated_pow2[x, y] = g * g + prev_left + prev_top - prev_left_top;
                    }

                    if(includeMin)
                    {
                        if (min > g) min = g;
                    }
                }
            }
            return (integrated_image, integrated_pow2, min);
        }

    }
}
