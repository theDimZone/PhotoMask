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
    public class BinarizationAction : IAction
    {
        public IAction next_action { get; set; }

        private delegate void BinarizationMethod(Img editing_image);
        private Dictionary<BinarizationMode, BinarizationMethod> binarization_methods = new();
        public BinarizationAction()
        {
            binarization_methods[BinarizationMode.Gavrilov] = Gavrilov;
            binarization_methods[BinarizationMode.Otsu] = Otsu;
            //binarization_methods[BinarizationMode.Niblack] = Niblack;
        }

        public void DoAction(Img current_img, List<Img> images)
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i].binarization_data.mode == BinarizationMode.None) continue;

                binarization_methods[images[i].binarization_data.mode](images[i]);
            }

            next_action?.DoAction(current_img, images);
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

        private void Gavrilov(Img editing_image)
        {
            int[,] integrated_image = buildIntegratedImage(editing_image.pixels_matrix);
            int t = integrated_image[editing_image.width - 1, editing_image.height - 1] / (editing_image.width * editing_image.height);

            GlobalMethod(editing_image, t);
        }

        private void Otsu(Img editing_image)
        {
            double[] norm_gisto = buildNormGisto(editing_image.pixels_matrix);

            int t = 0;
            double sigma_max = 0;

            double l_max = 256;
            for(int i = 255; i >= 0; i--)
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
            for(int i = 0; i < l_max; i++)
            {
                double omega1 = N_sum;
                double omega2 = 1.0d - omega1;

                double u1 = Ni_sum / omega1;
                double u2 = (u_T - u1 * omega1) / omega2;

                double sigma = omega1 * omega2 * Math.Pow(u1 - u2, 2);
                if(sigma > sigma_max)
                {
                    sigma_max = sigma;
                    t = i;
                }

                N_sum += norm_gisto[i];
                Ni_sum += i * norm_gisto[i];
            }

            GlobalMethod(editing_image, t);
        }

        //private int constraintBottom(int val) => val < 0 ? 0 : val;
        //private int constraintTop(int val, int max) => val < max ? val : ; 

        /* WIP
        private void Niblack(Img editing_image)
        {
            int[,] integrated_image = buildIntegratedImage(editing_image.pixels_matrix);
            // need integrated_image ^ 2
            float k = editing_image.binarization_data.param;
            int a = editing_image.binarization_data.windows_size;
            int w = editing_image.width;
            int h = editing_image.height;

            Parallel.For(0, w, x =>
            {
                for (int y = 0; y < h; y++)
                {
                    int x1 = x - a / 2 - 1;
                    int y1 = y - a / 2 - 1;
                    int x2 = x + a / 2;
                    int y2 = y + a / 2;
                    

                    //int s_left_bottom = x1 < 0 || y1 < 0 ? 0 : integrated_image[x1, y1];
                    //int s_right_top = x2 >= w || y2 >= h ? integrated_image[w - 1, h - 1] : integrated_image[x2, y2];
                    //int s_left_top = x1 < 0 || y2 < 0 ? 0 : integrated_image[x1, y2];
                    //int s_right_bottom = x2 < 0 || y1 < 0 ? 0 : integrated_image[x2, y1];

                    int M = integrated_image[x2, y2] + integrated_image[x1, y1] - integrated_image[x1, y2] - integrated_image[x2, y1];
                }
            });
        }
        */

        private int[,] buildIntegratedImage(Pixel[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);
            int[,] integrated_image = new int[w, h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int prev_left = x == 0 ? 0 : integrated_image[x - 1, y];
                    int prev_top = y == 0 ? 0 : integrated_image[x, y - 1];
                    int prev_left_top = y == 0 || x == 0 ? 0 : integrated_image[x - 1, y - 1];

                    integrated_image[x, y] = toGreyscale(matrix[x, y]) + prev_left + prev_top - prev_left_top;
                }
            }
            return integrated_image;
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

        private int toGreyscale(Pixel pix)
        {
            return (int)Math.Round(0.2125f * pix.R + 0.7154f * pix.G + 0.0721f * pix.B);
        }

    }
}
