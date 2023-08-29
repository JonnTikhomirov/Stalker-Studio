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

namespace Stalker_Studio.StalkerWin.Import
{
    /// <summary>
    /// Логика взаимодействия для ImportFile_Win.xaml
    /// </summary>
    public partial class ImportFile_Win : Window
    {
        public ImportFile_Win(string pathProject, string[] pathSelected = null,string obrazPath = null)
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            PathProjectResources = pathProject;
            Selected = pathSelected;
            if (Directory.Exists(obrazPath))
                Properties.Settings.Default.ImportPath = obrazPath;
        }

        public bool UseIgnore = true;

        private string PathProjectResources;

        private string[] Selected;

        public void Initialize_Browser()
        {
            MainWindow.ProgramData.LoadDataBrowser(PathProjectResources, browser, null, this,UseIgnore);
        }

        public void Initialize_Browser_Import()
        {
            LoadDataBrowser(Properties.Settings.Default.ImportPath, browser_import, null, this,UseIgnore);
        }

        public void LoadDataBrowser(string path, TreeView treebrowser, List<FileInfo> patternFile = null, Window winTitle = null, bool UseIgonore = true)
        {
            if (!Properties.Settings.Default.UseIgnorePather)
                UseIgonore = false;

            if (winTitle == null && MainWindow.ProgramData.MainWinThread != null)
            {
                winTitle = MainWindow.ProgramData.MainWinThread;
            }

            string orig = null;
            if (winTitle != null)
            {
                orig = winTitle.Title;
                winTitle.Title += " [ Инициализация обозревателя ]";
            }
            treebrowser.Items.Clear();
            TreeViewItem fItem = new TreeViewItem();
            fItem.Tag = path;
            if (path == null)
                return;
            string txt = MainWindow.ProgramData.GetLastSplash(path.Trim('\\'));
            if (Properties.Settings.Default.browser_mode == 1 || Properties.Settings.Default.browser_mode == 2 || Properties.Settings.Default.browser_mode == 3)
            {
                txt = AddonClass.RussianLogic.GetRussianName(path);
            }
            fItem.Header = txt;
            if (Properties.Settings.Default.UseMarkers)
            {
                if (AddonClass.ColorMarker.GetMarkerByPathFile(path) != null)
                    fItem.Foreground = AddonClass.ColorMarker.GetMarkerByPathFile(path);
                if (Properties.Settings.Default.UseIgnorePather && UseIgonore)
                {
                    string[] ignoreName = Properties.Settings.Default.IgnorePather.Split(',');
                    if (ignoreName.Where(x => x == new DirectoryInfo(path).Name).Count() > 0)
                    {
                        fItem.Foreground = new SolidColorBrush(Colors.White);
                        fItem.Background = new SolidColorBrush(Colors.Red);
                    }
                }
            }



            AddRecurceFolders(path, fItem, UseIgonore, patternFile, winTitle, orig);




            treebrowser.Items.Add(fItem);
            if (orig != null)
                winTitle.Title = orig;
        }

