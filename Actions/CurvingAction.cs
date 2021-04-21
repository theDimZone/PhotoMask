using photomask.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace photomask.Actions
{
    public class CurvingAction : IAction
    {
        public IAction next_action { get; set; }

        public void DoAction(Img current_img, List<Img> images)
        {
            //for (int i = 0; i < images.Count; i++)
            Parallel.For(0, images.Count, i =>
            {
                Parallel.For(0, images[i].width, x =>
                {
                    for (int y = 0; y < images[i].height; y++)
                    {
                        int R = images[i].pixels_matrix[x, y].R;
                        int G = images[i].pixels_matrix[x, y].G;
                        int B = images[i].pixels_matrix[x, y].B;

                        switch (current_img.curving_data.channel)
                        {
                            case CurvingChannel.RGB:
                                R = images[i].curving_data.interpolated_points[R];
                                G = images[i].curving_data.interpolated_points[G];
                                B = images[i].curving_data.interpolated_points[B];
                                break;
                            case CurvingChannel.R:
                                R = images[i].curving_data.interpolated_points[R];
                                break;
                            case CurvingChannel.G:
                                G = images[i].curving_data.interpolated_points[G];
                                break;
                            case CurvingChannel.B:
                                B = images[i].curving_data.interpolated_points[B];
                                break;
                        }

                        int a = images[i].pixels_matrix[x, y].A;
                        images[i].pixels_matrix[x, y] = new Pixel(a, R, G, B);
                    }

                });

                if (images[i].Id == current_img.Id) current_img.curving_data.SetGistoPoints(images[i].pixels_matrix);
            });
            
            
            next_action?.DoAction(current_img, images);
        }
    }
}
