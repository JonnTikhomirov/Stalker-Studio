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
using System.IO;

namespace Stalker_Studio.StalkerWin.convert
{
    /// <summary>
    /// Логика взаимодействия для ConvertPythonWin.xaml
    /// </summary>
    public partial class ConvertPythonWin : Window
    {
        public ConvertPythonWin(string pathW)
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            if (File.Exists(pathW))
                PathWork = pathW;
            else if (Directory.Exists(pathW))
            {
                PathWork = pathW;
                if(Directory.Exists(pathW.TrimEnd('\\') + "\\textures"))
                {
                    PathWork = pathW.TrimEnd('\\') + "\\textures";
                }

            }
            else
            {
                if(MessageBox.Show("Выбрать папку для работы?","Внимание отсутствует корневой каталог",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
                    fold.Description = "Папка с текстурами";
                    if(fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        PathWork = fold.SelectedPath;
                         if (Directory.Exists(pathW))
                        {
                            PathWork = pathW;
                            if (Directory.Exists(pathW.TrimEnd('\\') + "\\textures"))
                            {
                                PathWork = pathW.TrimEnd('\\') + "\\textures";
                            }

                        }
                    }
                }
            }

        }
        private string PathWork;
        private string FileImage;
        private void InitBrowser()
        {
            if (Directory.Exists(PathWork))
            {
                MainWindow.ProgramData.LoadDataBrowser(PathWork, tree_browser, null, this, false);
            }
        }

        private void SetTitle(string AddIO)
        {
            if (AddIO != null)
                this.Title = $"Конверт .dds | .png ({AddIO})";
            else
                this.Title = "Конверт .dds | .png";
        }

        private async void OpenImage(string file)
        {
            FileInfo fls = new FileInfo(file);
            if (fls.Name.ToUpper().EndsWith(".dds".ToUpper()))
            {
                await Task.Run(() =>
                {
                    StalkerClass.DDSImage dds = new StalkerClass.DDSImage(File.ReadAllBytes(file));
                    img.Dispatcher.Invoke(new Action(() =>
                    {
                        img.Source = MainWindow.BitmapToImageSource(dds.BitmapImage);
                    }));
                    
                });
                FileImage = file;
                
                
                SetTitle(fls.Name);
                combo_mode.SelectedIndex = 1;
            }
            else if (fls.Name.ToUpper().EndsWith(".png".ToUpper()))
            {
                FileImage = file;
                img.Source = MainWindow.BitmapToImageSource(new System.Drawing.Bitmap(file));
                SetTitle(fls.Name);
                combo_mode.SelectedIndex = 0;
            }
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (img.Source != null)
            {
                if (combo_mode.SelectedIndex == 0)
                {
                    //to dds
                    System.Windows.Forms.SaveFileDialog sa = new System.Windows.Forms.SaveFileDialog();
                    sa.Filter = "Direct Textures|*.dds";
                    if (sa.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        //to png

                        System.Drawing.Bitmap bit = MainWindow.FromSourceImage((BitmapImage)img.Source);
                        bit.Save(sa.FileName);
                        MessageBox.Show($"Файл \"{MainWindow.ProgramData.GetLastSplash(sa.FileName)}\" Успешно сохранён!", "Файл сохранён", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (combo_mode.SelectedIndex == 1)
                {
                    System.Windows.Forms.SaveFileDialog sa = new System.Windows.Forms.SaveFileDialog();
                    sa.Filter = "PNG|*.png";
                    if (sa.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        //to png
                        StalkerClass.DDSImage dds = new StalkerClass.DDSImage(File.ReadAllBytes(FileImage));
                        dds.BitmapImage.Save(sa.FileName);
                        MessageBox.Show($"Файл \"{MainWindow.ProgramData.GetLastSplash(sa.FileName)}\" Успешно сохранён!", "Файл сохранён", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
            }
        }

        private void btn_openFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Direct Textures|*.dds|PNG|*.png";
            if(open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenImage(open.FileName);
            }

        }

        private void tree_browser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(tree_browser.SelectedItem != null)
            {
                if(File.Exists((tree_browser.SelectedItem as TreeViewItem).Tag.ToString()))
                {
                    OpenImage((tree_browser.SelectedItem as TreeViewItem).Tag.ToString());
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PathWork))
            {
                InitBrowser();
            }
            else if (File.Exists(PathWork))
            {
                OpenImage(PathWork);
            }
        }
    }
}