        private void AddRecurceFolders(string path, TreeViewItem item, bool UseIgonore = true, List<FileInfo> patternFile = null, Window title = null, string origtitle = null)
        {
            DirectoryInfo f = new DirectoryInfo(path);
            if (title != null && origtitle != null)
                title.Title = origtitle + $" [ Инициализация обозревателя ({f.GetFiles().Length}:{f.GetDirectories().Length}) ]";

            if (Properties.Settings.Default.UseIgnorePather && UseIgonore)
            {
                if (Properties.Settings.Default.IgnorePather.Split(',').Where(x => x.ToUpper() == f.FullName.ToUpper()).Count() > 0)
                {
                    

                    TreeViewItem t = new TreeViewItem();
                    t.Tag = f.FullName;
                    t.Header = f.Name;
                    t.Foreground = new SolidColorBrush(Colors.White);
                    t.Background = new SolidColorBrush(Colors.Red);
                    item.Items.Add(t);
                    return;
                }
            }

            foreach (var v in f.GetFiles())
            {
                CheckBox fS = new CheckBox();
                fS.Checked += (sender, e) =>
                {
                    if (browser_import.Items.Count > 0)
                    {
                        int co = GetCountSelect((browser_import.Items[0] as TreeViewItem));
                        txt_counter.Text = "Кол-во файлов: " + co;
                    }
                };
                fS.Unchecked += (sender, e) =>
                {
                    if (browser_import.Items.Count > 0)
                    {
                        int co = GetCountSelect((browser_import.Items[0] as TreeViewItem));
                        txt_counter.Text = "Кол-во файлов: " + co;
                    }
                };
                fS.Foreground = new SolidColorBrush(Colors.White);

                fS.Tag = v.FullName;
                if(Selected != null)
                  foreach(var vse in Selected)
                {
                    string re = GetPathToGamedata(v.FullName).Trim();
                    if (re.ToUpper() == vse.ToUpper() || re.ToUpper().Contains(vse.ToUpper()))
                    {
                        item.IsExpanded = true;
                        fS.IsChecked = true;
                    }
                }

                string txt = v.Name;
                if (Properties.Settings.Default.browser_mode == 1)
                {

                    txt = AddonClass.RussianLogic.GetRussianName(v.FullName);
                }
                if (Properties.Settings.Default.browser_mode == 2)
                {

                    txt = AddonClass.RussianLogic.GetFirstSection(v.FullName);
                }
                if (Properties.Settings.Default.browser_mode == 3)
                {
                    txt = AddonClass.RussianLogic.GetInvNameFirstSection(v.FullName);
                }
                fS.Content = txt.Replace("_","-");
                if (Properties.Settings.Default.UseMarkers)
                    if (AddonClass.ColorMarker.GetMarkerByPathFile(v.FullName) != null)
                        fS.Foreground = AddonClass.ColorMarker.GetMarkerByPathFile(v.FullName);
                if (patternFile == null)
                {
                    item.Items.Add(fS);
                }
                else if (patternFile.Where(x => x.FullName == v.FullName).Count() > 0)
                {
                    item.Items.Add(fS);
                }
            }
            foreach (var v in f.GetDirectories())
            {
                if (Properties.Settings.Default.UseIgnorePather && UseIgonore)
                {
                    if (Properties.Settings.Default.IgnorePather.Split(',').Where(x => x.ToUpper() == v.FullName.ToUpper()).Count() > 0)
                    {
                        TreeViewItem t = new TreeViewItem();
                        t.Tag = v.FullName;
                        t.Header = v.Name;
                        t.Foreground = new SolidColorBrush(Colors.White);
                        t.Background = new SolidColorBrush(Colors.Red);
                        item.Items.Add(t);
                        continue;
                    }
                }

                TreeViewItem nE = new TreeViewItem();
                nE.Tag = v.FullName;
                string txt = v.Name;
                if (Properties.Settings.Default.browser_mode == 1 || Properties.Settings.Default.browser_mode == 2 || Properties.Settings.Default.browser_mode == 3)
                {
                    txt = AddonClass.RussianLogic.GetRussianName(v.FullName);
                }
                nE.Header = txt;
                if (Properties.Settings.Default.UseMarkers)
                    if (AddonClass.ColorMarker.GetMarkerByPathFile(v.FullName) != null)
                        nE.Foreground = AddonClass.ColorMarker.GetMarkerByPathFile(v.FullName);


                item.Items.Add(nE);
                AddRecurceFolders(v.FullName, nE, UseIgonore, null, title, origtitle);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(PathProjectResources) || !Directory.Exists(Properties.Settings.Default.ImportPath))
            {
                MessageBox.Show("Внимание выберите путь для проекта и путь для импорта в Настройки->Основные->Импорт", "Ошибка каталогов", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }
            Initialize_Browser();
            Initialize_Browser_Import();

            int c = GetCountSelect(browser_import.Items[0] as TreeViewItem);
            txt_counter.Text = "Кол-во файлов: " + c;

            this.Title = "Импорт файлов";
            if (browser.Items.Count <= 0)
            {
                MessageBox.Show("Нету каталога проекта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            else
                (browser.Items[0] as TreeViewItem).IsExpanded = true;

            if (browser_import.Items.Count <= 0)
            {
                MessageBox.Show("Нету каталога импорта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            else
                (browser_import.Items[0] as TreeViewItem).IsExpanded = true;

        }

        private void MN_CLEAR_Click(object sender, RoutedEventArgs e)
        {
            if(browser_import.SelectedItem is TreeViewItem)
            {
             //   MessageBox.Show((browser_import.SelectedItem as TreeViewItem).Header.ToString());
                SetRegistrValueRec(browser_import.SelectedItem as TreeViewItem, false);
            }
            else
                SetRegistrValueRec(browser_import.Items[0] as TreeViewItem,false);
        }

        private void SetValueRecursive(TreeViewItem it,bool val)
        {
            foreach(var el in it.Items)
            {
                if (el is CheckBox)
                {
                    CheckBox _el = (el as CheckBox);
                    _el.IsChecked = val;
                }
                else if (el is TreeViewItem)
                    SetValueRecursive(el as TreeViewItem, val);
            }

        }

        private int GetCountSelect(TreeViewItem elem)
        {
            int counter = 0;
            foreach(var v in elem.Items)
            {
                if (v is CheckBox)
                {
                    CheckBox box = (v as CheckBox);
                    if ((bool)box.IsChecked)
                        counter++;
                }
                else
                    counter += GetCountSelect(v as TreeViewItem);
            }
            return counter;
        }

        private void SetRegistrValueRec(TreeViewItem it,bool val)
        {
            //here reg elements
            SetValueRecursive(it, val);

            int co = GetCountSelect((browser_import.Items[0] as TreeViewItem));
            txt_counter.Text = "Кол-во файлов: " + co;
        }

        private void SetOnlyCatalogs(TreeViewItem item,bool value)
        {
            foreach (var el in item.Items)
            {
                 if (el is TreeViewItem)
                    SetValueRecursive(el as TreeViewItem, value);
            }

            int co = GetCountSelect((browser_import.Items[0] as TreeViewItem));
            txt_counter.Text = "Кол-во файлов: " + co;
        }

        private void SetOnlyFiles(TreeViewItem item, bool value)
        {
            foreach (var el in item.Items)
            {
                if (el is CheckBox)
                {
                    CheckBox _el = (el as CheckBox);
                    _el.IsChecked = value;
                }
            }

            int co = GetCountSelect((browser_import.Items[0] as TreeViewItem));
            txt_counter.Text = "Кол-во файлов: " + co;
        }

        private FileInfo[] GetFilesToCopy(TreeViewItem item)
        {
            List<FileInfo> vs = new List<FileInfo>();

            foreach(var v in item.Items)
            {
                if (v is CheckBox)
                {
                    CheckBox vCheck = (v as CheckBox);
                    if ((bool)vCheck.IsChecked)
                        vs.Add(new FileInfo(vCheck.Tag.ToString()));
                }
                else if (v is TreeViewItem)
                   vs.AddRange(GetFilesToCopy(v as TreeViewItem));
            }

            return vs.ToArray();
        }

        private void MN_ADD_Click(object sender, RoutedEventArgs e)
        {
            if(browser_import.SelectedItem is TreeViewItem)
            {
               // MessageBox.Show((browser_import.SelectedItem as TreeViewItem).Header.ToString());
                SetRegistrValueRec(browser_import.SelectedItem as TreeViewItem, true);
            }
            else
                SetRegistrValueRec(browser_import.Items[0] as TreeViewItem, true);
        }


        private string GetPathToGamedata(string pathFile)
        {
            FileInfo vs = new FileInfo(pathFile);
            string[] wrk = vs.FullName.Split('\\');
            string vsResult = "";
            for(int i = 0; i < wrk.Length; i++)
            {
                if (wrk[i].ToUpper() == "config".ToUpper() || wrk[i].ToUpper() == "configs".ToUpper() || wrk[i].ToUpper() == "textures".ToUpper() || wrk[i].ToUpper() == "meshes".ToUpper() || wrk[i].ToUpper() == "sounds".ToUpper() || wrk[i].ToUpper() == "ai".ToUpper() || wrk[i].ToUpper() == "levels".ToUpper() || wrk[i].ToUpper() == "anims".ToUpper() || wrk[i].ToUpper() == "scripts".ToUpper() || wrk[i].ToUpper() == "shaders".ToUpper() || wrk[i].ToUpper() == "spawns".ToUpper())
                {
                    break;
                }
                else
                    vsResult += wrk[i] + "\\";
            }
            return vs.FullName.Replace(vsResult,"");
        }

        private string GetPathByToGamedata(string path)
        {
            FileInfo vs = new FileInfo(path);
            string[] wrk = vs.FullName.Split('\\');
            string vsResult = "";
            for (int i = 0; i < wrk.Length; i++)
            {
                if (wrk[i].ToUpper() == "config".ToUpper() || wrk[i].ToUpper() == "configs".ToUpper() || wrk[i].ToUpper() == "textures".ToUpper() || wrk[i].ToUpper() == "meshes".ToUpper() || wrk[i].ToUpper() == "sounds".ToUpper() || wrk[i].ToUpper() == "ai".ToUpper() || wrk[i].ToUpper() == "levels".ToUpper() || wrk[i].ToUpper() == "anims".ToUpper() || wrk[i].ToUpper() == "scripts".ToUpper() || wrk[i].ToUpper() == "shaders".ToUpper() || wrk[i].ToUpper() == "spawns".ToUpper())
                {
                    break;
                }
                else
                    vsResult += wrk[i] + "\\";
            }
            return vsResult;
        }

        private async void btn_import_invoke_Click(object sender, RoutedEventArgs e)
        {

            List<FileInfo> files = GetFilesToCopy(browser_import.Items[0] as TreeViewItem).ToList();
            string dirOutput = GetPathByToGamedata((browser.Items[0] as TreeViewItem).Tag.ToString());
            progress.Value = 0;
            progress.Maximum = files.Count;
            bool copyRelease = (bool)check_copyExists.IsChecked;
            bool _break = false;
            foreach(var fil in files)
            {
                if (_break)
                    break;

                txt_counter.Text = GetPathToGamedata(fil.FullName);
                await Task.Run(() =>
                {
                    try
                    {
                        // System.Threading.Thread.Sleep(10);
                       // Console.WriteLine(dirOutput.TrimEnd('\\') + "\\" + GetPathToGamedata(fil.FullName));
                        string newFile = dirOutput.TrimEnd('\\') + "\\" + GetPathToGamedata(fil.FullName);

                        if(GetPathToGamedata(fil.FullName) == fil.FullName)
                        {
                            newFile = dirOutput.TrimEnd('\\') +"\\" + fil.Name;
                        }
                        string dir = newFile.Replace(MainWindow.ProgramData.GetLastSplash(newFile), "");
                        if (!File.Exists(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        if (copyRelease)
                        {
                            if (File.Exists(newFile))
                                File.Delete(newFile);
                            File.Copy(fil.FullName, newFile);
                        }
                        else
                        {
                            if (!File.Exists(newFile))
                                File.Copy(fil.FullName, newFile);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка структуры каталогов, дан не верный входной каталог!", "Error folder structures", MessageBoxButton.OK, MessageBoxImage.Error);
                        _break = true;
                    }

                });

                progress.Value++;
            }

            if (_break)
            {
                progress.Value = progress.Maximum;
            }
            else
                MessageBox.Show("Импорт успешно завершён!", "Импорт завершён", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MN_DOPCATALOGS_Click(object sender, RoutedEventArgs e)
        {
            if (browser_import.SelectedItem is TreeViewItem)
            {
                //   MessageBox.Show((browser_import.SelectedItem as TreeViewItem).Header.ToString());
                SetOnlyCatalogs(browser_import.SelectedItem as TreeViewItem, true);
            }
            else
                SetOnlyCatalogs(browser_import.Items[0] as TreeViewItem, true);
        }

        private void MN_ONLYFILES_Click(object sender, RoutedEventArgs e)
        {
            if (browser_import.SelectedItem is TreeViewItem)
            {
                //   MessageBox.Show((browser_import.SelectedItem as TreeViewItem).Header.ToString());
                SetOnlyFiles(browser_import.SelectedItem as TreeViewItem, true);
            }
            else
                SetOnlyFiles(browser_import.Items[0] as TreeViewItem, true);
        }
    }
}
