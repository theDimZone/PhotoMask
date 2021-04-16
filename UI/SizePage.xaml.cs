using photomask.Image;
using photomask.Static;
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
    /// Логика взаимодействия для SizePage.xaml
    /// </summary>
    public partial class SizePage : Page
    {
        private Img image { get; set; }

        public SizePage(Img image)
        {
            this.image = image;
            this.DataContext = this.image;

            InitializeComponent();
        }

        private void maskWidth_Changed(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (Util.isNatural(box.Text))
            {
                Grid grid = box.Parent as Grid;
                TextBox h_box = grid.FindName("textBoxHeight") as TextBox;
                image.width_view = Convert.ToInt32(box.Text);

                h_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }

        }

        private void maskHeight_Changed(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (Util.isNatural(box.Text))
            {
                Grid grid = box.Parent as Grid;
                TextBox w_box = grid.FindName("textBoxWidth") as TextBox;
                image.height_view = Convert.ToInt32(box.Text);

                w_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        private void buttonResize_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            image.Resize(image.width_view, image.height_view);

            Images.Calculate(image);
        }

        private void buttonResizeOriginal_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            image.ResizeToOriginal();

            Grid grid = but.Parent as Grid;
            TextBox w_box = grid.FindName("textBoxWidth") as TextBox;
            TextBox h_box = grid.FindName("textBoxHeight") as TextBox;
            w_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            h_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();

            Images.Calculate(image);
        }
    }
}
