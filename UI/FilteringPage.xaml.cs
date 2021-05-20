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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace photomask.UI
{
    /// <summary>
    /// Логика взаимодействия для FilteringPage.xaml
    /// </summary>
    public partial class FilteringPage : Page
    {
        public Img image { get; set; }

        public FilteringPage(Img image)
        {
            this.image = image;
            this.DataContext = this.image;
            InitializeComponent();
        }

        private double try_parse(string s)
        {
            double res = 0.0d;
            if (s.IndexOf("/") != -1)
            {
                string[] digs = s.Split("/");
                double d1, d2;
                if(digs.Length >= 2 && Double.TryParse(digs[0], out d1) && Double.TryParse(digs[1], out d2))
                {
                    res = d1 / d2;
                }
            } else {
                Double.TryParse(s, out res);
            }
            return res;
        }

        private double[,] stringToMatrix(string s)
        {
            string[] lines = s.Split(Environment.NewLine);
            int h = lines.Length;
            int w = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;

            double[,] matrix = new double[h, w];

            for (int i = 0; i < h; i++)
            {
                string[] digits = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < w; j++)
                {
                    matrix[i, j] = try_parse(digits[j]);
                }
            }

            return matrix;
        }

        private void buttonSet_Click(object sender, RoutedEventArgs e)
        {
            FilteringMode selected = (FilteringMode)comboBox.SelectedIndex;
            
            if(selected == FilteringMode.Linear)
            {
                image.filtering_data.kernel_view = textBoxKernel.Text;
                image.filtering_data.kernel = stringToMatrix(textBoxKernel.Text);

            } else if(selected == FilteringMode.Median)
            {
                // median param
                image.filtering_data.median_radius = Int32.Parse(textBoxMedianRadius.Text);
            }

            image.filtering_data.mode = selected;
            Images.Calculate(image);
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilteringMode selected = (FilteringMode)comboBox.SelectedIndex;
            if (selected == FilteringMode.Linear)
            {
                gridLinearInput.Visibility = Visibility.Visible;
                stackPanelMedian.Visibility = Visibility.Hidden;
            } else if(selected == FilteringMode.Median)
            {
                gridLinearInput.Visibility = Visibility.Hidden;
                stackPanelMedian.Visibility = Visibility.Visible;
            } else
            {
                gridLinearInput.Visibility = Visibility.Hidden;
                stackPanelMedian.Visibility = Visibility.Hidden;
            }
        }

        private void textBoxKernel_TextChanged(object sender, TextChangedEventArgs e)
        {
            double sum = textBoxKernel.Text
                .Split(Environment.NewLine)
                .Aggregate(0.0d, (double acc, string line) => acc += line
                                                                        .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                                                        .Aggregate(0.0d, (double lacc, string dig) => lacc += try_parse(dig)));

            labelKernelSum.Content = Math.Round(sum, 5).ToString();

        }

        private void buttonGenerateGauss_Click(object sender, RoutedEventArgs e)
        {
            int r = Int32.Parse(textBoxRadiusGauss.Text);
            double sigma = Double.Parse(textBoxSigmaGauss.Text);

            string result = "";
            for(int i = -r; i <= r; i++)
            {
                for(int j = -r; j <= r; j++)
                {
                    double g = 1.0d / (2.0d * Math.PI * sigma * sigma) * Math.Exp(-1.0d * (i * i + j * j) / (2.0d * sigma * sigma));
                    string s = string.Format("{0:0.00000}", Math.Round(g, 5));
                    
                    result += s + " ";
                }
                
                if (i != r) result += Environment.NewLine;
            }

            textBoxKernel.Text = result;
        }
    }
}
