using photomask.Image;
using photomask.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace photomask.UI
{
    /// <summary>
    /// Логика взаимодействия для CurvingPage.xaml
    /// </summary>
    public partial class CurvingPage : Page
    {
        public Img image { get; set; }

        public CurvingPage(Img image)
        {
            this.image = image;
            this.DataContext = this.image;
            InitializeComponent();
        }

        private void OnPageLoad(object sender, RoutedEventArgs e)
        {
            DrawCurve(withCalculate: true);
        }

        private void DrawCurve(bool withCalculate)
        {
            paintSurface.Children.Clear();
            image.curving_data.points.Sort((f, s) => f.X.CompareTo(s.X));

            if (image.curving_data.points.Count >= 2)
            {
                image.curving_data.SetInterpolation();
                
                if(withCalculate) Images.Calculate(image);

                DrawGisto();

                Polyline line = new Polyline();
                line.Stroke = Brushes.Pink;
                line.StrokeThickness = 2;
                line.StrokeLineJoin = PenLineJoin.Round;

                Point p_i;    
                for (int i = 0; i < 256; i++)     
                {    
                    p_i = new Point(i, image.curving_data.interpolated_points[i]);
                    line.Points.Add(ToCanvasPoint(p_i, paintSurface));
                }

                paintSurface.Children.Add(line);

            }


            // Draw the points.
            foreach (Point point in image.curving_data.points)
            {
                var p = ToCanvasPoint(point, paintSurface);

                Rectangle rect = new Rectangle();
                rect.Width = 8;
                rect.Height = 8;
                Canvas.SetLeft(rect, p.X - 4);
                Canvas.SetTop(rect, p.Y - 4);
                rect.Fill = Brushes.White;
                rect.Stroke = Brushes.Black;
                rect.StrokeThickness = 1;
                rect.DataContext = point;
                rect.MouseRightButtonDown += Point_MouseRightButtonDown;
                paintSurface.Children.Add(rect);
            }

        }

        private void DrawGisto()
        {
            gistoSurface.Children.Clear();

            int[] pix_count = image.curving_data.gisto_points;

            int max = pix_count.Max();
            double rect_width = gistoSurface.ActualWidth / 256.0d;
            double k = gistoSurface.ActualHeight / (max * 1.0d);

            Polygon poly = new Polygon();
            poly.Fill = Brushes.Black;
            Canvas.SetLeft(poly, 0);
            Canvas.SetBottom(poly, 0);

            poly.Points.Add(new Point(0, gistoSurface.ActualHeight));
            for (int i = 0; i < 256; i++)
            {
                double y = Math.Round(gistoSurface.ActualHeight - k * pix_count[i] / gistoSurface.ActualHeight * gistoSurface.ActualHeight);
                double x = i * rect_width;
                poly.Points.Add(new Point(x, y));
                poly.Points.Add(new Point(x + rect_width, y));

            }
            poly.Points.Add(new Point(255 * rect_width + rect_width, gistoSurface.ActualHeight));
            gistoSurface.Children.Add(poly);
        }

        private void AddPoint(Point point)
        {
            if (image.curving_data.points.Exists(p => p.X == point.X)) return;

            image.curving_data.points.Add(point);
            DrawCurve(withCalculate: true);
        }

        private Point ToCanvasPoint(Point p, Canvas surface)
        {
            var x = Math.Round(p.X / 255 * surface.ActualWidth);
            var y = Math.Round(surface.ActualHeight - p.Y / 255 * surface.ActualHeight);

            return new Point(x, y);
        }

        private Point ToRgbPoint(Point p, Canvas surface)
        {
            var x = Math.Round(p.X / surface.ActualWidth * 255);
            var y = Math.Round(255 - p.Y / surface.ActualHeight * 255);

            return new Point(x, y);
        }

        private void comboBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            ComboBox box = item.Parent as ComboBox;

            int channel_index = box.Items.IndexOf(item);

            image.curving_data.channel_view = channel_index;

            Images.Calculate(image);
            DrawGisto();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var p = ToRgbPoint(e.GetPosition(paintSurface), paintSurface);

            labelX.Content = p.X.ToString();
            labelY.Content = p.Y.ToString();
        }

        private void buttonNewPoint_Click(object sender, RoutedEventArgs e)
        {
            if (!Util.isNatural(textBoxX.Text) || !Util.isNatural(textBoxY.Text)) return;
            var x = Util.Clamp(Convert.ToInt32(textBoxX.Text), 0, 255);
            var y = Util.Clamp(Convert.ToInt32(textBoxY.Text), 0, 255);

            AddPoint(new Point(x, y));
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(paintSurface);

            AddPoint(ToRgbPoint(p, paintSurface));
        }


        private void Point_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;
            var point = (Point)rect.DataContext;
            image.curving_data.points.Remove(point);

            DrawCurve(withCalculate: true);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawCurve(withCalculate: false);
        }
    }
}
