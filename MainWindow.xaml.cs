using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace photomask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Masker masker = new Masker();
        public ObservableCollection<ImageMask> images = new ObservableCollection<ImageMask>();

        public MainWindow()
        {
            InitializeComponent();
            itemsControl.ItemsSource = images;
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|Все файлы (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                ImageMask mask = new ImageMask(openFileDialog.FileName);
                images.Add(mask);

                Scroll.ScrollToEnd();
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG изображение|*.png|JPEG изображение|*.jpeg|BMP изображение|*.bmp";
            saveFileDialog.RestoreDirectory = true;

            saveFileDialog.FileName = "PhotoMask " + DateTime.Now.ToString("dd-MM-yyyy HHmmss");
            saveFileDialog.DefaultExt = "png";

            if (saveFileDialog.ShowDialog() == true)
                masker.Save(saveFileDialog.FileName);
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            ImageMask mask = but.DataContext as ImageMask;
            int index = images.IndexOf(mask);

            if(index < images.Count() - 1)
            {
                (images[index], images[index + 1]) = (images[index + 1], images[index]);

                Remask();
            }
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            ImageMask mask = but.DataContext as ImageMask;
            int index = images.IndexOf(mask);

            if (index > 0 && images.Count() >= 2)
            {
                (images[index], images[index - 1]) = (images[index - 1], images[index]);

                Remask();
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            ImageMask mask = but.DataContext as ImageMask;

            images.Remove(mask);
            
            Remask();
        }

        // kastil'
        private void comboBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            ComboBox box = item.Parent as ComboBox;

            int method_index = box.Items.IndexOf(item);

            ImageMask mask = box.DataContext as ImageMask;
            mask.method_view = method_index;

            Remask();
        }

        private void slider_Click(object sender, RoutedEventArgs e)
        {
            Remask();
        }


        private void maskWidth_Changed(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            ImageMask mask = box.DataContext as ImageMask;

            if(Util.isNatural(box.Text))
            {
                Grid grid = box.Parent as Grid;
                TextBox h_box = grid.FindName("textBoxHeight") as TextBox;
                mask.width_view = Convert.ToInt32(box.Text);

                h_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }

        }

        private void maskHeight_Changed(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            ImageMask mask = box.DataContext as ImageMask;

            if (Util.isNatural(box.Text))
            {
                Grid grid = box.Parent as Grid;
                TextBox w_box = grid.FindName("textBoxWidth") as TextBox;
                mask.height_view = Convert.ToInt32(box.Text);

                w_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        private void buttonResize_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            ImageMask mask = but.DataContext as ImageMask;
            mask.Resize(mask.width_view, mask.height_view);
            Remask();
        }

        private void buttonResizeOriginal_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            ImageMask mask = but.DataContext as ImageMask;
            mask.ResizeToOriginal();
            
            Grid grid = but.Parent as Grid;
            TextBox w_box = grid.FindName("textBoxWidth") as TextBox;
            TextBox h_box = grid.FindName("textBoxHeight") as TextBox;
            w_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            h_box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();

            Remask();
        }

        private void Remask()
        {
            var time1 = DateTime.Now;

            masker.Blend(images);

            var time2 = DateTime.Now;
            textTime.Text = Math.Round((time2 - time1).TotalMilliseconds).ToString();

            imageMain.Source = masker.ImageSource;
            buttonSave.IsEnabled = imageMain.Source == null ? false : true;
        }
    }
}
