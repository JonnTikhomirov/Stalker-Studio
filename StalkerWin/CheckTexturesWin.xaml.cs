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


namespace Stalker_Studio.StalkerWin
{
    /// <summary>
    /// Логика взаимодействия для CheckTexturesWin.xaml
    /// </summary>
    public partial class CheckTexturesWin : Window
    {
        public CheckTexturesWin(string PathWork)
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);

            if (!Directory.Exists(PathWork))
                return;

            //need path to gamedata
            string gamedata = "";
        HERE:

            for (int i = 0; i < PathWork.Split('\\').Length; i++)
            {
                if (PathWork.Split('\\')[i].ToUpper().Contains("gamedata".ToUpper()))
                {
                    gamedata += PathWork.Split('\\')[i];
                    break;
                }
                else
                    gamedata += PathWork.Split('\\')[i] + "\\";
            }

            if (!gamedata.EndsWith("gamedata"))
            {
                if (MessageBox.Show("Извольти выбрать полный путь к gamedata\nЧтобы найти папки для текстур,моделей,звука", "Полный путь", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    System.Windows.Forms.FolderBrowserDialog fold = new System.Windows.Forms.FolderBrowserDialog();
                    if (fold.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        gamedata = fold.SelectedPath;
                        goto HERE;
                    }
                }
                else
                    return;
            }

            WorkPath = gamedata;

        }

        private string WorkPath;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string meshes = $"{WorkPath}\\meshes";
            if (!Directory.Exists(meshes))
            {
                MessageBox.Show("Путь к моделям отсутствует!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                Console.WriteLine(meshes);   
                MainWindow.ProgramData.LoadDataBrowser(meshes, treeBrowser);
            }
        }

        private List<string> GetPathTexturesByModel(FileInfo pathModels)
        {

            if (File.Exists($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\ogfmodel.mtl"))
                File.Delete($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\ogfmodel.mtl");

            List<string> ls = new List<string>();
            string convert = $"{System.Windows.Forms.Application.StartupPath}\\Convertor\\ogf2obj.exe";

            if (!File.Exists($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\{pathModels.Name}"))
            {
                File.Copy(pathModels.FullName, $"{System.Windows.Forms.Application.StartupPath}\\Convertor\\{pathModels.Name}");
            }


            string bat = $"cd Convertor{Environment.NewLine}ogf2obj.exe {pathModels.Name} invoke.obj";


            File.WriteAllText($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\bat.bat", bat, Encoding.Default);


            ProcessStartInfo batS = new ProcessStartInfo($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\bat.bat") { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true };

            Process proc = Process.Start(batS);

            proc.StandardOutput.ReadToEnd();



            if (File.Exists($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\ogfmodel.mtl"))
            {
                string[] fls = File.ReadAllLines($"{System.Windows.Forms.Application.StartupPath}\\Convertor\\ogfmodel.mtl", Encoding.UTF8);
                foreach (var v in fls)
                {
                    if (v.Contains("\\") || v.Contains(".dds"))
                    {
                        if (ls.Where(x => x == v.Trim().Replace("Map_Kd", "").Replace("Map_Ka", "").Trim()).Count() <= 0)
                            ls.Add(v.Trim().Replace("Map_Kd", "").Replace("Map_Ka", "").Trim());
                    }
                }
            }



            return ls;
        }

        private void TreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(treeBrowser.SelectedItem != null)
            {
                string pathOgf = ((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString();
                string textures = $"{WorkPath}\\textures";
                if (pathOgf.EndsWith(".ogf"))
                {
                    txt_not_found.Text = "";

                    List<string> Gets = GetPathTexturesByModel(new FileInfo(pathOgf));
                    bool have = false;
                    foreach(var v in Gets)
                    {
                        if(!File.Exists(textures + "\\" + v))
                        {
                            txt_not_found.Foreground = new SolidColorBrush(Colors.Red);
                            txt_not_found.Text += v + Environment.NewLine;
                            have = true;
                        }
                    }

                    if (!have)
                    {
                        txt_not_found.Foreground = new SolidColorBrush(Colors.Green);
                        txt_not_found.Text = "Все текстуры найдены.";
                    }

                    
                }
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_import_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Properties.Settings.Default.ImportPath))
            {
                List<string> notFounds = txt_not_found.Text.Split('\n').ToList();
                notFounds.Remove(notFounds.Where(x => string.IsNullOrWhiteSpace(x)).First());
                
                string vrk = MainWindow.ProgramData.GetGamedataFromInCatalogs(MainWindow.ProgramData.Gamedata);
                string p = MainWindow.ProgramData.GetGamedataFromInCatalogs(Properties.Settings.Default.ImportPath);
                if (Directory.Exists(vrk + "\\textures"))
                    vrk += "\\textures";
                if (Directory.Exists(p + "\\textures"))
                    p += "\\textures";

                string[] array = notFounds.ToArray();
                for(int i = 0; i < array.Length; i++)
                {
                    array[i] = "textures\\" + array[i].Trim();
                }
                
                Import.ImportFile_Win win = new Import.ImportFile_Win(vrk,array,p);
                if(MessageBox.Show("Если путь игнорируется то импорт не возможен\nВы можете убрать игнорирование в ручную\nУбрать полностью игнорирование?(может занять время)","Не использовать игнорирование",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    win.UseIgnore = false;

                }
                win.Owner = this;
                win.Show();
            }
            else
            {
                MessageBox.Show("Выберите каталог для импорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
