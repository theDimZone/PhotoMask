using Microsoft.Win32;
using photomask.Image;
using photomask.Static;
using photomask.UI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace photomask.UI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            itemsControl.ItemsSource = Images.imgs;
            Images.ResultChanged += delegate (double elapsed_time)
            {
                Render(elapsed_time);
            };
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|Все файлы (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                Img mask = new Img(openFileDialog.FileName);
                Images.imgs.Add(mask);

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
                Images.SaveResult(saveFileDialog.FileName);
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            Img mask = but.DataContext as Img;
            int index = Images.imgs.IndexOf(mask);

            if(index < Images.imgs.Count() - 1)
            {
                (Images.imgs[index], Images.imgs[index + 1]) = (Images.imgs[index + 1], Images.imgs[index]);

                Images.Calculate(Images.imgs[index + 1]);
            }
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            Img mask = but.DataContext as Img;
            int index = Images.imgs.IndexOf(mask);

            if (index > 0 && Images.imgs.Count() >= 2)
            {
                (Images.imgs[index], Images.imgs[index - 1]) = (Images.imgs[index - 1], Images.imgs[index]);

                Images.Calculate(Images.imgs[index]);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            Img mask = but.DataContext as Img;

            int index = Images.imgs.IndexOf(mask);

            foreach(var win in OwnedWindows)
            {
                var act_win = win as ActionsWindow;
                if (act_win.image == mask) act_win.Close();
            }

            Images.imgs.Remove(mask);


            if (Images.imgs.Count() == 0)
            {
                Images.Reset();
            } 
            else if(index < Images.imgs.Count())
            {
                Images.Calculate(Images.imgs[index]);
            }
            else
            {
                Images.Calculate(Images.imgs.Last());
            }
        }
        private void buttonActions_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            Img mask = but.DataContext as Img;

            ActionsWindow actionsWindow = new ActionsWindow(mask);
            actionsWindow.Owner = this;

            actionsWindow.Show();
        }

        private void Render(double elapsed_time)
        {
            imageMain.Source = Images.ResultImageSource;

            textTime.Text = elapsed_time.ToString();

            buttonSave.IsEnabled = imageMain.Source == null ? false : true;
        }
    }
}
