﻿using photomask.Image;
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

            frame.Navigate(new BlendPage(image));
        }

        private void buttonBlend_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(new BlendPage(image));
        }

        private void buttonSize_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(new SizePage(image));
        }
    }
}