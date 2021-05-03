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
    }
}
