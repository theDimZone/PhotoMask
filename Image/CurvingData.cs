using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace photomask.Image
{
    public enum CurvingChannel
    {
        RGB,
        R,
        G,
        B
    }

    public class CurvingData
    {
        public CurvingChannel channel { get; set; } = CurvingChannel.RGB;

        public List<Point> points { get; set; } = new List<Point>();

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
                    for (int i = 0; i < 255; i++) Interlocked.Add(ref pix_count[i], arr[i]);
                }
            );

            gisto_points = pix_count;
        }
    }
}
