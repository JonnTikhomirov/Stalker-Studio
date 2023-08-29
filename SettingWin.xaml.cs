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

namespace Stalker_Studio
{
    /// <summary>
    /// Логика взаимодействия для SettingWin.xaml
    /// </summary>
    public partial class SettingWin : Window
    {
        public SettingWin()
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);

            Init_Def();
            Init_VID();
            Init_Import();
            Init_Hints();
        }

        private void Init_Def()
        {
            encoding_Ltx.SelectedIndex = Properties.Settings.Default.Encoding_LTX;
            encoding_XML.SelectedIndex = Properties.Settings.Default.Encoding_XML;


            check_autosave.IsChecked = Properties.Settings.Default.AutoSave;


            txt_link_icon.Text = Properties.Settings.Default.ui_icon;

            check_mode_ltx.IsChecked = Properties.Settings.Default.Mode_ltx_form;

            check_all_param.IsChecked = Properties.Settings.Default.Ltx_all_param;
            check_replace_link.IsChecked = Properties.Settings.Default.Replace_Link;
            links.Text = Properties.Settings.Default.text_Links;
            check_usePrm.IsChecked = Properties.Settings.Default.UseTranslatePrm;
            check_useHints.IsChecked = Properties.Settings.Default.UseHints;
        }

        private void Init_Import()
        {
            txt_importG.Text = Properties.Settings.Default.ImportPath;

        }

        private void Init_VID()
        {
            check_see_setka.IsChecked = MainWindow.ProgramData.Show_Setka_DDS;
            txt_setka_x.Text = StalkerClass.DDSImage.S_X.ToString();
            txt_setka_y.Text = StalkerClass.DDSImage.S_Y.ToString();

            comboMode.SelectedIndex = Properties.Settings.Default.browser_mode;
            comboSection.SelectedIndex = Properties.Settings.Default.section_mode;
            check_markers.IsChecked = Properties.Settings.Default.UseMarkers;
        }

        private void Init_Hints()
        {
            check_loadSection.IsChecked = Properties.Settings.Default.LoadSectHints;
            check_startPos.IsChecked = Properties.Settings.Default.StatPointHints;
            check_loadinfoportion.IsChecked = Properties.Settings.Default.LoadInfoportionHints;
        }



        //private void HideAll()
        //{
        //    grid_default_setting.Visibility = Visibility.Hidden;
        //    grid_vid_setting.Visibility = Visibility.Hidden;
        //    grid_import_setting.Visibility = Visibility.Hidden;
        //    grid_ignore_setting.Visibility = Visibility.Hidden;
        //    grid_hints_setting.Visibility = Visibility.Hidden;
        //}


        //private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if(lst_setting.SelectedIndex != -1)
        //    {
        //        HideAll();
        //        if(lst_setting.SelectedIndex == 0)
        //        {
        //            grid_default_setting.Visibility = Visibility.Visible;
        //        }
        //        else if(lst_setting.SelectedIndex == 1)
        //        {
        //            grid_vid_setting.Visibility = Visibility.Visible;
        //        }
        //        else if(lst_setting.SelectedIndex == 2)
        //        {
        //            grid_import_setting.Visibility = Visibility.Visible;
        //        }
        //        else if (lst_setting.SelectedIndex == 3)
        //        {
        //            grid_ignore_setting.Visibility = Visibility.Visible;
        //        }
        //        else if (lst_setting.SelectedIndex == 4)
        //        {
        //            grid_hints_setting.Visibility = Visibility.Visible;
        //        }
        //    }
        //}

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Here we save all setting
            try
            {
                MainWindow.ProgramData.Show_Setka_DDS = (bool)check_see_setka.IsChecked;
                StalkerClass.DDSImage.S_X = int.Parse(txt_setka_x.Text);
                StalkerClass.DDSImage.S_Y = int.Parse(txt_setka_y.Text);
                StalkerClass.DDSImage.S_W = int.Parse(txt_setka_x.Text);
                StalkerClass.DDSImage.S_H = int.Parse(txt_setka_y.Text);
                Properties.Settings.Default.browser_mode = comboMode.SelectedIndex;
                Properties.Settings.Default.section_mode = comboSection.SelectedIndex;
                Properties.Settings.Default.UseMarkers = (bool)check_markers.IsChecked;

                Properties.Settings.Default.Encoding_XML = encoding_XML.SelectedIndex;
                Properties.Settings.Default.Encoding_LTX = encoding_Ltx.SelectedIndex;
                Properties.Settings.Default.ui_icon = txt_link_icon.Text;
                Properties.Settings.Default.AutoSave = (bool)check_autosave.IsChecked;
                Properties.Settings.Default.Mode_ltx_form = (bool)check_mode_ltx.IsChecked;
                Properties.Settings.Default.Ltx_all_param = (bool)check_all_param.IsChecked;

                Properties.Settings.Default.Replace_Link = (bool)check_replace_link.IsChecked;
                Properties.Settings.Default.text_Links = links.Text;
                Properties.Settings.Default.UseTranslatePrm = (bool)check_usePrm.IsChecked;
                Properties.Settings.Default.UseHints = (bool)check_useHints.IsChecked;
                Properties.Settings.Default.LoadSectHints = (bool)check_loadSection.IsChecked;
                Properties.Settings.Default.StatPointHints = (bool)check_startPos.IsChecked;
                Properties.Settings.Default.LoadInfoportionHints = (bool)check_loadinfoportion.IsChecked;
                Properties.Settings.Default.ImportPath = txt_importG.Text;

                MainWindow.ProgramData.InitializeComponentEncoding();

                Properties.Settings.Default.Save();

                MessageBox.Show("Сохранение успешно!", "Параметры", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception g)
            {
                MessageBox.Show($"Ошибка при сохранении!\n{g.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_sel_icon_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ope = new System.Windows.Forms.OpenFileDialog();
            ope.Filter = "*.dds|*.dds|All Files|*.*";
            ope.InitialDirectory = MainWindow.ProgramData.Gamedata;
            if(ope.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_link_icon.Text = ope.FileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            FileInfo fls = new System.IO.FileInfo($"{System.Windows.Forms.Application.StartupPath}\\Markers.ltx");
            File.WriteAllText(fls.FullName, $"[Markers]");
            MessageBox.Show("Все маркеры убраны", "Маркеры очищены", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn_dialog_import_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(txt_importG.Text))
                fold.SelectedPath = txt_importG.Text;
            fold.Description = "Выберите основную директорию для импорта ресурсов в проект";
            if(fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_importG.Text = fold.SelectedPath;
            }
        }

        private void MN_IMPORT_PASTE_Click(object sender, RoutedEventArgs e)
        {
            if(Clipboard.GetText() != null)
            {
                txt_importG.Text = Clipboard.GetText();
            }
        }

        private void MN_IMPORT_GO_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(txt_importG.Text))
            {
                System.Diagnostics.Process.Start(txt_importG.Text);
            }
        }

        private void MN_IMPORT_COPY_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(txt_importG.Text))
            {
                Clipboard.SetText(txt_importG.Text);
            }
        }

        private void MN_IGNORE_GO_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(txt_importG.Text))
            {
                MainWindow.ProgramData.MainWinThread.txtSelectGamedata.Text = txt_importG.Text;
                Properties.Settings.Default.ImportPath = txt_importG.Text;
                Properties.Settings.Default.Save();
                MainWindow.ProgramData.MainWinThread.SetSelectGamedata();
                this.Close();
            }
        }

        private void btn_ignore_clear_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IgnorePather = "";
            Properties.Settings.Default.Save();
            MessageBox.Show("Все пути которые игнорировались были убраны!", "Убраны пути", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
