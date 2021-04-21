using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace photomask.Image
{
    public enum CurvingChannel
    {
        R,
        G,
        B,
        RGB
    }

    public class CurvingData : ICloneable
    {
        public CurvingChannel channel { get; set; } = CurvingChannel.RGB;

        public List<Point> points { get; set; } = new List<Point>();

        public int[] interpolated_points { get; set; } = new int[256];

        public int[] gisto_points { get; set; } = new int[256];
        public int channel_view
        {
            get => (int)channel;
            set => channel = (CurvingChannel)value;
        }

        public CurvingData()
        {
            points.Add(new Point(0, 0));
            points.Add(new Point(255, 255));
            SetInterpolation();
        }

        // Lagrange interpolation
        public void SetInterpolation()
        {
            double n = points.Count;

            for (int c = 0; c < 256; c++)
            {
                double result = 0;

                //if (points[0] == new Point(0, 0) && points[1] == new Point(255, 255))
                //{
                //    interpolated_points[c] = c;
                //    continue;
                //}

                for (int i = 0; i < n; i++)
                {
                    double term = points[i].Y;
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                            term = term * (c - points[j].X) /
                                      (points[i].X - points[j].X);
                    }

                    result += term;
                }

                int y = (int)Math.Round(result);
                interpolated_points[c] = Util.Clamp(y, 0, 255);
            }
        }

        public void SetGistoPoints(Pixel[,] pixels_matrix)
        {
            int[] pix_count = new int[256];

            Parallel.For<int[]>(0, pixels_matrix.GetLength(0), () => (new int[256]), (x, loop, subtotal) =>
            {
                for (int y = 0; y < pixels_matrix.GetLength(1); y++)
                {
                    int a = 0;
                    
                    switch(channel)
                    {
                        case CurvingChannel.RGB:
                            a = (pixels_matrix[x, y].R + pixels_matrix[x, y].G + pixels_matrix[x, y].B) / 3;
                            break;
                        case CurvingChannel.R:
                            a = pixels_matrix[x, y].R;
                            break;
                        case CurvingChannel.G:
                            a = pixels_matrix[x, y].G;
                            break;
                        case CurvingChannel.B:
                            a = pixels_matrix[x, y].B;
                            break;
                    }
                    
                    subtotal[a]++;
                }
                return subtotal;
            },
                (arr) =>
                {
                    for (int i = 0; i < 256; i++) Interlocked.Add(ref pix_count[i], arr[i]);
                }
            );

            gisto_points = pix_count;
        }

        public object Clone()
        {
            CurvingData data = new CurvingData();
            data.channel = channel;
            data.gisto_points = gisto_points;
            data.interpolated_points = interpolated_points;
            data.points = points;
            return data;
        }
    }
}
