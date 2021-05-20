using photomask.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class FilteringAction : IAction
    {
        public IAction next_action { get; set; }

        private delegate void FilteringMethod(Img editing_image);
        private Dictionary<FilteringMode, FilteringMethod> filtering_methods = new Dictionary<FilteringMode, FilteringMethod>();
        
        public FilteringAction()
        {
            filtering_methods[FilteringMode.Linear] = Linear;
            filtering_methods[FilteringMode.Median] = Median;
        }

        public void DoAction(Img current_img, List<Img> images)
        {
            Parallel.For(0, images.Count, i =>
            {
                if (images[i].filtering_data.mode == FilteringMode.None) return;

                filtering_methods[images[i].filtering_data.mode](images[i]);
            });

            next_action?.DoAction(current_img, images);
        }

        // works, but slow af.. TODO: boost the speed, mb gpu could help...
        private void Median(Img image)
        {
            int r = image.filtering_data.median_radius;
            if (r <= 0) return;
            int side = r * 2 + 1;
            int mid = side * side / 2;
            Pixel[,] pixels = image.pixels_matrix.Clone() as Pixel[,];
            Parallel.For(0, image.width, x =>
            {
                for (int y = 0; y < image.height; y++)
                {
                    int[] Rvalues = new int[side * side];
                    int[] Gvalues = new int[side * side];
                    int[] Bvalues = new int[side * side];

                    for (int i = -r; i <= r; i++)
                    {
                        for (int j = -r; j <= r; j++)
                        {
                            int x_match = x + j;
                            int y_match = y + i;

                            if (x_match < 0)
                            {
                                x_match = -j;
                            }
                            else if (x_match >= image.width)
                            {
                                x_match = x - j;
                            }

                            if (y_match < 0)
                            {
                                y_match = -i;
                            }
                            else if (y_match >= image.height)
                            {
                                y_match = y - i;
                            }

                            Pixel pix = pixels[x_match, y_match];
                            int d = (i + r) * side + (j + r);

                            Rvalues[d] = pix.R;
                            Gvalues[d] = pix.G;
                            Bvalues[d] = pix.B;
                        }
                    }

                    image.pixels_matrix[x, y] = new Pixel(
                        image.pixels_matrix[x, y].A, 
                        Util.QuickSelect(Rvalues, mid), 
                        Util.QuickSelect(Gvalues, mid), 
                        Util.QuickSelect(Bvalues, mid));
                }
            });
        }

        private void Linear(Img image)
        {
            int w_kernel = image.filtering_data.kernel.GetLength(0);
            int h_kernel = image.filtering_data.kernel.GetLength(1);
            if (w_kernel == 0 || h_kernel == 0) return;
            if (w_kernel % 2 == 0) w_kernel--;
            if (h_kernel % 2 == 0) h_kernel--;

            int radius_h = h_kernel / 2;
            int radius_w = w_kernel / 2;

            Pixel[,] pixels = image.pixels_matrix.Clone() as Pixel[,];
            Parallel.For(0, image.width, x =>
            {
                //int y = d / image.width;
                //int x = d % image.width;
                for (int y = 0; y < image.height; y++)
                {
                    double conv_R = 0.0d;
                    double conv_G = 0.0d;
                    double conv_B = 0.0d;

                    for (int i = -radius_h; i <= radius_h; i++)
                    {
                        for (int j = -radius_w; j <= radius_w; j++)
                        {
                            int x_match = x + j;
                            int y_match = y + i;

                            if(x_match < 0)
                            {
                                x_match = -j;
                            } else if(x_match >= image.width)
                            {
                                x_match = x - j;
                            }

                            if(y_match < 0)
                            {
                                y_match = -i;
                            } else if(y_match >= image.height)
                            {
                                y_match = y - i;
                            }

                            Pixel pix = pixels[x_match, y_match];
                            double k = image.filtering_data.kernel[i + radius_h, j + radius_w];
                            conv_R += k * pix.R;
                            conv_G += k * pix.G;
                            conv_B += k * pix.B;
                        }
                    }

                    int R = Util.Clamp((int)conv_R, 0, 255);
                    int G = Util.Clamp((int)conv_G, 0, 255);
                    int B = Util.Clamp((int)conv_B, 0, 255);
                    image.pixels_matrix[x, y] = new Pixel(image.pixels_matrix[x, y].A, R, G, B);
                }
            });
        }


    }
}
