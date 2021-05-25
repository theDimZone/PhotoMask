using photomask.Image;
using photomask.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace photomask.UI
{
    /// <summary>
    /// Логика взаимодействия для FrequencyFilteringPage.xaml
    /// </summary>
    public partial class FrequencyFilteringPage : Page
    {
        public Img image { get; set; }
        private int original_width { get; set; } = 0;
        private int original_height { get; set; } = 0;
        private double[][] circles { get; set; } = new double[0][];
        private bool is_init_image { get; set; } = true;
        private bool is_init_canvas { get; set; } = true;

        public FrequencyFilteringPage(Img image)
        {
            this.image = image;
            this.circles = image.frequency_filtering_data.circles;
            this.DataContext = this.image;

            if (image.frequency_filtering_data.fourier_image != null)
            {
                SetOriginals();
            }

            InitializeComponent();
        }

        private void AdjustSize()
        {
            canvasFourier.Width = imageFourier.ActualWidth;
            canvasFourier.Height = imageFourier.ActualHeight;
        }

        private void SetOriginals()
        {
            if (image.frequency_filtering_data.fourier_image != null)
            {
                original_height = image.frequency_filtering_data.fourier_image.Height;
                original_width = image.frequency_filtering_data.fourier_image.Width;
            }
        }

        private double try_parse(string s)
        {
            double res = 0.0d;
            Double.TryParse(s, out res);
            return res;
        }

        private void OnPageLoad(object sender, RoutedEventArgs e)
        {
            if(image.frequency_filtering_data.fourier_image != null)
            {
                /*
                if (image.frequency_filtering_data.old_height != image.pixels_matrix.GetLength(1)
                    || image.frequency_filtering_data.old_width != image.pixels_matrix.GetLength(0))
                {
                    image.frequency_filtering_data.ClearFourier();
                }
                else
                {
                    imageFourier.Source = Util.GetImageSourceAnyFormat(image.frequency_filtering_data.fourier_image);
                    gridGenerate.Visibility = Visibility.Hidden;
                    gridFourier.Visibility = Visibility.Visible;
                }
                */
                imageFourier.Source = Util.GetImageSourceAnyFormat(image.frequency_filtering_data.fourier_image);
                gridGenerate.Visibility = Visibility.Hidden;
                gridFourier.Visibility = Visibility.Visible;
            } 
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustSize();
            drawCircles();
        }

        void imageUpdated(object sender, EventArgs e)
        {
            if(imageFourier.ActualWidth > 0 && is_init_image)
            {
                AdjustSize();
                is_init_image = false;
            }
        }

        void canvasUpdated(object sender, EventArgs e)
        {
            if (canvasFourier.ActualWidth > 0 && is_init_canvas)
            {
                drawCircles();
                is_init_canvas = false;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = ToOriginalPoint(e.GetPosition(canvasFourier), canvasFourier);
            labelCursor.Content = p.X.ToString() + ";" + p.Y.ToString();
        }

        private void buttonGenerate_Click(object sender, RoutedEventArgs e)
        {
            image.frequency_filtering_data.GenerateSpectrum(image.pixels_matrix);
            SetOriginals();
            imageFourier.Source = Util.GetImageSourceAnyFormat(image.frequency_filtering_data.fourier_image);
            gridGenerate.Visibility = Visibility.Hidden;
            gridFourier.Visibility = Visibility.Visible;

        }
        private void textBoxFilterArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] lines = textBoxFilterArea.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            circles = lines.Select(line => line.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(d => try_parse(d)).ToArray()).ToArray();
            for(int i = 0; i < circles.Length; i++) Array.Resize(ref circles[i], 4);

            drawCircles();
        }

        private void drawCircles()
        {
            if (image.frequency_filtering_data.fourier_image == null) return;

            canvasFourier.Children.Clear();
            double mul = canvasFourier.ActualHeight / original_height;

            foreach (double[] line in circles)
            {
                for(int i = 3; i >= 2; i--)
                {
                    Ellipse c = new Ellipse();
                    double r = line[i] * mul;
                    c.Width = r * 2;
                    c.Height = r * 2;
                    c.Stroke = Brushes.Red;
                    c.StrokeThickness = 1;

                    Point p = ToCanvasPoint(new Point(line[0], line[1]), canvasFourier);
                    Canvas.SetLeft(c, p.X - r);
                    Canvas.SetBottom(c, p.Y - r);
                    canvasFourier.Children.Add(c);
                    
                }
            }
        }

        private void buttonSet_Click(object sender, RoutedEventArgs e)
        {
            FrequencyFilteringMode selected = (FrequencyFilteringMode)comboBox.SelectedIndex;

            image.frequency_filtering_data.circles = circles;
            image.frequency_filtering_data.circles_view = textBoxFilterArea.Text;
            image.frequency_filtering_data.inner_multiplier = try_parse(textBoxCondInner.Text);
            image.frequency_filtering_data.rest_multiplier = try_parse(textBoxContRest.Text);
            image.frequency_filtering_data.mode = selected;
            Images.Calculate(image);
        }

        private Point ToCanvasPoint(Point p, Canvas surface)
        {
            var x = Math.Round(p.X / original_width * surface.ActualWidth + surface.ActualWidth / 2);
            var y = Math.Round(p.Y / original_height * surface.ActualHeight + surface.ActualHeight / 2);

            return new Point(x, y);
        }

        private Point ToOriginalPoint(Point p, Canvas surface)
        {
            var x = Math.Round(p.X / surface.ActualWidth * original_width - original_width / 2);
            var y = Math.Round(original_height / 2 - p.Y / surface.ActualHeight * original_height);

            return new Point(x, y);
        }
    }
}
