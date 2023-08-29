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
using System.Diagnostics;

namespace Stalker_Studio.StalkerWin.convert.ogf
{
    /// <summary>
    /// Логика взаимодействия для ogf_objectWin.xaml
    /// </summary>
    public partial class ogf_objectWin : Window
    {
        /// <summary>
        /// Окно преобразует ogf в object конвертор: convert.exe
        /// </summary>
        /// <param name="pathToMeshes"></param>
        public ogf_objectWin(string pathToMeshes)
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            DirMeshes = pathToMeshes;

            combo_two.Visibility = Visibility.Hidden;
            combo_three.Visibility = Visibility.Hidden;
        }

        private string DirMeshes;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(DirMeshes))
            {
                MainWindow.ProgramData.LoadDataBrowser(DirMeshes, treeBrowser, null, this,false);
            }
            else
            {
                if (MessageBox.Show("Выберите папку с моделями", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
                    if (fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (Directory.Exists(fold.SelectedPath + "\\meshes"))
                            DirMeshes = fold.SelectedPath += "\\meshes";
                        else
                            DirMeshes = fold.SelectedPath;

                        if (Directory.Exists(DirMeshes))
                        {
                            MainWindow.ProgramData.LoadDataBrowser(DirMeshes, treeBrowser, null, this, false);
                        }
                    }
                    else
                        this.Close();
                }
                else
                    this.Close();
            }
        }

        private void treeBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(treeBrowser.SelectedItem != null && File.Exists((treeBrowser.SelectedItem as TreeViewItem).Tag.ToString()))
            {
                lab_invoke.Text = (treeBrowser.SelectedItem as TreeViewItem).Tag.ToString();

                if (File.Exists(txt_output.Text))
                {
                    return;
                }

                if (Directory.Exists(txt_output.Text))
                {
                   // txt_output.Text += txt_output.Text.Trim('\\') + "\\" + MainWindow.ProgramData.GetLastSplash(lab_invoke.Text.ToString().Replace("ogf","object"));
                }
                else
                {
                   // txt_output.Text += MainWindow.ProgramData.GetLastSplash(lab_invoke.Text.ToString().Replace("ogf", "object"));
                }
            }
        }

        private void btn_output_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
            fold.SelectedPath = txt_output.Text;
            if(fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_output.Text = fold.SelectedPath;
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_invoke_Click(object sender, RoutedEventArgs e)
        {
            if (combo_main.SelectedIndex != -1 && treeBrowser.SelectedItem != null)
            {
                if(!Directory.Exists(txt_output.Text) && !File.Exists(txt_output.Text))
                {
                    txt_output.Text = System.Windows.Forms.Application.StartupPath;
                }

                string invokeFile = (treeBrowser.SelectedItem as TreeViewItem).Tag.ToString();
                string outputFile = txt_output.Text.TrimEnd('\\') + "\\" + MainWindow.ProgramData.GetLastSplash(lab_invoke.Text);
                string dirWork = $"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\Convert\\converter_2015-06-24\\converter\\win32\\";
                if (File.Exists(dirWork + "\\" + MainWindow.ProgramData.GetLastSplash(invokeFile)))
                    File.Delete(dirWork + "\\" + MainWindow.ProgramData.GetLastSplash(invokeFile));

                File.Copy(invokeFile, dirWork + "\\" + MainWindow.ProgramData.GetLastSplash(invokeFile));
                invokeFile = dirWork + "\\" + MainWindow.ProgramData.GetLastSplash(invokeFile);

                if (string.IsNullOrWhiteSpace(MainWindow.ProgramData.GetLastSplash(outputFile)))
                    outputFile = System.Windows.Forms.Application.StartupPath + "\\" + outputFile;

                List<ConvertExeInvoke.format_specific_option> options = new List<ConvertExeInvoke.format_specific_option>();
                if(combo_main.SelectedIndex == 0)
                {
                    options.Add(ConvertExeInvoke.format_specific_option.ogf);
                }
                if (combo_main.SelectedIndex == 1)
                {
                    options.Add(ConvertExeInvoke.format_specific_option.omf);
                }
                if (combo_main.SelectedIndex == 2)
                {
                    options.Add(ConvertExeInvoke.format_specific_option.dm);
                }
                if (combo_main.SelectedIndex == 3)
                {
                    options.Add(ConvertExeInvoke.format_specific_option.level);
                }
                if (combo_main.SelectedIndex == 4)
                {
                    options.Add(ConvertExeInvoke.format_specific_option.ogg2wav);
                }
                if (combo_main.SelectedIndex == 5)
                {
                    options.Add(ConvertExeInvoke.format_specific_option.dds2tga);
                }

                if(combo_two.Visibility == Visibility.Visible && combo_two.SelectedIndex != -1)
                {
                    ConvertExeInvoke.format_specific_option frms = (ConvertExeInvoke.format_specific_option)((combo_two.SelectedItem as ComboBoxItem).Tag);
                    if(frms != ConvertExeInvoke.format_specific_option.None)
                    {
                        options.Add(frms);
                    }
                }
                if (combo_three.Visibility == Visibility.Visible && combo_three.SelectedIndex != -1)
                {
                    ConvertExeInvoke.format_specific_option frms = (ConvertExeInvoke.format_specific_option)((combo_three.SelectedItem as ComboBoxItem).Tag);
                    if (frms != ConvertExeInvoke.format_specific_option.None)
                    {
                        options.Add(frms);
                    }
                }



                FileInfo fileToCopy = ConvertExeInvoke.Convert((treeBrowser.SelectedItem as TreeViewItem).Tag.ToString(), outputFile, options.ToArray());
                if(fileToCopy == null)
                {
                    MessageBox.Show("Ошибка конверта!\nИнформацию смотреть в консоли", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if(File.Exists(txt_output.Text.TrimEnd('\\') + "\\" + fileToCopy.Name))
                    {
                        File.Delete(txt_output.Text.TrimEnd('\\') + "\\" + fileToCopy.Name);
                    }

                    File.Copy(fileToCopy.FullName, txt_output.Text.TrimEnd('\\') + "\\" + fileToCopy.Name);

                    MessageBox.Show($"Файл \"{fileToCopy.Name}\" успешно конвертирован!", $"Конверт успешен!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
        }

        private void combo_main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            combo_three.Visibility = Visibility.Hidden;
            combo_two.Visibility = Visibility.Hidden;

            combo_two.Items.Clear();
            combo_three.Items.Clear();

            int index = combo_main.SelectedIndex;
            if(index != -1)
            {
                if(index == 0)
                {
                    combo_two.Visibility = Visibility.Visible;
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = "object";
                    item.Tag = ConvertExeInvoke.format_specific_option.@object;
                    combo_two.Items.Add(item);

                    ComboBoxItem item2 = new ComboBoxItem();
                    item2.Content = "skls";
                    item2.Tag = ConvertExeInvoke.format_specific_option.skls;
                    combo_two.Items.Add(item2);

                    ComboBoxItem item3 = new ComboBoxItem();
                    item3.Content = "skl";
                    item3.Tag = ConvertExeInvoke.format_specific_option.skl;
                    combo_two.Items.Add(item3);

                    ComboBoxItem item4 = new ComboBoxItem();
                    item4.Content = "bones";
                    item4.Tag = ConvertExeInvoke.format_specific_option.bones;
                    combo_two.Items.Add(item4);
                }
                if (index == 1)
                {
                    combo_two.Visibility = Visibility.Visible;

                    ComboBoxItem item2 = new ComboBoxItem();
                    item2.Content = "skls";
                    item2.Tag = ConvertExeInvoke.format_specific_option.skls;
                    combo_two.Items.Add(item2);

                    ComboBoxItem item3 = new ComboBoxItem();
                    item3.Content = "skl";
                    item3.Tag = ConvertExeInvoke.format_specific_option.skl;
                    combo_two.Items.Add(item3);
                }

                if (index == 3)
                {
                    combo_two.Visibility = Visibility.Visible;
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = "mode maya";
                    item.Tag = ConvertExeInvoke.format_specific_option.mode_maya;
                    combo_two.Items.Add(item);

                    ComboBoxItem item2 = new ComboBoxItem();
                    item2.Content = "mode le";
                    item2.Tag = ConvertExeInvoke.format_specific_option.mode_le;
                    combo_two.Items.Add(item2);

                    ComboBoxItem item3 = new ComboBoxItem();
                    item3.Content = "mode l2";
                    item3.Tag = ConvertExeInvoke.format_specific_option.mode_le2;
                    combo_two.Items.Add(item3);


                    combo_three.Visibility = Visibility.Visible;
                    ComboBoxItem it = new ComboBoxItem();
                    it.Content = "with lods";
                    it.Tag = ConvertExeInvoke.format_specific_option.with_lods;
                    combo_three.Items.Add(it);

                    ComboBoxItem it2 = new ComboBoxItem();
                    it2.Content = "None";
                    it2.Tag = ConvertExeInvoke.format_specific_option.None;
                    combo_three.Items.Add(it2);
                }
                if (index == 5)
                {
                    combo_two.Visibility = Visibility.Visible;
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = "with solid";
                    item.Tag = ConvertExeInvoke.format_specific_option.with_solid;
                    combo_two.Items.Add(item);
                    ComboBoxItem item2 = new ComboBoxItem();
                    item2.Content = "None";
                    item2.Tag = ConvertExeInvoke.format_specific_option.None;
                    combo_two.Items.Add(item2);

                    combo_three.Visibility = Visibility.Visible;
                    ComboBoxItem it = new ComboBoxItem();
                    it.Content = "with bump";
                    it.Tag = ConvertExeInvoke.format_specific_option.with_bump;
                    combo_three.Items.Add(it);
                    ComboBoxItem it2 = new ComboBoxItem();
                    it2.Content = "None";
                    it2.Tag = ConvertExeInvoke.format_specific_option.None;
                    combo_three.Items.Add(it2);
                }
            }
        }


        private void openSpecFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(MainWindow.ProgramData.Gamedata))
                fold.SelectedPath = MainWindow.ProgramData.Gamedata;
            fold.Description = "Выбор папки с файлами для конверта";
            if(fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirMeshes = fold.SelectedPath;
                MainWindow.ProgramData.LoadDataBrowser(DirMeshes, treeBrowser, null, this, false);
            }
        }
    }
}
