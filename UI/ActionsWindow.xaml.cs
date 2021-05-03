using photomask.Image;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для ActionsWindow.xaml
    /// </summary>
    public partial class ActionsWindow : Window
    {
        public Img image { get; set; }

        public ActionsWindow(Img image)
        {
            this.image = image;

            InitializeComponent();

            labelTitle.Content = "Blend";
            frame.Navigate(new BlendPage(image));
        }

        private void buttonBlend_Click(object sender, RoutedEventArgs e)
        {
            labelTitle.Content = "Blend";
            frame.Navigate(new BlendPage(image));
        }

        private void buttonSize_Click(object sender, RoutedEventArgs e)
        {
            labelTitle.Content = "Size";
            frame.Navigate(new SizePage(image));
        }

        private void buttonCurving_Click(object sender, RoutedEventArgs e)
        {
            labelTitle.Content = "Curving";
            frame.Navigate(new CurvingPage(image));
        }

        private void buttonBinarization_Click(object sender, RoutedEventArgs e)
        {
            labelTitle.Content = "Binarization";
            frame.Navigate(new BinarizationPage(image));
        }

        private void buttonFiltering_Click(object sender, RoutedEventArgs e)
        {
            labelTitle.Content = "Filtering";
            frame.Navigate(new FilteringPage(image));
        }
    }
}
