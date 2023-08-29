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

namespace Stalker_Studio.StalkerWin
{
    /// <summary>
    /// Логика взаимодействия для ReplaceOldLinkWin.xaml
    /// </summary>
    public partial class ReplaceOldLinkWin : Window
    {
        public ReplaceOldLinkWin(string workPath,string excluseFile = null)
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            WorkPath = workPath;
            ExcluseFile = excluseFile;
            comMode.SelectedIndex = 0;
        }

        private string WorkPath;
        private string ExcluseFile = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_invoke_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txt_old_link.Text) && !string.IsNullOrWhiteSpace(txt_new_link.Text))
            {
                int counter = 0;
                StalkerHierarchyElement.FinderElement foi = new StalkerHierarchyElement.FinderElement(WorkPath, MainWindow.ProgramData.Encoding_XML,MainWindow.ProgramData.Encoding_LTX);
                if (comMode.SelectedIndex == 0)
                {
                    List<FileInfo> vs = foi.GetGlobalFilesLtx(new string[] { $"#include", txt_old_link.Text });

                    foreach (var v in vs)
                    {

                        if (ExcluseFile != null && MainWindow.ProgramData.GetLastSplash(ExcluseFile) == v.Name)
                            continue;
                        string[] str = File.ReadAllLines(v.FullName, MainWindow.ProgramData.Encoding_LTX);
                        for (int i = 0; i < str.Length; i++)
                        {
                            if (str[i].Trim().StartsWith("#include") && str[i].Trim().Contains(txt_old_link.Text))
                            {
                                str[i] = str[i].Replace($"#include \"{txt_old_link.Text}\"", $"#include \"{txt_new_link.Text}\"");
                                counter++;
                            }
                        }
                        File.WriteAllLines(v.FullName, str, MainWindow.ProgramData.Encoding_LTX);
                    }
                }
                else if(comMode.SelectedIndex == 1)
                {
                    List<FileInfo> vs = foi.GetGlobalFilesLtx(new string[] { txt_old_link.Text });
                    foreach(var v in vs)
                    {
                        if (ExcluseFile != null && MainWindow.ProgramData.GetLastSplash(ExcluseFile) == v.Name)
                            continue;
                        string txt = File.ReadAllText(v.FullName, MainWindow.ProgramData.Encoding_LTX);
                        txt = txt.Replace(txt_old_link.Text, txt_new_link.Text);
                        File.WriteAllText(v.FullName, txt,MainWindow.ProgramData.Encoding_LTX);
                        counter++;
                    }
                }
                MessageBox.Show($"Замена ссылок успешна!\nКол-во замен: {counter}", "Замена ссылок", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void comMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(comMode.SelectedIndex == 0)
            {
                textblock.Text = "Замена ссылок в файлах.\n#include \"old\" на #include \"new\"";
            }
            else if(comMode.SelectedIndex == 1)
            {
                textblock.Text = "Полное замены \"old\" на \"new\".\nБез учёта \"#include\"";
            }
        }
    }
}
