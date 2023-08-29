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
using System.Drawing;
using System.Diagnostics;

namespace Stalker_Studio.StalkerWin
{
    /// <summary>
    /// Логика взаимодействия для AddIconForIconWin.xaml
    /// </summary>
    public partial class AddIconForIconWin : Window
    {
        public AddIconForIconWin(string InputDDS)
        {
            InitializeComponent();
            this.InputDDS = InputDDS;
            StalkerClass.DDSImage dds = new StalkerClass.DDSImage(System.IO.File.ReadAllBytes(InputDDS));
            System.Drawing.Bitmap bts = dds.BitmapImage;
            for (int x = 0; x < bts.Width; x++)
            {
                if (x % StalkerClass.DDSImage.S_Y == 0)
                {
                    for (int y = 0; y < bts.Height; y++)
                    {
                       bts.SetPixel(x, y, System.Drawing.Color.Red);
                    }
                }
            }
            for (int y = 0; y < bts.Height; y++)
            {
                if (y % StalkerClass.DDSImage.S_Y == 0)
                {
                    for (int x = 0; x < bts.Width; x++)
                    {
                        bts.SetPixel(x, y, System.Drawing.Color.Red);
                    }
                }
            }

            

           // image.Source = MainWindow.BitmapToImageSource(bts);
            Graphics grap = Graphics.FromImage(bts);
            



            image.Source = MainWindow.BitmapToImageSource(bts);

        }
        private string InputDDS;
        private string InputAddPng = null;

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pStart = e.GetPosition(image);
        }

        private System.Windows.Point pStart;
        private System.Windows.Point _pEnd;

        private float X_OFFSET;
        private float Y_OFFSET;

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pEnd = e.GetPosition(image);
            _pEnd = pEnd;
            int _x = (int)pStart.X;
            int _y = (int)pStart.Y;

            int w = (int)pEnd.X;
            int h = (int)pEnd.Y;

            if (_x > w)
            {
                int tmp = _x;
                _x = w;
                w = tmp;
            }
            if (_y > h)
            {
                int tmp = _y;
                _y = h;
                h = tmp;
            }


            System.Drawing.Bitmap bts = MainWindow.FromSourceImage((BitmapImage)(image.Source));
            int count_x = -2;
            int count_y = -2;
            for (int x = 0; x < bts.Width; x++)
            {
                if (x % StalkerClass.DDSImage.S_X == 0)
                {
                    count_x++;
                    if (x >= _x && x <= _x + StalkerClass.DDSImage.S_X)
                        break;
                }

            }
            if (count_x == -1)
                count_x = 0;

            for (int y = 0; y < bts.Height; y++)
            {
                if (y % StalkerClass.DDSImage.S_X == 0)
                {
                    count_y++;
                    if (y >= _y && y <= _y + StalkerClass.DDSImage.S_X)
                        break;
                }
            }
            if (count_y == -1)
                count_y = 0;
            //  dds_selected_in = new Point(dds_selected_in.x)


            int max_x = -1;
            int max_y = -1;
            if (w > _x)
                max_x = w;
            else
                max_x = _x;
            if (h > _y)
                max_y = h;
            else
                max_y = _y;
            int count_w = 1;
            int count_h = 1;
            for (int x = _x; x <= max_x; x++)
                if (x % StalkerClass.DDSImage.S_X == 0)
                    count_w++;
            for (int y = _y; y <= max_y; y++)
                if (y % StalkerClass.DDSImage.S_Y == 0)
                    count_h++;

            float __x = (float)(count_x * StalkerClass.DDSImage.S_X);
            float __y = (float)(count_y * StalkerClass.DDSImage.S_Y);
            X_OFFSET = __x;
            Y_OFFSET = __y;

            this.Title = $"Точка: {count_x} {count_y}";

        }

        private float Size_Width = 250;
        private float Size_Heigth = 100;

        private void btn_add_icon_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Png|*.png|All Files|*.*";
            if(open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InputAddPng = open.FileName;
                this.Title = $"Добавление: {new System.IO.FileInfo(open.FileName).Name}";
                StalkerClass.DDSImage dds = new StalkerClass.DDSImage(System.IO.File.ReadAllBytes(InputDDS));
                System.Drawing.Bitmap bts = dds.BitmapImage;
                Graphics grap = Graphics.FromImage(bts);

                Bitmap addBit = new Bitmap(InputAddPng);

                StalkerWin.Dialogs.EnterSizeWin enSize = new Dialogs.EnterSizeWin("Введите размер изображения для вставки");
                enSize.Owner = this;
                enSize.ShowDialog();
                if(enSize.IsOk)
                {
                    Size_Width = enSize.Si_Width;
                    Size_Heigth = enSize.Si_Heigth;
                }

                grap.DrawImage(addBit, X_OFFSET, Y_OFFSET,Size_Width,Size_Heigth);
                grap.Save();
                // grap.Save();
                image.Source = MainWindow.BitmapToImageSource(bts);

            }
        }

        private void btn_save_icon_Click(object sender, RoutedEventArgs e)
        {
            if (InputAddPng == null)
                return;
            
            Bitmap bts = MainWindow.FromSourceImage((BitmapImage)image.Source);
            string workDir = $"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\python\\";
            if (System.IO.File.Exists(workDir + "\\tmp.png"))
                System.IO.File.Delete(workDir + "\\tmp.png");
            if (System.IO.File.Exists(workDir + "\\tmpDDS.png"))
                System.IO.File.Delete(workDir + "\\tmpDDS.png");
            if (System.IO.File.Exists(workDir + "\\tmp.dds"))
                System.IO.File.Delete(workDir + "\\tmp.dds");


            bts.Save(workDir + "\\tmp.png", System.Drawing.Imaging.ImageFormat.Png);


            System.IO.File.WriteAllText($"{workDir}\\bat.bat", $"@chcp 1251{Environment.NewLine}cd {workDir}{Environment.NewLine}DDS.exe dtx3 dds tmp.png tmpDDS.png", Encoding.Default);

            ProcessStartInfo proc = new ProcessStartInfo($"{workDir}\\bat.bat") { CreateNoWindow = false,UseShellExecute = false,RedirectStandardOutput = true };
            Process prs = Process.Start(proc);

            string txt = prs.StandardOutput.ReadToEnd();

            if(txt.Contains("code: 0"))
            {
                MessageBox.Show($"Иконка \"{new System.IO.FileInfo(InputAddPng).Name}\" Успешно добавлена!", "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                System.IO.File.Copy(workDir + "\\tmpDDS.png", workDir + "\\tmp.dds");
                System.IO.File.Copy(workDir + "\\tmp.dds", InputDDS, true);
            }
             

          //  System.IO.File.Copy(workDir + "\\tmpDDS.png", workDir + "\\tmp.dds");

            
            
        }
    }
}
