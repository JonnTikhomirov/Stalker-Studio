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

namespace Stalker_Studio.StalkerWin.convert.conv
{
    /// <summary>
    /// Логика взаимодействия для OutputDirWinConv.xaml
    /// </summary>
    public partial class OutputDirWinConv : Window
    {
        public OutputDirWinConv(string defPath = "")
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            txt_path.Text = defPath;
            InitPather();
        }

        private int GetCountFilesRecursive(string path, int counter = 0)
        {
            DirectoryInfo dirs = new DirectoryInfo(path);
            counter += dirs.GetFiles().Length;

            foreach (var v in dirs.GetDirectories())
               counter += GetCountFilesRecursive(v.FullName);
            return counter;
        }

        private async void InitPather()
        {
            string txtPath = txt_path.Text;
            string oriTitle = this.Title;
            lab_counter_files.Content = "Количество файлов: -";
            this.Title = oriTitle + $" (Please wait...)";

            btn_select_path.IsEnabled = false;
            txt_path.IsEnabled = false;

            await Task.Run(() =>
            {
                if (Directory.Exists(txtPath))
                {
                    //Количество файлов:
                    int a = GetCountFilesRecursive(txtPath);
                    lab_counter_files.Dispatcher.Invoke(new Action(() =>
                    {
                        lab_counter_files.Content = $"Количество файлов: {a}";
                    }));
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.Title = oriTitle;
                }));

                btn_select_path.Dispatcher.Invoke(new Action(() =>
                {
                    btn_select_path.IsEnabled = !false;
                }));
                txt_path.Dispatcher.Invoke(new Action(() =>
                {
                    txt_path.IsEnabled = !false;
                }));
            });
        }

        private void txt_path_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                InitPather();
            }

        }

        private void btn_select_path_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
            fold.Description = "Папка с ресурсами";
            if (Directory.Exists(txt_path.Text))
                fold.SelectedPath = txt_path.Text;

            if(fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_path.Text = fold.SelectedPath;
                InitPather();
            }

        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_invoke_Click(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(txt_path.Text) && comboMode.SelectedIndex != -1)
            {
                int counterFileUse = -1;
                ConvertExeInvoke.format_specific_option option = ConvertExeInvoke.format_specific_option.None;
                if(comboMode.SelectedIndex == 0)
                {
                    option = ConvertExeInvoke.format_specific_option.ogg2wav;
                    if (Directory.Exists($"{txt_path.Text.TrimEnd('\\')}\\sounds"))
                        counterFileUse = GetCountFilesRecursive($"{txt_path.Text.TrimEnd('\\')}\\sounds");
                }
                if(comboMode.SelectedIndex == 1)
                {
                    option = ConvertExeInvoke.format_specific_option.dds2tga;
                    if (Directory.Exists($"{txt_path.Text.TrimEnd('\\')}\\textures"))
                        counterFileUse = GetCountFilesRecursive($"{txt_path.Text.TrimEnd('\\')}\\textures");
                }
                
                if(option != ConvertExeInvoke.format_specific_option.None)
                {
                    
                    FileInfo vsF = ConvertExeInvoke.Convert("", "", new ConvertExeInvoke.format_specific_option[] { option }, txt_path.Text);
                    if(counterFileUse != -1)
                        lab_counter_files.Content = $"Количество файлов: {counterFileUse}";
                }
                else
                {
                    MessageBox.Show("Режим не выбран!", "Выберите режим", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
