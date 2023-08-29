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
using System.Threading;

namespace Stalker_Studio.StalkerWin
{
    /// <summary>
    /// Логика взаимодействия для LoggerFinder.xaml
    /// </summary>
    public partial class LoggerFinder : Window
    {
        public LoggerFinder()
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            Thread thAliver = new Thread(MainThread);
            thAnimation = new Thread(AnimationThread);
            thAnimation.Start();
            thAliver.Start();
        }
        Thread thAnimation;

        public void AnimationThread()
        {
            try
            {
                string dir = $"{System.Windows.Forms.Application.StartupPath}\\Data\\Image\\anim";
                string[] anmFrames = new string[] { dir + "\\0.png", dir + "\\1.png", dir + "\\2.png", dir + "\\3.png", dir + "\\4.png", dir + "\\5.png", dir + "\\6.png", dir + "\\7.png" };
                int counter = 0;
                while (true)
                {
                    img_anm.Dispatcher.Invoke(new Action(() =>
                    {
                        img_anm.Source = MainWindow.BitmapToImageSource(new System.Drawing.Bitmap(anmFrames[counter]));
                    }));

                    if (counter < anmFrames.Length-1)
                        counter++;
                    else
                        counter = 0;
                    Thread.Sleep(1000);
                }
            }
            catch
            {
                string dir = $"{System.Windows.Forms.Application.StartupPath}\\Data\\Image\\anim\\None.png";

                img_anm.Dispatcher.Invoke(new Action(() =>
                {
                    img_anm.Source = MainWindow.BitmapToImageSource(new System.Drawing.Bitmap(dir));
                }));
            }
        }

        public void MainThread()
        {
            while (true)
            {
                string text = null;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    text = Clipboard.GetText();
                }));

                if (text != null && text.ToUpper().Contains("Expression".ToUpper()) || text.ToUpper().Contains("Function".ToUpper()))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Clipboard.SetText("");
                    }));

                    this.lab_log.Dispatcher.Invoke(new Action(() =>
                    {
                        this.lab_log.Text = text;
                    }));

                    if (thAnimation.IsAlive)
                    {
                        thAnimation.Abort();
                    }


                  //  MessageBox.Show(text, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                    //break point
                }
                System.Threading.Thread.Sleep(1500);
            }
        }
    }
}
