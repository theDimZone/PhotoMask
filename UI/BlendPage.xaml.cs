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
    /// Логика взаимодействия для BlendPage.xaml
    /// </summary>
    public partial class BlendPage : Page
    {
        private Img image { get; set; }

        public BlendPage(Img image)
        {
            this.image = image;
            this.DataContext = this.image;

            InitializeComponent();
        }

        private void slider_Click(object sender, RoutedEventArgs e)
        {
            Images.Calculate(image);
        }

        private void comboBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            ComboBox box = item.Parent as ComboBox;

            int mode_index = box.Items.IndexOf(item);

            image.blend_data.mode_view = mode_index;

            Images.Calculate(image);
        }
    }
}
