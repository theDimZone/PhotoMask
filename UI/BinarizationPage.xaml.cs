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
    /// Логика взаимодействия для BinarizationPage.xaml
    /// </summary>
    public partial class BinarizationPage : Page
    {
        public Img image { get; set; }

        public BinarizationPage(Img image)
        {
            this.image = image;
            this.DataContext = this.image;
            InitializeComponent();
        }

        private void SetParams()
        {
            BinarizationMode selected = (BinarizationMode)comboBox.SelectedIndex;
            if(selected == BinarizationMode.None 
                || selected == BinarizationMode.Gavrilov 
                || selected == BinarizationMode.Otsu)
            {
                stackPanelInputs.Visibility = Visibility.Hidden;
            } else
            {
                if (selected != image.binarization_data.mode)
                    textBoxParam.Text = image.binarization_data.default_params[selected].ToString();
                else
                    textBoxParam.Text = image.binarization_data.param.ToString();

                stackPanelInputs.Visibility = Visibility.Visible;
            }
        }

        private void buttonSet_Click(object sender, RoutedEventArgs e)
        {
            int mode_index = comboBox.SelectedIndex;
            image.binarization_data.mode_view = mode_index;
            image.binarization_data.windows_size = Convert.ToInt32(textBoxSize.Text);
            image.binarization_data.param = float.Parse(textBoxParam.Text);

            Images.Calculate(image);
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetParams();
        }
    }
}
