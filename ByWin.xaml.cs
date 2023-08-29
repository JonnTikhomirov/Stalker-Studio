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
using System.Windows.Shapes;

namespace Stalker_Studio
{
    /// <summary>
    /// Логика взаимодействия для ByWin.xaml
    /// </summary>
    public partial class ByWin : Window
    {
        public ByWin()
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);

            ap_pro.Source = MainWindow.BitmapToImageSource(new System.Drawing.Bitmap($"{System.Windows.Forms.Application.StartupPath}\\Data\\Image\\ap-pro.jpg"));
            VK2.Source = MainWindow.BitmapToImageSource(new System.Drawing.Bitmap($"{System.Windows.Forms.Application.StartupPath}\\Data\\Image\\vk.png"));
            //VK.Source = MainWindow.BitmapToImageSource(new System.Drawing.Bitmap($"{System.Windows.Forms.Application.StartupPath}\\Data\\Image\\vk.png"));
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/stalker_fans_sdk");
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ap-pro.ru/profile/11401-kirill-suhinin/");
        }

        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/optim270");
        }
    }
}
