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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using UI.SyntaxBox;
using System.Diagnostics;
using System.Windows.Interop;
using Stalker_Studio.StalkerClass;

namespace Stalker_Studio
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InterfaceHelper.InitializeWindow(this);
            InterfaceHelper.InitializeWindow(this);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t\t\tАвтор SDK: Кирилл Сухинин\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("LtxLanguage V.1.1.1.0\nLtx Libray V.1.0.0.0\nXml string parser V.1.0.0.0 (static loader | block index | pattern logic)\nFTextures Libray V.1.0.0.0\nStalkerHierarchyElement Libray V.1.0.0.0");
            Console.WriteLine("UI.SyntaxBox (FOR BETA TEST) - Ltx Text");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Network protocol: None V.0.0.0.0");
            Console.ForegroundColor = ConsoleColor.White;

            ProgramData.MainWinThread = this;

            if (Properties.Settings.Default.FirstStartUp)
            {
                ByWin b = new ByWin();
                b.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                b.ShowDialog();
            }

            ProgramData.PasteLogic += PasteLogic_Script;
            HideChildrenMenu();
            SetSelectGamedata();

            Ini_Encoding();
            SetEncoding();
            InitializaterLtxParametrInWin();
            Hot_Key(MenuMain);
            OnStartUpArgs();
        }

        Dictionary<string, string> ltx_parametr = new Dictionary<string, string>();

        private void OnStartUpArgs()
        {
            if (App.Args.Length > 0)
            {
                if (Directory.Exists(App.Args[0]))
                {
                    ProgramData.Gamedata = App.Args[0];
                    SelectGamedata.Visibility = Visibility.Hidden;
                    browser.Visibility = Visibility.Visible;
                    ProgramData.LoadDataBrowser(ProgramData.Gamedata, treeBrowser);
                    if (ProgramData.xmlStrings.Count <= 0 && Properties.Settings.Default.InitXmlStat)
                    {
                        ProgramData.GetValueByLinkText("ammo", true);
                    }
                }
                else if (File.Exists(App.Args[0]))
                {
                    txtSelectGamedata.Text = App.Args[0];
                    SelectGamedata.Visibility = Visibility.Hidden;
                    browser.Visibility = Visibility.Visible;
                    OpenFile(App.Args[0]);
                }
                return;
            }
            else
                return;
        }

        private void PasteLogic_Script(string InWord,string Ower)
        {
            if(grid_script.Visibility == Visibility.Visible)
            {
                //here do
                //txt_script.TextChanged -= text_TextChanged;
                int cur = txt_script.CaretIndex;

                txt_script.Text = txt_script.Text.Remove(cur - Ower.Length, Ower.Length);


                txt_script.Text = txt_script.Text.Insert(cur - Ower.Length, InWord);
                txt_script.CaretIndex = (cur - Ower.Length) + InWord.Length;

                if (ProgramData.HintsWin != null)
                {
                    ProgramData.HintsWin.Close();
                    ProgramData.HintsWin = null;
                }

              //  text.TextChanged += text_TextChanged;
            }
        }

        /// <summary>
        /// Создания интерфейса для параметров
        /// </summary>
        private void InitializaterLtxParametrInWin()
        {
            //Основные параметры
            {

                if (false)
                {
                    TreeViewItem itGroup = new TreeViewItem();
                    itGroup.Header = "Основные параметры";



                    ltx_parametr.Add("Цена -", "cost");
                    ltx_parametr.Add("Размер Магазина -", "ammo_mag_size");
                    ltx_parametr.Add("Вес -", "inv_weight");

                    ltx_parametr.Add("Калибр -", "ammo_class");
                    ltx_parametr.Add("Импуль -", "hit_impulse");
                    ltx_parametr.Add("Дистанция -", "fire_distance");
                    ltx_parametr.Add("Скорость пули -", "bullet_speed");
                    ltx_parametr.Add("Скорострельность", "rpm");






                    foreach (var k in ltx_parametr.Keys)
                    {
                        AddToGroupElements(itGroup, 240, new string[] { k }, new FrameworkElement[] { new TextBox() { Name = $"prm_{ltx_parametr[k]}", Tag = ltx_parametr[k], Width = 100 } });
                    }


                    TreeViewItem hud = new TreeViewItem();
                    hud.Header = "Звуки";
                    ltx_parametr.Add("Выстрел -", "snd_shoot");
                    ltx_parametr.Add("Перезарядка -", "snd_reload");
                    ltx_parametr.Add("Пустой -", "snd_empty");


                    foreach (var k in ltx_parametr.Keys.Where(x => x == "Выстрел -" || x == "Перезарядка -" || x == "Пустой -"))
                    {

                        AddToGroupElements(hud, 210, new string[] { k }, new FrameworkElement[] { new TextBox() { Name = $"prm_{ltx_parametr[k]}", Tag = ltx_parametr[k], Width = 120 } });
                    }
                }
                string pathPrm = $"{System.Windows.Forms.Application.StartupPath}\\GroupsParametr.ltx";
                if (File.Exists(pathPrm))
                {
                    StalkerClass.HierarchyLtx.LtxFile _prm = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                    foreach(var v in _prm.Sections)
                    {
                        string nameGroup = v.Name_Section;

                        TreeViewItem objI = new TreeViewItem();
                        objI.Header = nameGroup;
                        List<string> prms = new List<string>();
                        foreach(var vPrm in v.Parametrs)
                        {
                            int Ispace = int.Parse(v.Heir_Section.Trim().Split(';')[1]);
                            int ISpace2 = int.Parse(v.Heir_Section.Trim().Split(';')[0]);

                            ltx_parametr.Add(vPrm.Name_Parametr.Trim(), vPrm.Value_Parametr.Trim());
                            if(vPrm.Desription_Parametr != null && vPrm.Desription_Parametr.Split(':').Length > 1)
                            {
                                Ispace = int.Parse(vPrm.Desription_Parametr.Trim().Split(':')[1]);
                                ISpace2 = int.Parse(vPrm.Desription_Parametr.Trim().Split(':')[0]);
                            }
                            AddToGroupElements(objI, Ispace, new string[] { vPrm.Value_Parametr.Trim() }, new FrameworkElement[] { new TextBox() { Name = $"prm_{vPrm.Name_Parametr.Trim()}",Tag = vPrm.Name_Parametr.Trim(),Width = ISpace2 } });
                        }
                    }
                }

                foreach(var k in ltx_parametr.Keys)
                {
                    //((Label)(GetElementByName($"lab_prm_{k}"))).MouseRightButtonUp += (sender, e) =>
                    //{
                    //    ((TextBox)(GetElementByName($"prm_{k}"))).Focus();
                    //};

                    //((Label)(GetElementByName($"lab_prm_{k}"))).MouseDoubleClick += (sender, e) =>
                    //{
                    //    StalkerClass.HierarchyLtx.LtxFile file = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
                    //    int indexCursor = txt_ltx_file.CaretOffset;
                    //    string section = "";
                    //    if (indexCursor >= 0)
                    //    {
                    //        foreach (var v in file.Sections)
                    //        {
                    //            int _index = txt_ltx_file.Text.IndexOf("[" + v.Name_Section + "]");
                    //            if (indexCursor >= _index)
                    //                section = v.Name_Section;
                    //        }

                    //    }

                    //    if (NowSection != null)
                    //        section = NowSection;
                    //    else
                    //        section = file.Sections[0].Name_Section;
                    //    section = section.Split(':')[0];

                    //    try
                    //    {
                    //        int a_offset = txt_ltx_file.Text.IndexOf(k);
                    //        int b_offset = k.Length;
                    //        int line_ = -1;
                    //        for (int i = 0; i < txt_ltx_file.Text.Split('\n').Length; i++)
                    //        {
                    //            if (txt_ltx_file.Text.Split('\n')[i].StartsWith(k))
                    //                line_ = i - 1;
                    //        }
                    //        txt_ltx_file.Focus();
                    //        txt_ltx_file.Select(a_offset, b_offset);
                    //        if (line_ != -1)
                    //            txt_ltx_file.ScrollToLine(line_);

                    //    }
                    //    catch
                    //    {

                    //    }
                    //};

                    //((TextBox)(GetElementByName($"prm_{k}"))).MouseDoubleClick += (sender, e) =>
                    //{
                    //    ((Label)(GetElementByName($"lab_prm_{k}"))).Focus();
                    //};

                    //((TextBox)(GetElementByName($"prm_{k}"))).KeyUp += (sender, e) =>
                    //{
                    //    if (e.Key == Key.Enter)
                    //    {
                    //        if (!string.IsNullOrWhiteSpace(((TextBox)(GetElementByName($"prm_{k}"))).Text))
                    //        {
                    //            int indexCursor = txt_ltx_file.CaretOffset;
                    //            string section = "";
                    //            StalkerClass.HierarchyLtx.LtxFile file = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
                    //            if (indexCursor >= 0)
                    //            {
                                    
                                    
                    //                foreach (var v in file.Sections)
                    //                {
                    //                    int _index = txt_ltx_file.Text.IndexOf("[" + v.Name_Section + "]");
                    //                    if (indexCursor >= _index)
                    //                        section = v.Name_Section;
                    //                }

                    //            }

                    //            if (NowSection != null)
                    //                section = NowSection;
                    //            else
                    //                section = file.Sections[0].Name_Section;

                    //            section = section.Split(':')[0];

                    //            if (file.Sections.Where(x => x.Name_Section == section).Count() > 0 && file.Sections.Where(x => x.Name_Section == section).First().Parametrs.Where(x => x.Name_Parametr == k).Count() > 0)
                    //            {
                    //                file.Sections.Where(x => x.Name_Section == section).First().Parametrs.Where(x => x.Name_Parametr == k).First().Value_Parametr = ((TextBox)(GetElementByName($"prm_{k}"))).Text;
                    //                txt_ltx_file.Text = file.ToString();

                    //               // if (Properties.Settings.Default.AutoSave)
                    //                {
                    //                    try
                    //                    {
                    //                        object[] result = CheckOnLtxGood(txt_ltx_file.Text);
                    //                        if (!(bool)(result[0]))
                    //                            throw new Exception($"Ошибка тэга! Line: {result[1]}");
                    //                        string nameFile_ = OpenFileInputNow;
                    //                        File.WriteAllText(nameFile_, txt_ltx_file.Text, ProgramData.Encoding_LTX);

                    //                    }
                    //                    catch (Exception g)
                    //                    {
                    //                        //  MessageBox.Show($"Не удалось сохранить файл!\n[{g.Message}]", "Сохранение файла", MessageBoxButton.OK, MessageBoxImage.Error);
                    //                    }
                    //                }
                    //            }
                    //            else
                    //            {
                    //                MessageBox.Show($"Ошибка параметров!\nОтсутствует секция или параметр!\nСекция: \"{section}\"\nПараметр: \"{k}\"\nИзмените настройки -> Параметры", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //            }
                    //        }
                    //    }
                    //    HotKey_Function(sender, e);
                    //};

                }

                //  AddToGroupElements(itGroup, 150, new string[] { "Цена-" }, new FrameworkElement[] { new TextBox() { Name = "prm_cost", Width = 50 } });
            }
        }

        private string InputNowParametr = null;


        private void Hot_Key(Grid parent)
        {
            //if(false)
            //{
            //    foreach (FrameworkElement vs in prm_ltx_default.Children)
            //    {
            //        vs.KeyUp += (sender, e) =>
            //        {
                        
            //            if (e.Key == Key.Down)
            //            {
            //                if ((((TreeViewItem)(treeBrowser.SelectedItem)).Parent) != null){


            //                    for(int i = 0; i < ((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Count; i++)
            //                    {
            //                        if (((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items[i] == (TreeViewItem)(treeBrowser.SelectedItem))
            //                        {
            //                            if(((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Count < ((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Count + 1)
            //                            {
            //                                try
            //                                {
            //                                    ((TreeViewItem)(((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items[i + 1])).IsSelected = true;
            //                                    OpenFile(((TreeViewItem)(((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items[i + 1])).Tag.ToString(), false);
                                                
                                               
            //                                    break;
            //                                }
            //                                catch
            //                                {

            //                                }
            //                            }
            //                        }
            //                    }

            //                }
            //            }
            //            else if(e.Key == Key.Up)
            //            {
            //                if ((((TreeViewItem)(treeBrowser.SelectedItem)).Parent) != null)
            //                {


            //                    for (int i = 0; i < ((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Count; i++)
            //                    {
            //                        if (((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items[i] == (TreeViewItem)(treeBrowser.SelectedItem))
            //                        {
            //                            if (((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Count > ((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Count - 1)
            //                            {
            //                                try
            //                                {
            //                                    ((TreeViewItem)(((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items[i - 1])).IsSelected = true;
            //                                    OpenFile(((TreeViewItem)(((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items[i - 1])).Tag.ToString(), false);
            //                                    break;
            //                                }
            //                                catch
            //                                {

            //                                }
            //                            }
            //                        }
            //                    }

            //                }
            //            }
            //        };
            //    }
            //}

            foreach(object v in parent.Children) 
            {
                if(v is Grid)
                {
                    Hot_Key((Grid)v);
                }
                else if(v is TextBox)
                {
                    ((FrameworkElement)(v)).KeyUp += HotKey_Function;
                }
            }
            //recursive method
        }

        private void HotKey_Function(object sender,KeyEventArgs e)
        {
            if (e.Key == Key.Left && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                if (treeBrowser.Visibility == Visibility.Visible)
                {
                    if (((TextBox)(sender)).Tag != null)
                        InputNowParametr = ((TextBox)(sender)).Tag.ToString();
                    treeBrowser.Focus();
                }
            }
            if(e.Key == Key.F1 && !Properties.Settings.Default.Mode_ltx_form)
            {
                OpenFile(OpenFileInputNow, false);
            }
            if(e.Key == Key.F2 && !Properties.Settings.Default.Mode_ltx_form)
            {
                Properties.Settings.Default.Replace_Link = !Properties.Settings.Default.Replace_Link;
                Properties.Settings.Default.Save();
                OpenFile(OpenFileInputNow, false);
            }
            if(e.Key == Key.F3 && !Properties.Settings.Default.Mode_ltx_form)
            {
                Properties.Settings.Default.Ltx_all_param = !Properties.Settings.Default.Ltx_all_param;
                Properties.Settings.Default.Save();
                OpenFile(OpenFileInputNow, false);
            }
            if(e.Key == Key.F4)
            {
                Properties.Settings.Default.Mode_ltx_form = !Properties.Settings.Default.Mode_ltx_form;
                Properties.Settings.Default.Save();
                OpenFile(OpenFileInputNow, false);
            }
            if((sender as TextBox).Name.ToString() == "txt_ltx_file" && e.Key == Key.F5)
            {
                Properties.Settings.Default.RemoveColorLtx = !Properties.Settings.Default.RemoveColorLtx;
                Properties.Settings.Default.Save();
            }
        }

        private void SetEncoding()
        {
            ProgramData.InitializeComponentEncoding();
        }

        private void Ini_Encoding()
        {

            comboEncoding.SelectedIndex = Properties.Settings.Default.Encoding_LTX;
        }

        public StalkerClass.WordDescription Words = new StalkerClass.WordDescription();

        private void ClearLtxModeElement()
        {
            List<FrameworkElement> del = new List<FrameworkElement>();
            foreach(FrameworkElement vEle in ltx_mode_elemetns.Children)
            {
                if (vEle.Name == "lab_section" || vEle.Name == "combo_section")
                {
                    continue;
                }
                else
                    del.Add(vEle);
            }
            foreach(FrameworkElement delel in del)
            {
                ltx_mode_elemetns.Children.Remove(delel);
            }
        }


        private void SaveModeparametrNoEvent(TextBox sender)
        {

            string namePar = sender.Tag.ToString();

            string vlaue = sender.Text;
            StalkerClass.HierarchyLtx.LtxFile f = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(OpenFileInputNow), ProgramData.Encoding_LTX);
            if (f.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == namePar).Count() >= 1)
            {
                if (!sender.Tag.ToString().StartsWith("LINK/"))
                {
                    f.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == namePar).First().Value_Parametr = vlaue;
                    txt_ltx_file.Text = f.ToString();
                }
                else if (sender.Tag.ToString().StartsWith("LINK/"))
                {
                    string orLink = sender.Tag.ToString().Split('/')[1];
                    StalkerClass.Xml.Xml_Text_File fls = ProgramData.GetFileByIdText(orLink);
                    fls.ExpressionsBlocks.Where(x => x.Id == orLink).First().Text = vlaue;
                    Console.WriteLine(orLink);
                    File.WriteAllText(fls._File.FullName, fls.ToString(), ProgramData.Encoding_XML);
                }

                File.WriteAllText(OpenFileInputNow, f.ToString(), ProgramData.Encoding_LTX);
            }
            else if (sender.Tag.ToString().StartsWith("LINK/"))
            {
                string orLink = sender.Tag.ToString().Split('/')[1];
                StalkerClass.Xml.Xml_Text_File fls = ProgramData.GetFileByIdText(orLink);
                fls.ExpressionsBlocks.Where(x => x.Id == orLink).First().Text = vlaue;
                Console.WriteLine(orLink);
                File.WriteAllText(fls._File.FullName, fls.ToString(), ProgramData.Encoding_XML);
            }
        }

        private void SaveModeparametr(TextBox sender)
        {
            sender.KeyUp += (_sender, e) =>
            {
                if (combo_section.SelectedIndex != -1)
                {
                    if (e.Key == Key.Enter)
                    {
                        string namePar = sender.Tag.ToString();
                        string vlaue = sender.Text;
                        StalkerClass.HierarchyLtx.LtxFile f = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(OpenFileInputNow), ProgramData.Encoding_LTX);
                        if (f.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == namePar).Count() >= 1)
                        {
                            if (!sender.Tag.ToString().StartsWith("LINK/"))
                            {
                                f.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == namePar).First().Value_Parametr = vlaue;
                                txt_ltx_file.Text = f.ToString();
                            }

                            File.WriteAllText(OpenFileInputNow, f.ToString(), ProgramData.Encoding_LTX);
                        }
                        else if(sender.Tag.ToString().StartsWith("LINK/"))
                        {
                            string orLink = sender.Tag.ToString().Split('/')[1];
                            StalkerClass.Xml.Xml_Text_File fls = ProgramData.GetFileByIdText(orLink);
                            fls.ExpressionsBlocks.Where(x => x.Id == orLink).First().Text = vlaue;
                            Console.WriteLine(orLink);
                            File.WriteAllText(fls._File.FullName, fls.ToString(), ProgramData.Encoding_XML);
                        }
                    }
                }
            };
        }

        private void AddLtxModeElement(Dictionary<string,string> vKeys,StalkerClass.HierarchyLtx.LtxFile ltx,int section = 0)
        {
            //key - name 
            //value - parametr
            int spacer = 30;

            if (ltx.Sections.Count <= 0)
                return;
            string origTitle = this.Title;
            if (!Properties.Settings.Default.Ltx_all_param)
            {

                foreach (var vK in vKeys.Keys)
                {
                    bool add = false;

                    foreach (var vEl in ltx.Sections[section].Parametrs)
                    {
                        if (vEl.Name_Parametr == vK)
                        {
                            add = true;
                            break;
                        }
                    }


                    if (add)
                    {
                        if (!ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().IsValue)
                            continue;

                        Label lab = new Label();
                        if (Properties.Settings.Default.UseTranslatePrm)
                        {
                            lab.Content = vKeys[vK];
                            foreach (var vss in Words.AllWords)
                            {
                                foreach(var vEl in vss.Value.Datas)
                                {
                                    if (vK.ToUpper() == vEl.NameParametr.ToUpper())
                                    {
                                        lab.Content = vEl.DataParametr;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                            lab.Content = vKeys[vK];
                        lab.Tag = "LTMOD_" + vK;
                        Grid.SetColumn(lab, 0);
                        TextBox TXT = new TextBox();
                        TXT.Height = 25;
                        Grid.SetColumn(TXT, 1);
                        TXT.VerticalAlignment = VerticalAlignment.Top;
                        lab.VerticalAlignment = VerticalAlignment.Top;
                        TXT.Tag = vK;
                        TXT.Text = ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Value_Parametr;
                        if (Properties.Settings.Default.Replace_Link && (ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Value_Parametr.ToUpper() == "description".ToUpper() || ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Value_Parametr.ToUpper().StartsWith("inv_name".ToUpper())))
                        {

                            this.Title = "Stalker Studio (Дозагрузка xml файлов)";
                            string val = ProgramData.GetValueByLinkText(ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Value_Parametr);
                            if (!string.IsNullOrWhiteSpace(val))
                            {

                                TXT.Text = val;
                                TXT.Tag = $"LINK/{ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Value_Parametr}/{ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Name_Parametr}";
                            }
                            else
                            {
                                TXT.Text = ltx.Sections[section].Parametrs.Where(x => x.Name_Parametr == vK).First().Value_Parametr;
                            }

                        }
                        TXT.Margin = new Thickness(0, spacer, 0, spacer);
                        lab.Margin = new Thickness(0, spacer, 0, spacer);
                        ltx_mode_elemetns.Children.Add(lab);
                        ltx_mode_elemetns.Children.Add(TXT);
                        spacer += 30;
                        TXT.KeyUp += HotKey_Function;

                        TXT.ContextMenu = new ContextMenu();

                        MenuItem copyItem = new MenuItem();
                        copyItem.Tag = vK;
                        copyItem.Header = "Копировать";
                        copyItem.Click += (sender, e) =>
                        {
                            Clipboard.SetText(TXT.Text);
                        };


                        MenuItem t = new MenuItem();
                        t.Tag = vK;
                        
                        t.Header = "Вставить значение";
                        t.Click += ltx_vstavka_Click;
                        TXT.ContextMenu.Items.Add(copyItem);
                        TXT.ContextMenu.Items.Add(t);

                        bool have = false;
                        string _InWord = vK;
                        foreach (var vI in Words.AllWords)
                        {
                            foreach (var vS in vI.Value.Datas)
                            {
                                if (_InWord == vS.NameParametr)
                                {
                                    TXT.ToolTip = new ToolTip();
                                    ((ToolTip)(TXT.ToolTip)).Content = vS.DataParametr.Replace("\\n", "\n");
                                    lab.ToolTip = new ToolTip();
                                    ((ToolTip)(lab.ToolTip)).Content = vS.DataParametr.Replace("\\n", "\n");
                                    have = true;
                                }
                            }
                        }
                        if (!have)
                        {
                            TXT.ToolTip = null;
                            lab.ToolTip = null;
                        }
                        SaveModeparametr(TXT);
                    }
                }
            }
            else
            {
                foreach (var vEl in ltx.Sections[section].Parametrs)
                {
                    if (vEl.IsValue)
                    {

                        TextBlock lab = new TextBlock();
                        lab.Foreground = new SolidColorBrush(Colors.White);
                        if (Properties.Settings.Default.UseTranslatePrm)
                        {
                            lab.Text = vEl.Name_Parametr;
                            foreach (var vss in Words.AllWords)
                            {
                                foreach(var _vEl in vss.Value.Datas)
                                {
                                    if(_vEl.NameParametr.ToUpper() == vEl.Name_Parametr.ToUpper())
                                    {
                                        lab.Text = _vEl.DataParametr;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                            lab.Text = vEl.Name_Parametr;
                        lab.Tag = "LTMOD_" + vEl.Name_Parametr;
                        Grid.SetColumn(lab, 0);
                        TextBox TXT = new TextBox();
                        TXT.Height = 25;
                        Grid.SetColumn(TXT, 1);
                        TXT.VerticalAlignment = VerticalAlignment.Top;
                        lab.VerticalAlignment = VerticalAlignment.Top;
                        TXT.Tag = vEl.Name_Parametr;
                        TXT.Text = vEl.Value_Parametr;
                        if (Properties.Settings.Default.Replace_Link && (vEl.Name_Parametr.ToUpper() == "description".ToUpper() || vEl.Name_Parametr.ToUpper().StartsWith("inv_name".ToUpper())))
                        {
                            this.Title = "Stalker Studio (Дозагрузка xml файлов)";
                            string val = ProgramData.GetValueByLinkText(vEl.Value_Parametr);
                            if (!string.IsNullOrWhiteSpace(val))
                            {

                                TXT.Text = val;
                                TXT.Tag = $"LINK/{vEl.Value_Parametr}/{vEl.Name_Parametr}";
                            }
                            else
                            {
                                TXT.Text = vEl.Value_Parametr;
                            }
                            
                        }


                        TXT.Margin = new Thickness(0, spacer, 0, spacer);
                        lab.Margin = new Thickness(0, spacer, 0, spacer);
                        ltx_mode_elemetns.Children.Add(lab);
                        ltx_mode_elemetns.Children.Add(TXT);
                        spacer += 30;
                        TXT.KeyUp += HotKey_Function;

                        TXT.ContextMenu = new ContextMenu();

                        MenuItem copyItem = new MenuItem();
                        copyItem.Tag = vEl.Name_Parametr;
                        copyItem.Header = "Копировать";
                        copyItem.Click += (sender, e) =>
                        {
                            Clipboard.SetText(TXT.Text);
                        };

                        MenuItem t = new MenuItem();
                        t.Tag = vEl.Name_Parametr;
                        t.Header = "Вставить значение";
                        t.Click += ltx_vstavka_Click;
                        TXT.ContextMenu.Items.Add(copyItem);
                        TXT.ContextMenu.Items.Add(t);

                        bool have = false;
                        string _InWord = vEl.Name_Parametr;
                        foreach (var vI in Words.AllWords)
                        {
                            foreach (var vS in vI.Value.Datas)
                            {
                                if (_InWord == vS.NameParametr)
                                {
                                    TXT.ToolTip = new ToolTip();
                                    ((ToolTip)(TXT.ToolTip)).Content = vS.DataParametr.Replace("\\n", "\n");
                                    lab.ToolTip = new ToolTip();
                                    ((ToolTip)(lab.ToolTip)).Content = vS.DataParametr.Replace("\\n", "\n");
                                    have = true;
                                }
                            }
                        }
                        if (!have)
                        {
                            TXT.ToolTip = null;
                            lab.ToolTip = null;
                        }
                        SaveModeparametr(TXT);
                        
                    }
                    
                }

            }
            this.Title = origTitle;
        }

        

        public void AddToGroupElements(TreeViewItem group, int width_panel, string[] NameElements, FrameworkElement[] elements, FrameworkElement[] centerElement = null)
        {
            if (NameElements.Length != elements.Length)
                return;

            for (int el = 0; el < NameElements.Length; el++)
            {
                StackPanel panel = new StackPanel();
                panel.HorizontalAlignment = HorizontalAlignment.Right;
                panel.Width = width_panel;
                Label labName = new Label();
                labName.Content = NameElements[el];
                labName.Name = $"lab_{elements[el].Name}";

                
                int _a = 20;
                for (int j = 0; j < labName.Content.ToString().Length; j++)
                    _a += 4;
                elements[el].Margin = new Thickness(_a, -25, 0, 0);
                elements[el].HorizontalAlignment = HorizontalAlignment.Right;
                labName.Margin = new Thickness(-5, 0, 0, 0);
                panel.Children.Add(labName);

                if (centerElement != null && el <= centerElement.Length)
                {
                    int a = 20;
                    for (int j = 0; j < labName.Content.ToString().Length; j++)
                        a += 4;
                    centerElement[el].HorizontalAlignment = HorizontalAlignment.Center;
                    centerElement[el].Margin = new Thickness(a, -25, 0, 0);
                    panel.Children.Add(centerElement[el]);
                }


                panel.Children.Add(elements[el]);
                group.Items.Add(panel);

            }
        }

        public void AddElement(string mainGroup, string[] NameElement, string[] TagElemtns)
        {
            TreeViewItem it = new TreeViewItem();
            it.Header = mainGroup;

            for (int i = 0; i < NameElement.Length; i++)
            {
                StackPanel panel = new StackPanel();
                panel.Width = 300;
                Label lab = new Label();
                lab.Content = NameElement[i];
                lab.Tag = TagElemtns[i];
                TextBox txt = new TextBox();
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.Width = 100;
                txt.Height = 20;

                int a = 20;
                for (int j = 0; j < lab.Content.ToString().Length; j++)
                    a += 4;

                txt.Margin = new Thickness(a, -25, 0, 0);
                panel.Children.Add(lab);
                panel.Children.Add(txt);

                it.Items.Add(panel);
            }
        }

        public void HideChildrenMenu()
        {
            //dds
            parametr_dds.Visibility = Visibility.Hidden;
            //gamedata
           // browser.Visibility = Visibility.Hidden;
            SelectGamedata.Visibility = Visibility.Hidden;
            grid_ltx_file.Visibility = Visibility.Hidden;
            grid_ltx_mode.Visibility = Visibility.Hidden;
            grid_xml_string_text.Visibility = Visibility.Hidden;
            grid_script.Visibility = Visibility.Hidden;
            parametr_scripts.Visibility = Visibility.Hidden;
            parametr_ogf.Visibility = Visibility.Hidden;
            this.Title = "Stalker Studio";
            
        }

        private UIElement FindGridInChildren(Grid parentGrid,string name)
        {
            foreach(var v in parentGrid.Children)
            {
                if(v is Grid)
                {
                    if (((Grid)(v)).Name == name)
                        return ((Grid)(v));
                }
            }
            return null;
        }

        private List<UIElement> FindAllGridInChildren(Grid parent)
        {
            List<UIElement> vs = new List<UIElement>();

            foreach(var v in parent.Children)
            {
                if (v is Grid)
                {
                    vs.Add((Grid)(v));
                }
            }
            return vs;
        }

        /// <summary>
        /// Метод который устанавливает первые grid для выбора gamedata
        /// </summary>
        public void SetSelectGamedata()
        {
            HideChildrenMenu();
            SelectGamedata.Visibility = Visibility.Visible;
            browser.Visibility = Visibility.Hidden;
            ProgramData.SeeDialogLoadXml = false;
            ProgramData.xmlStrings.Clear();
            ProgramData.LoaderXml = false;
            ProgramData.PaternWinThread = null;
        }






        private void txtSelectGamedata_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (Directory.Exists(txtSelectGamedata.Text) && txtSelectGamedata.Text.ToUpper().Contains("gamedata".ToUpper()))
            //{
            //    ProgramData.Gamedata = txtSelectGamedata.Text;
            //    SelectGamedata.Visibility = Visibility.Hidden;
            //    browser.Visibility = Visibility.Visible;

            //    ProgramData.LoadDataBrowser(ProgramData.Gamedata,treeBrowser);
            //}
        }

        private void btnSelectGamedata_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            folder.Description = "Выберите путь к gamedata для работы";
            if (Directory.Exists(txtSelectGamedata.Text))
                folder.SelectedPath = txtSelectGamedata.Text;
            if(folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSelectGamedata.Text = folder.SelectedPath;

                GamedataManager.This.SetRootAtPath(folder.SelectedPath);
                ViewModel.Workspace.This.Browser.Root = GamedataManager.This.Root;
            }
        }

        private void tooltip_changeGamedata_Click(object sender, RoutedEventArgs e)
        {
            SetSelectGamedata();
        }

        public static System.Drawing.Bitmap FromSourceImage(BitmapImage img)
        {
            if (img == null)
                return null;
            MemoryStream stream = new MemoryStream();

            stream.Position = 0;

            stream = (MemoryStream)img.StreamSource;


            System.Drawing.Bitmap bit = new System.Drawing.Bitmap(new MemoryStream(stream.ToArray()),false);

            return bit;
        }

        public static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            BitmapImage bitmapimage = new BitmapImage();
            
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

            }
            bitmap.Dispose();
            return bitmapimage;
        }

        private string OpenFileInputNow = null;
        private string LastOpenFileInput = null;

        private void OpenFile(string pathToFile,bool iniCurrentbrowser = true)
        {

            if (!File.Exists(pathToFile))
                return;

            HideChildrenMenu();

            this.Title = "Visual Studio (Загрузка...)";

            bool wasOpen = false;

            string nameFile = ProgramData.GetLastSplash(pathToFile);
            
            if(nameFile.Split('.')[nameFile.Split('.').Length - 1].ToUpper() == "xml".ToUpper())
            {
                string xmlT = File.ReadAllText(pathToFile, ProgramData.Encoding_XML);
                object[] res = StalkerClass.Xml.Xml_Text_File.CheckStringTextXml(xmlT);
                bool value = (bool)res[0];
                if (value)
                {
                    wasOpen = true;
                    grid_xml_string_text.Visibility = Visibility.Visible;
                    string txtXml = File.ReadAllText(pathToFile, ProgramData.Encoding_XML);
                    txt_xml_strings.Text = txtXml;
                    txt_xml_strings.Tag = new StalkerClass.Xml.Xml_Text_File(txtXml, true);
                }
                else
                {
                    if (res[1] != null)
                        MessageBox.Show("Ошибка открытия файла!\n" + res[1].ToString(), "Не удалось открыть файл xml!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if(nameFile.Split('.')[nameFile.Split('.').Length - 1].ToUpper() == "script".ToUpper())
            {
                //TODO: Сделать listbox для функций.
                string textScript = File.ReadAllText(pathToFile, ProgramData.Encoding_Script);
                grid_script.Visibility = Visibility.Visible;
                txt_script.Text = textScript;
                parametr_scripts.Visibility = Visibility.Visible;

                StalkerClass.Scripts.IndexerScripts inscript = new StalkerClass.Scripts.IndexerScripts(textScript);

                scripts_list_func.Items.Clear();
                foreach(var v in inscript._Functions)
                {
                    scripts_list_func.Items.Add(v);
                }
                ProgramData.Hints.ClearDopHints();
                ProgramData.Hints.InitializerDopHints(textScript);
                wasOpen = true;
            }
            else
            {
                return;
            }
           if (wasOpen)
            {
               Initialize_Pre_Icon();

                if (iniCurrentbrowser && treeBrowser.Items.Count > 0)
                    setCurrentElementBrowser((TreeViewItem)treeBrowser.Items[0], pathToFile);
                string _ls = OpenFileInputNow;
                OpenFileInputNow = pathToFile;

                if (OpenFileInputNow != null && _ls != OpenFileInputNow)
                {
                    LastOpenFileInput = _ls;
                }
                string _nameFile = nameFile;
                if (_nameFile.Contains("."))
                    this.Title = $"Stalker Studio - {_nameFile}";
            }
            else
            {
                this.Title = "Visual Studio";
            }
        }

        private void OpenParent(TreeViewItem inputItem)
        {
            if(inputItem.Parent != null && inputItem.Parent is TreeViewItem)
            {
                ((TreeViewItem)(inputItem.Parent)).IsExpanded = true;
                OpenParent(((TreeViewItem)(inputItem.Parent)));
            }
        }

        private bool setCurrentElementBrowser(TreeViewItem workItem,string pathToElement)
        {
            foreach(TreeViewItem it in workItem.Items)
            {
                if (it.Tag.ToString() == pathToElement)
                {

                    it.IsSelected = true;
                    OpenParent(it);
                    return true;
                }
                else
                    setCurrentElementBrowser(it, pathToElement);
            }
            return false;
        }


        private void treeBrowser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(treeBrowser.SelectedItem != null)
            {
                TreeViewItem item = (TreeViewItem)(treeBrowser.SelectedItem);
                if(item.Tag != null)
                {
                    OpenFile(item.Tag.ToString());
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            file.Title = "Выберите файл для открытия";
            file.Filter = "*.*|*.*|Ltx File|*.ltx|dds file|*.dds";
            if(file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                HideChildrenMenu();
                browser.Visibility = Visibility.Visible;
                string dirLastFile = $"{file.FileName.Replace(ProgramData.GetLastSplash(file.FileName), "")}";
                List<string> oth = Properties.Settings.Default.LastOpenIndex.Split(';').ToList<string>();
                if (oth.Where(x => x.ToUpper().Replace(";", "") == file.FileName).Count() <= 0)
                {
                    Properties.Settings.Default.LastOpenIndex += file.FileName + ";";
                    Properties.Settings.Default.Save();
                }
                OpenFile(file.FileName);
            }
        }

        private void Initialize_Pre_Icon(int select = -1)
        {
            if (txt_ltx_file.Text.Contains("inv_grid") && FindIconDDS() != null)
            {
                StalkerClass.DDSImage dds = new StalkerClass.DDSImage(File.ReadAllBytes(FindIconDDS()));

                int x = -1;
                int y = -1;

                int w = -1;
                int h = -1;

                LtxLanguage.LtxData dat = new LtxLanguage.LtxData(txt_ltx_file.Text);

                if (select == -1)
                {
                    //TODO: переделать нахуй пидр
                    try
                    {
                        if (dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_x") != null)
                        {
                            x = int.Parse(dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_x").DataParametr);
                        }
                        if (dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_y") != null)
                        {
                            y = int.Parse(dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_y").DataParametr);
                        }
                        if (dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_width") != null)
                        {
                            w = int.Parse(dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_width").DataParametr);
                        }
                        if (dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_height") != null)
                        {
                            h = int.Parse(dat.LtxSectionDatas.Where(z => z.NameSection == NowSection).First().GetParametr("inv_grid_height").DataParametr);
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    if (dat.LtxSectionDatas[select].GetParametr("inv_grid_x") != null)
                    {
                        x = int.Parse(dat.LtxSectionDatas[select].GetParametr("inv_grid_x").DataParametr);
                    }
                    if (dat.LtxSectionDatas[select].GetParametr("inv_grid_y") != null)
                    {
                        y = int.Parse(dat.LtxSectionDatas[select].GetParametr("inv_grid_y").DataParametr);
                    }
                    if (dat.LtxSectionDatas[select].GetParametr("inv_grid_width") != null)
                    {
                        w = int.Parse(dat.LtxSectionDatas[select].GetParametr("inv_grid_width").DataParametr);
                    }
                    if (dat.LtxSectionDatas[select].GetParametr("inv_grid_height") != null)
                    {
                        h = int.Parse(dat.LtxSectionDatas[select].GetParametr("inv_grid_height").DataParametr);
                    }
                }

                if (x != -1 && y != -1)
                {
                    if(w == -1 || h == -1)
                    {
                        w = 1;
                        h = 1;
                    }

                    System.Drawing.Bitmap btsI = StalkerClass.DDSImage.GetIcon(dds.BitmapImage, x, y, w, h);
                    //img_pre_icon.Source = BitmapToImageSource(btsI);
                }
                else
                {
                    //img_pre_icon.Visibility = Visibility.Hidden;
                }

            }
            else
            {
                //img_pre_icon.Visibility = Visibility.Hidden;
            }
        }

        private string LastSection;
        private string LastHeir;
        private string NowSection;

        private object[] CheckOnLtxGood(string allLtx)
        {
            string[] ltxLine = allLtx.Split('\n');
            for(int i = 0; i < ltxLine.Length; i++)
            {
                if (ltxLine[i].StartsWith("[") && !ltxLine[i].Contains("]"))
                    return new object[] { false, ltxLine[i] };

                if (ltxLine[i].Contains("]") && !ltxLine[i].Contains("["))
                    return new object[] { false, ltxLine[i] };

                //if (ltxLine[i].Split(';')[0].Contains("=") && string.IsNullOrWhiteSpace(ltxLine[i].Split(';')[0].Split('=')[1].Trim()) && !ltxLine[i].Split(';')[0].ToUpper().Contains("discovery_dependency".ToUpper()))
                    //return new object[] { false, $"null value:{ltxLine[i]}" };
            }

            return new object[] { true, -1 };
        }

        private void GoNext(bool message = false)
        {

            TreeViewItem selected = ((TreeViewItem)(treeBrowser.SelectedItem));
            string txtSelect = txt_ltx_file.SelectedText;
            txtSelect = txtSelect.Replace("#include", "").Trim().Trim('\"');

            string nowPath = ((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString().Replace(ProgramData.GetLastSplash(((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString()), "");

            string pathOpen = nowPath + "\\" + txtSelect;

            pathOpen = pathOpen.Replace(@"\\", "\\");
            setCurrentElementBrowser((TreeViewItem)(treeBrowser.Items[0]),pathOpen);

            if (File.Exists(pathOpen))
                OpenFile(pathOpen);
            else if(message)
            {
                StalkerClass.HierarchyLtx.Ltx_Parametr par = new StalkerClass.HierarchyLtx.Ltx_Parametr(txtSelect);
                if (par.IsValue)
                {
                    if (par.Value_Parametr.Contains("\\"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{ProgramData.Gamedata + "\\" + par.Value_Parametr}");
                        Console.ForegroundColor = ConsoleColor.White;
                        //TODO: написать открытия файла
                      //  OpenFile(ProgramData.Gamedata + "\\" + par.Value_Parametr);
                    }
                }
            }
        }

        private void txt_ltx_file_MouseEnter(object sender, MouseEventArgs e)
        {
            bool have = false;
            string _InWord = txt_ltx_file.SelectedText.Split('=')[0].Trim();
            foreach(var vI in Words.AllWords)
            {
                foreach(var vS in vI.Value.Datas)
                {
                  if(_InWord == vS.NameParametr)
                    {
                        txt_ltx_file.ToolTip = new ToolTip();
                        ((ToolTip)(txt_ltx_file.ToolTip)).Content = vS.DataParametr.Replace("\\n","\n");
                        have = true;
                    }
                }
            }
            if (!have)
                txt_ltx_file.ToolTip = null;
        }

        private void txt_ltx_file_MouseMove(object sender, MouseEventArgs e)
        {
            bool have = false;
            string _InWord = txt_ltx_file.SelectedText.Split('=')[0].Trim();
            foreach (var vI in Words.AllWords)
            {
                foreach (var vS in vI.Value.Datas)
                {
                    if (_InWord == vS.NameParametr)
                    {
                        txt_ltx_file.ToolTip = new ToolTip();
                        ((ToolTip)(txt_ltx_file.ToolTip)).Content = vS.DataParametr.Replace("\\n", "\n");
                        have = true;
                    }
                }
            }
            if (!have)
                txt_ltx_file.ToolTip = null;
        }

        private void txtSelectGamedata_KeyUp(object sender, KeyEventArgs e)
        {
            if(Directory.Exists(txtSelectGamedata.Text) && e.Key == Key.Enter)
            {
                if(txtSelectGamedata.Text == Properties.Settings.Default.ImportPath)
                {
                    if(MessageBox.Show("Вы точно хотите начать редактировать путь импорта?","Подтвердите действие",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                ProgramData.Gamedata = txtSelectGamedata.Text;
                SelectGamedata.Visibility = Visibility.Hidden;
                browser.Visibility = Visibility.Visible;
                List<string> oth = Properties.Settings.Default.LastOpenIndex.Split(';').ToList<string>();
                if (oth.Where(x => x.ToUpper().Replace(";","") == ProgramData.Gamedata.ToUpper()).Count() <= 0)
                {
                    if (txtSelectGamedata.Text != Properties.Settings.Default.ImportPath)
                    {
                        Properties.Settings.Default.LastOpenIndex += ProgramData.Gamedata + ";";
                        Properties.Settings.Default.Save();
                    }
                }
                ProgramData.LoadDataBrowser(ProgramData.Gamedata,treeBrowser);
                if (ProgramData.xmlStrings.Count <= 0 && Properties.Settings.Default.InitXmlStat)
                {
                    ProgramData.GetValueByLinkText("ammo",true);
                }
            }
            else if (File.Exists(txtSelectGamedata.Text) && e.Key == Key.Enter)
            {
                SelectGamedata.Visibility = Visibility.Hidden;
                browser.Visibility = Visibility.Visible;
                OpenFile(txtSelectGamedata.Text);
            }
        }

        private void ltx_f_copy_Click(object sender, RoutedEventArgs e)
        {
            string outText = txt_ltx_file.SelectedText;
            if(!string.IsNullOrWhiteSpace(outText))
            {
                Clipboard.SetText(outText);
            }
        }

        private void ltx_f_next_Click(object sender, RoutedEventArgs e)
        {
            GoNext(true);
        }

        private void treeBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        
        private void setLinked_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.HierarchyIncludeWin hierarchyInclude = new StalkerWin.HierarchyIncludeWin(ProgramData.Gamedata);
            hierarchyInclude.Owner = this;
            hierarchyInclude.ShowDialog();
        }

        private void repOldLink_Click(object sender, RoutedEventArgs e)
        {

            string excluseFile = treeBrowser.SelectedItem == null ? null : ((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString();
            StalkerWin.ReplaceOldLinkWin rep = new StalkerWin.ReplaceOldLinkWin(ProgramData.Gamedata, excluseFile);
            rep.Owner = this;
            rep.ShowDialog();

        }

        private void btn_grid_clear_dds_Click(object sender, RoutedEventArgs e)
        {
            OpenFile(OpenFileInputNow);
        }

        //private void CutDDS()
        //{
        //    //HERE LOGIC FOR WPN
        //    //TODO: переделать логику cut для other dds
        //    LtxLanguage.LtxData dat = new LtxLanguage.LtxData(txt_dds_grid.Text);
        //    if (dat.LtxSectionDatas.Count >= 1 && dat.LtxSectionDatas[0].GetParametr("inv_grid_x") != null && dat.LtxSectionDatas[0].GetParametr("inv_grid_y") != null && dat.LtxSectionDatas[0].GetParametr("inv_grid_width") != null && dat.LtxSectionDatas[0].GetParametr("inv_grid_height") != null)
        //    {
        //        try
        //        {
        //            StalkerClass.DDSImage dds = new StalkerClass.DDSImage(File.ReadAllBytes(FindIconDDS()));
        //            int x = int.Parse(dat.LtxSectionDatas[0].GetParametr("inv_grid_x").DataParametr);
        //            int y = int.Parse(dat.LtxSectionDatas[0].GetParametr("inv_grid_y").DataParametr);
        //            int w = int.Parse(dat.LtxSectionDatas[0].GetParametr("inv_grid_width").DataParametr);
        //            int h = int.Parse(dat.LtxSectionDatas[0].GetParametr("inv_grid_height").DataParametr);
        //            if (x != -1 && y != -1 && w != -1 && h != -1)
        //            {
        //                try
        //                {
        //                    if (image_dds.Source == null)
        //                        return;
        //                    System.Drawing.Bitmap icon = StalkerClass.DDSImage.GetIcon(dds.BitmapImage, x, y, w, h);
        //                    if (icon == null)
        //                        return;
        //                    image_dds.Width = icon.Width;
        //                    image_dds.Height = icon.Height;

        //                    image_dds.Source = BitmapToImageSource(icon);
        //                }
        //                catch
        //                {

        //                }
        //            }
        //            else
        //            {
        //                OpenFile(OpenFileInputNow);
        //                Initialize_CutDDS();
        //            }
        //        }
        //        catch (Exception g)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Red;
        //            Console.WriteLine(g.Message + " | " + g);
        //            Console.ForegroundColor = ConsoleColor.White;
        //        }
        //    }
        //}

        private void byFileOp_Click(object sender, RoutedEventArgs e)
        {
            if(LastOpenFileInput != null)
            {
                bool si = setCurrentElementBrowser((TreeViewItem)treeBrowser.Items[0], LastOpenFileInput);
                
                string dirLastFile = $"{LastOpenFileInput.Replace(ProgramData.GetLastSplash(LastOpenFileInput),"")}";
                OpenFile(LastOpenFileInput,false);
                if (!si)
                {
                    //here we initialize new folder for last file
                    ProgramData.LoadDataBrowser(dirLastFile,treeBrowser);
                    setCurrentElementBrowser((TreeViewItem)treeBrowser.Items[0], OpenFileInputNow);
                }
            }
        }

        private void tp_setting_win_Click(object sender, RoutedEventArgs e)
        {
            SettingWin win = new SettingWin();
            win.Owner = this;
            win.ShowDialog();

        }

        private void tp_see_setka_dds_Click(object sender, RoutedEventArgs e)
        {
            ProgramData.Show_Setka_DDS = !ProgramData.Show_Setka_DDS ? true : false;
        }

        private void image_dds_MouseMove(object sender, MouseEventArgs e)
        {
            //




        }

        private Point _pEnd;

        private int[] inv_grid_int = null;

        private void DeleteElementTreeView(string tagElement,TreeViewItem items)
        {
            for(int i = 0; i < items.Items.Count; i++)
            {
                if (((TreeViewItem)(items.Items[i])).Tag.ToString() == tagElement)
                {
                    items.Items.RemoveAt(i);
                    break;
                }
                else if(((TreeViewItem)(items.Items[i])).Items.Count > 0)
                {
                    DeleteElementTreeView(tagElement, ((TreeViewItem)(items.Items[i])));
                }
            }
            
        }

        private void brow_menu_del_Click(object sender, RoutedEventArgs e)
        {
            if (treeBrowser.SelectedItem != null)
            {
                if (MessageBox.Show($"Вы уверены что хотите удалить файл \'{ProgramData.GetLastSplash(((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString().Trim('\\'))}\'?\nЭтот файл нельзя будет восстановить.", "Удаления файла?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                       File.Delete($"{((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString()}");

                        string name = ProgramData.GetLastSplash(((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString().Trim('\\'));

                        if ((TreeViewItem)((TreeViewItem)(treeBrowser.SelectedItem)).Parent != null)
                        {
                            DeleteElementTreeView(((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString(), (TreeViewItem)((TreeViewItem)(treeBrowser.SelectedItem)).Parent);
                        }
                        else
                        {
                            ProgramData.LoadDataBrowser(ProgramData.Gamedata, treeBrowser);
                        }

                        MessageBox.Show($"Файл успешно удалён", "Файл удалён", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch(Exception g)
                    {
                        MessageBox.Show($"Не удалось удалить файл!\n{g.Message}", "Ошибка удаления файла!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void open_inp_browser_Click(object sender, RoutedEventArgs e)
        {
            //if(toolTipHeaderFiles.SelectedIndex != -1)
            //{
            //    string fullPath = ((FileInfo)(((ListBoxItem)(toolTipHeaderFiles.SelectedItem)).Tag)).FullName;
            //    string dir = $"{fullPath.Replace(ProgramData.GetLastSplash(fullPath), "")}";

            //    ProgramData.LoadDataBrowser(dir,treeBrowser);

            //    setCurrentElementBrowser((TreeViewItem)treeBrowser.Items[0], fullPath);
            //}
        }

        private void btn_set_inv_for_Click(object sender, RoutedEventArgs e)
        {
            if(inv_grid_int.Length >= 4 && inv_grid_int[0] != -1 && inv_grid_int[1] != -1 && inv_grid_int[2] != -1 && inv_grid_int[3] != -1)
            {
                //need win for selected item
                //StalkerWin.SelectLtxForIconWin selct = new StalkerWin.SelectLtxForIconWin(inv_grid_int, (BitmapImage)image_dds.Source, ProgramData.Gamedata);
                //selct.ShowDialog();
            }
        }

        private void btn_set_on_last_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Mode_ltx_form)
            {
                string ltxFile = LastOpenFileInput;
                if (inv_grid_int.Length >= 4 && inv_grid_int[0] != -1 && inv_grid_int[1] != -1 && inv_grid_int[2] != -1 && inv_grid_int[3] != -1)
                {


                    StalkerClass.HierarchyLtx.LtxFile data = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
                    int indexCursor = txt_ltx_file.CaretOffset;
                    string section = NowSection;
                    if (indexCursor >= 0)
                    {
                        foreach (var v in data.Sections)
                        {
                            int _index = txt_ltx_file.Text.IndexOf("[" + v.Name_Section + "]");
                            if (indexCursor >= _index)
                                section = v.Name_Section;
                        }

                    }

                    data.Sections.Where(x => x.Name_Section == section).First().Parametrs.Where(x => x.Name_Parametr == "inv_grid_x").First().Value_Parametr = inv_grid_int[0].ToString();
                    data.Sections.Where(x => x.Name_Section == section).First().Parametrs.Where(x => x.Name_Parametr == "inv_grid_y").First().Value_Parametr = inv_grid_int[1].ToString();
                    data.Sections.Where(x => x.Name_Section == section).First().Parametrs.Where(x => x.Name_Parametr == "inv_grid_width").First().Value_Parametr = inv_grid_int[2].ToString();
                    data.Sections.Where(x => x.Name_Section == section).First().Parametrs.Where(x => x.Name_Parametr == "inv_grid_height").First().Value_Parametr = inv_grid_int[3].ToString();

                    File.WriteAllText(ltxFile, data.ToString(), ProgramData.Encoding_LTX);
                    txt_ltx_file.Text = data.ToString();

                    MessageBox.Show("Иконка изменена!", "Изменение иконки", MessageBoxButton.OK, MessageBoxImage.Information);

                    btn_set_on_last.Visibility = Visibility.Hidden;

                    OpenFile(LastOpenFileInput);

                }
            }
            else
            {
                if (inv_grid_int.Length >= 4 && inv_grid_int[0] != -1 && inv_grid_int[1] != -1 && inv_grid_int[2] != -1 && inv_grid_int[3] != -1)
                {
                    string file = LastOpenFileInput;
                    StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(file), ProgramData.Encoding_LTX);
                    if(combo_section.SelectedIndex <= fls.Sections.Count)
                    {
                        int sectI = combo_section.SelectedIndex;
                        fls.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == "inv_grid_x").First().Value_Parametr = inv_grid_int[0].ToString();
                        fls.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == "inv_grid_y").First().Value_Parametr = inv_grid_int[1].ToString();
                        fls.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == "inv_grid_width").First().Value_Parametr = inv_grid_int[2].ToString();
                        fls.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == "inv_grid_height").First().Value_Parametr = inv_grid_int[3].ToString();
                        File.WriteAllText(file, fls.ToString(), ProgramData.Encoding_LTX);

                        OpenFile(LastOpenFileInput);
                        btn_set_on_last.Visibility = Visibility.Hidden;
                        combo_section.SelectedIndex = sectI;
                        MessageBox.Show("Иконка изменена!", "Изменение иконки", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        public string FindIconDDS()
        {
            if (!Directory.Exists(ProgramData.Gamedata) && OpenFileInputNow != null && OpenFileInputNow.EndsWith(".dds"))
                return OpenFileInputNow;
            else if (OpenFileInputNow != null && OpenFileInputNow.EndsWith(".dds"))
                return OpenFileInputNow;
            else if (!Directory.Exists(ProgramData.Gamedata))
                return null;

            string pathToMainGamedata = "";
            if (ProgramData.Gamedata.ToUpper().Contains("gamedata".ToUpper()))
            {

                for (int i = 0; i < ProgramData.Gamedata.Split('\\').Length; i++)
                {
                    if (ProgramData.Gamedata.Split('\\')[i].ToUpper().Contains("gamedata".ToUpper()))
                    {
                        pathToMainGamedata += "\\" + ProgramData.Gamedata.Split('\\')[i];
                        break;
                    }
                    else
                    {
                        pathToMainGamedata += "\\" + ProgramData.Gamedata.Split('\\')[i];
                    }
                }

            }
            else
                pathToMainGamedata = ProgramData.Gamedata;

            pathToMainGamedata = pathToMainGamedata.Trim('\\');

            if (File.Exists(Properties.Settings.Default.ui_icon))
            {
                return Properties.Settings.Default.ui_icon;
            }

            if (File.Exists($"{pathToMainGamedata}\\textures\\ui\\{Properties.Settings.Default.ui_icon}"))
            {
                return $"{pathToMainGamedata}\\textures\\ui\\{Properties.Settings.Default.ui_icon}";
            }
            else
            {
                if (Directory.Exists(pathToMainGamedata + "\\textures\\"))
                    pathToMainGamedata += "\\textures\\";
                if (Directory.Exists(pathToMainGamedata + "ui\\"))
                    pathToMainGamedata += "ui\\";

                StalkerHierarchyElement.FinderElement find = new StalkerHierarchyElement.FinderElement(pathToMainGamedata, ProgramData.Encoding_XML, ProgramData.Encoding_LTX);

                List<FileInfo> files = find.GetGlobalFilesOnName(Properties.Settings.Default.ui_icon,true);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Properties.Settings.Default.ui_icon);
                Console.WriteLine(files.Count);
                Console.ForegroundColor = ConsoleColor.White;
                if (files.Count >= 1 && files[0] != null && files[0].Name.EndsWith(".dds"))
                {
                    if(MessageBox.Show("Запомнить найденый путь", "Найден путь для иконки", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Properties.Settings.Default.ui_icon = files[0].FullName;
                        Properties.Settings.Default.Save();
                    }
                    return files[0].FullName;
                }
                else
                {

                }
            }

            return null;
        }

        private void toolTip_change_icon_Click(object sender, RoutedEventArgs e)
        {
            //TODO: HERE DO CHANGE ICON BY LTX
            if (txt_ltx_file.Text.Contains("inv_grid") && txt_ltx_file.Visibility == Visibility.Visible)
            {



                if(File.Exists(FindIconDDS()))
                {

                    OpenFile(FindIconDDS());
                    btn_set_on_last.Visibility = Visibility.Visible;
                }
                else
                {
                    StalkerHierarchyElement.FinderElement find = new StalkerHierarchyElement.FinderElement(ProgramData.Gamedata, ProgramData.Encoding_XML, ProgramData.Encoding_LTX);

                    List<FileInfo> files = find.GetGlobalFilesOnName(Properties.Settings.Default.ui_icon);
                    if(files.Count >= 1 && files[0] != null && files[0].Name.EndsWith(".dds"))
                    {
                        OpenFile(files[0].FullName);
                        btn_set_on_last.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if(MessageBox.Show("Внимание! Файл по имени: \'ui_icon_equipment.dds\' не найден!\nВыбрать в ручную?", "Поиск иконок",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
                            file.Title = "Выберите файл с иконками";
                            file.Filter = "dds|*.dds|*.*|*.*";
                            if(file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                OpenFile(file.FileName);
                                btn_set_on_last.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("В данном файле не найдены параметры связанные с иконками", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(comboEncoding.SelectedIndex != -1)
            {

                Properties.Settings.Default.Encoding_LTX = comboEncoding.SelectedIndex;
                Properties.Settings.Default.Save();
                SetEncoding();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            StalkerWin.Creator.CreateNewFiles cr = new StalkerWin.Creator.CreateNewFiles(ProgramData.Gamedata);
            cr.Owner = this;
            cr.ShowDialog();
            if(cr.FileName != null)
            {
                ((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Add(new TreeViewItem() { Header = cr.FileName,Tag = new FileInfo(ProgramData.Gamedata + "\\" + cr.FileName) });
            }
            if (cr.Restart)
            {
                ProgramData.LoadDataBrowser(ProgramData.Gamedata, treeBrowser);
            }
        }

        private void brow_menu_add_Click(object sender, RoutedEventArgs e)
        {
            if(treeBrowser.SelectedItem != null)
            {
                string path = ((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString();
                if (File.Exists(path))
                {
                    path = new FileInfo(path).FullName.Replace(new FileInfo(path).Name, "");
                }
                Console.WriteLine(((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString() + "\n" + path);
                StalkerWin.Creator.CreateNewFiles cr = new StalkerWin.Creator.CreateNewFiles(path);
                cr.Owner = this;
                cr.ShowDialog();
                if (cr.FileName != null)
                {
                    ((TreeViewItem)(((TreeViewItem)(treeBrowser.SelectedItem)).Parent)).Items.Add(new TreeViewItem() { Header = cr.FileName, Tag = new FileInfo(path + "\\" + cr.FileName) });
                }
            }
        }

        private void treeBrowser_KeyUp(object sender, KeyEventArgs e)
        {
            if(treeBrowser.SelectedItem != null)
            {
                if (e.Key == Key.Enter)
                    OpenFile(((TreeViewItem)(treeBrowser.SelectedItem)).Tag.ToString(), false);
            }
        }

        private void combo_section_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(combo_section.SelectedIndex != -1 && OpenFileInputNow != null)
            {
                ClearLtxModeElement();
                NowSection = combo_section.Items[combo_section.SelectedIndex].ToString();
                StalkerClass.HierarchyLtx.LtxFile ltxF = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(OpenFileInputNow), ProgramData.Encoding_LTX);
                AddLtxModeElement(ltx_parametr, ltxF, combo_section.SelectedIndex);
                if (combo_section.SelectedIndex != -1)
                {
                    try
                    {
                        if (ltxF.Sections.Count > combo_section.SelectedIndex && ltxF.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == "inv_grid_x" || x.Name_Parametr == "inv_grid_y").Count() > 0)
                        {
                            Initialize_Pre_Icon(combo_section.SelectedIndex);
                        }
                        else
                        {
                            //img_pre_icon.Visibility = Visibility.Hidden;
                        }
                    }
                    catch(IndexOutOfRangeException)
                    {
                        combo_section.SelectedIndex = 0;
                        if (ltxF.Sections[combo_section.SelectedIndex].Parametrs.Where(x => x.Name_Parametr == "inv_grid_x" || x.Name_Parametr == "inv_grid_y").Count() > 0)
                        {
                            Initialize_Pre_Icon(combo_section.SelectedIndex);
                        }
                        else
                        {
                            //img_pre_icon.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ByWin win = new ByWin();
            win.Owner = this;
            win.Show();
        }

        private void checkTextures_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.CheckTexturesWin check = new StalkerWin.CheckTexturesWin(ProgramData.Gamedata);
            check.Owner = this;
            check.ShowDialog();
        }

        //окно умной вставки тут!
        private void ltx_vstavka_Click(object sender, RoutedEventArgs e)
        {
            //при ltx нужно определить какой параметр мы заменяем
            //при textbox просто меняет по sender
           


            MenuItem that = (MenuItem)sender;
            string pole = that.Tag.ToString();
            Console.WriteLine(pole);
            if (pole.StartsWith("LINK/"))
            {
                pole = pole.Split('/')[2];
            }
                
            bool a = false;
            if (pole == "txt_ltx_file")
            {

                string prm = txt_ltx_file.SelectedText;
                if(new StalkerClass.HierarchyLtx.Ltx_Parametr(prm) != null && new StalkerClass.HierarchyLtx.Ltx_Parametr(prm).Value_Parametr != null)
                {
                    pole = new StalkerClass.HierarchyLtx.Ltx_Parametr(prm).Name_Parametr;
                    a = true;
                }
            }

            ICSharpCode.AvalonEdit.TextEditor txt = null;
            foreach(FrameworkElement v in ltx_mode_elemetns.Children)
            {
                if (v is ICSharpCode.AvalonEdit.TextEditor && v.Tag.ToString() == pole)
                    txt = (ICSharpCode.AvalonEdit.TextEditor)v;
                if(v is TextBox && v.Tag != null && v.Tag.ToString().Split('/').Length >= 2 && v.Tag.ToString().Split('/')[2] == pole)
                {
                    txt = (ICSharpCode.AvalonEdit.TextEditor)v;
                }
            }

            if (a && txt == null)
                txt = txt_ltx_file;
            
            if (txt == null)
                return;


            if (ProgramData.PaternWinThread == null)
                ProgramData.PaternWinThread = new StalkerWin.PaternWin(ProgramData.Gamedata, pole);
            ProgramData.PaternWinThread.Owner = this;
            ProgramData.PaternWinThread.ShowDialog();
            if (ProgramData.PaternWinThread.IsOk && !a)
            {
                if (ProgramData.PaternWinThread.Add)
                {
                    //add
                    txt.Text += ","+ ProgramData.PaternWinThread.TextBody + ",";
                    txt.Text = txt.Text.Trim(',');
                }
                else
                {
                    //set
                    txt.Text = ProgramData.PaternWinThread.TextBody;
                }
                //SaveModeparametrNoEvent(txt);
            }
            else if(ProgramData.PaternWinThread.IsOk && a)
            {

                StalkerClass.HierarchyLtx.LtxFile f = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
                if (f.Sections.Where(x => x.Name_Section == NowSection).Count() > 0 && f.Sections.Where(x => x.Name_Section == NowSection).First().Parametrs.Where(x => x.Name_Parametr == pole).Count() > 0)
                {
                    if (!ProgramData.PaternWinThread.Add)
                        f.Sections.Where(x => x.Name_Section == NowSection).First().Parametrs.Where(x => x.Name_Parametr == pole).First().Value_Parametr = ProgramData.PaternWinThread.TextBody;
                    else
                    {
                        f.Sections.Where(x => x.Name_Section == NowSection).First().Parametrs.Where(x => x.Name_Parametr == pole).First().Value_Parametr += "," + ProgramData.PaternWinThread.TextBody + ",";
                        f.Sections.Where(x => x.Name_Section == NowSection).First().Parametrs.Where(x => x.Name_Parametr == pole).First().Value_Parametr = f.Sections.Where(x => x.Name_Section == NowSection).First().Parametrs.Where(x => x.Name_Parametr == pole).First().Value_Parametr.Trim(',');
                    }
                    txt_ltx_file.Text = f.ToString().Replace("\r", "");
                }
            }

        }

        private void ltx_act_virovni_Click(object sender, RoutedEventArgs e)
        {
            StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
            foreach(var v in fls.Sections)
            {
                foreach(var el in v.Parametrs)
                {
                    el.Space_IN = "\t\t\t";
                    el.Space_OUT = "\t";


                }
            }

            txt_ltx_file.Text = fls.ToString();
        }

        private void ltx_act_del_start_des_Click(object sender, RoutedEventArgs e)
        {
            StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
            foreach (var v in fls.Sections)
            {
                List<StalkerClass.HierarchyLtx.Ltx_Parametr> del = new List<StalkerClass.HierarchyLtx.Ltx_Parametr>();
                foreach (var el in v.Parametrs)
                {
                    if(el.Value_Parametr == null && el.Desription_Parametr != null && el.Name_Parametr == null)
                    {
                        del.Add(el);
                    }
                }

                foreach(var d in del)
                {
                    v.Parametrs.Remove(d);
                }

             
            }
            txt_ltx_file.Text = fls.ToString();
        }

        private void ltx_act_del_all_des_Click(object sender, RoutedEventArgs e)
        {
            StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
            foreach (var v in fls.Sections)
            {
                foreach (var el in v.Parametrs)
                {
                    if (el.Desription_Parametr != null)
                        el.Desription_Parametr = null;
                }



            }
            txt_ltx_file.Text = fls.ToString();
        }

        private void toolTip_act_static_prm_Click(object sender, RoutedEventArgs e)
        {
            if(OpenFileInputNow != null)
            {
                StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(txt_ltx_file.Text);
                if(NowSection != null && fls.Sections.Where(x => x.Name_Section == NowSection).Count() > 0)
                {
                    StalkerWin.StaticSelectPrm_Win win = new StalkerWin.StaticSelectPrm_Win();
                    win.Owner = this;
                    win.ShowDialog();
                    if (win.IsOk)
                    {
                        StalkerClass.HierarchyLtx.LtxFile stat = new StalkerClass.HierarchyLtx.LtxFile(new System.IO.FileInfo($"{System.Windows.Forms.Application.StartupPath}\\static_prm\\static_hud.ltx"), Encoding.UTF8);

                        foreach (var vi in fls.Sections.Where(x => x.Name_Section == NowSection).First().Parametrs)
                        {
                            foreach(var elStat in stat.Sections.Where(x => x.Name_Section == win.TextBody).First().Parametrs)
                            {
                                if(vi.Name_Parametr == elStat.Name_Parametr)
                                {
                                    vi.Value_Parametr = elStat.Value_Parametr;
                                    break;
                                }
                            }
                        }
                    }
                }
                txt_ltx_file.Text = fls.ToString();

            }
        }

        private void toolTip_act_xml_string_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.xmlstring.XmlStringWin win = new StalkerWin.xmlstring.XmlStringWin();
            win.Owner = this;
            win.Show();
        }

        private void toolTip_addWpnIcon_Click(object sender, RoutedEventArgs e)
        {
            if(OpenFileInputNow != null && OpenFileInputNow.EndsWith(".dds"))
            {
                StalkerWin.AddIconForIconWin icon = new StalkerWin.AddIconForIconWin(OpenFileInputNow);

                icon.ShowDialog();
            }
        }
         
        private void ltx_pack_paste_Click(object sender, RoutedEventArgs e)
        {
            //int input_line = txt_ltx_file.GetLineIndexFromCharacterIndex(txt_ltx_file.CaretOffset);
            int input_line = 0;
            List<FileInfo> vs = new List<FileInfo>();
            vs.Add(new FileInfo($"{System.Windows.Forms.Application.StartupPath}\\Pack_parametr\\OriginalPacks.ltx"));
            StalkerWin.StaticSelectPrm_Win win = new StalkerWin.StaticSelectPrm_Win(vs);
            win.Owner = this;
            win.ShowDialog();

            if (win.IsOk)
            {
                List<string> allText = txt_ltx_file.Text.Split('\n').ToList<string>();
                for(int i = 0; i < allText.Count; i++)
                {
                    if(i == input_line)
                    {
                        allText.Insert(i, win.TextBody);
                    }
                }
                txt_ltx_file.Text = String.Join("\n", allText);
            }

        }

        private void brow_menu_add_marker_Click(object sender, RoutedEventArgs e)
        {
            if(treeBrowser.SelectedItem != null && (treeBrowser.SelectedItem as TreeViewItem).Tag != null)
            {
                string pathMarker = (treeBrowser.SelectedItem as TreeViewItem).Tag.ToString();
                System.Windows.Forms.ColorDialog colorWinD = new System.Windows.Forms.ColorDialog();
                if (AddonClass.ColorMarker.GetMarkerByPathFile(pathMarker) != null)
                    colorWinD.Color = System.Drawing.Color.FromArgb(AddonClass.ColorMarker.GetMarkerByPathFile(pathMarker).Color.R, AddonClass.ColorMarker.GetMarkerByPathFile(pathMarker).Color.G, AddonClass.ColorMarker.GetMarkerByPathFile(pathMarker).Color.B);
                if(colorWinD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    AddonClass.ColorMarker.AddMarkerByPathFile(pathMarker, $"{colorWinD.Color.R},{colorWinD.Color.G},{colorWinD.Color.B}");
                    (treeBrowser.SelectedItem as TreeViewItem).Foreground = AddonClass.ColorMarker.GetMarkerByPathFile(pathMarker);
                }
            }
        }

        private void brow_menu_del_marker_Click(object sender, RoutedEventArgs e)
        {
            if (treeBrowser.SelectedItem != null && (treeBrowser.SelectedItem as TreeViewItem).Tag != null)
            {
                string pathMarker = (treeBrowser.SelectedItem as TreeViewItem).Tag.ToString();
                AddonClass.ColorMarker.RemoveMarkerByPathFile(pathMarker);
                (treeBrowser.SelectedItem as TreeViewItem).Foreground = new SolidColorBrush(Colors.White);
                (treeBrowser.SelectedItem as TreeViewItem).Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void check_error_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.LoggerFinder win = new StalkerWin.LoggerFinder();
            win.Show();
        }


        private void treeBrowser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (treeBrowser.SelectedItem != null && treeBrowser.SelectedItem is TreeViewItem)
            {
                if(treeBrowser.ToolTip == null)
                    treeBrowser.ToolTip = new ToolTip();

                (treeBrowser.ToolTip as ToolTip).Content = new FileInfo((treeBrowser.SelectedItem as TreeViewItem).Tag.ToString()).Name;
                (treeBrowser.ToolTip as ToolTip).IsOpen = false;
                (treeBrowser.ToolTip as ToolTip).IsOpen = true;
            }
        }

        private void treeBrowser_MouseLeave(object sender, MouseEventArgs e)
        {
            if(treeBrowser.ToolTip != null)
            {
                (treeBrowser.ToolTip as ToolTip).IsOpen = false;
                treeBrowser.ToolTip = null;
            }
        }

        private void treeBrowser_MouseEnter(object sender, MouseEventArgs e)
        {
            if (treeBrowser.ToolTip != null)
            {
                (treeBrowser.ToolTip as ToolTip).IsOpen = false;
                treeBrowser.ToolTip = null;
            }
        }

        private void toolTip_aworkDialogs_Click(object sender, RoutedEventArgs e)
        {
            //xrSDK.Editor.DialogEditor\settings.json
            if (Directory.Exists(ProgramData.Gamedata)) {

                Json.Rootobject objSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Rootobject>(File.ReadAllText($"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\xrSDK.Editor.DialogEditor\\settings.json", Encoding.UTF8));

                objSetting.Application.GameDirectory = ProgramData.Gamedata;

                Json.FileImport.WriteInFileJson($"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\xrSDK.Editor.DialogEditor\\settings.json", objSetting);
                

            }

            System.Diagnostics.Process.Start($"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\xrSDK.Editor.DialogEditor\\xrSDK.Editor.DialogEditor.App.exe");
        }

        private void nm_ltx_add_parametr_Click(object sender, RoutedEventArgs e)
        {
            if(combo_section.SelectedItem != null)
            {
                StalkerWin.Dialogs.Enter_string_win enterD = new StalkerWin.Dialogs.Enter_string_win("Введите параметр", "Введите названия нового параметра");
                enterD.Owner = this;
                enterD.ShowDialog();
                if (enterD.IsOk)
                {
                    string nameParametr = enterD.TextBody;
                    string valueParametr = "";

                    StalkerClass.HierarchyLtx.LtxFile fLtx = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(OpenFileInputNow), ProgramData.Encoding_LTX);
                    if (fLtx.Sections.Where(x => x.Name_Section == fLtx.Sections[combo_section.SelectedIndex].Name_Section).Count() > 0)
                    {
                        fLtx.Sections[combo_section.SelectedIndex].Parametrs.Add(new StalkerClass.HierarchyLtx.Ltx_Parametr($"{nameParametr}\t\t=\t{valueParametr}"));
                        ClearLtxModeElement();
                        AddLtxModeElement(ltx_parametr, fLtx, combo_section.SelectedIndex);
                        File.WriteAllText(OpenFileInputNow, fLtx.ToString(), ProgramData.Encoding_LTX);
                    }
                      


                }
            }
        }

        private bool SaveScript_Logic(bool message = false)
        {
            if(grid_script.Visibility == Visibility.Visible)
            {
                try
                {
                    File.WriteAllText(OpenFileInputNow, txt_script.Text, ProgramData.Encoding_Script);
                    if (message)
                        MessageBox.Show("Успешно сохранён.", $"Файл \"{new FileInfo(OpenFileInputNow).Name}\"", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                catch
                {
                    
                }

            }
            return false;
        }


        private void LogicHints(object sender,KeyEventArgs e)
        {
            if (ProgramData.Hints.clsKey.Where(x => x == (e.Key)).Count() > 0 || ProgramData.Hints.words_spliter.Where(x => e.Key.ToString().ToUpper().Contains(x.ToString().ToUpper())).Count() > 0)
            {
                ProgramData.InputHints = null;
                if (ProgramData.HintsWin != null)
                    ProgramData.HintsWin.Close();
                ProgramData.HintsWin = null;

            }
            else
            {
                ProgramData.InputHints += e.Key.ToString().Replace("Back", "");
                if (ProgramData.Hints.WHints.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).Count() > 0 || ProgramData.Hints.WHints_Function.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).Count() > 0 || ProgramData.Hints.WHints_Local.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).Count() > 0)
                {
                    if (ProgramData.InputHints != null)
                    {
                        List<string> item = ProgramData.Hints.WHints.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).ToList();

                        if (ProgramData.Hints.WHints_Function.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).Count() > 0)
                            item.AddRange(ProgramData.Hints.WHints_Function.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).ToList());
                        if (ProgramData.Hints.WHints_Local.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).Count() > 0)
                            item.AddRange(ProgramData.Hints.WHints_Local.Where(x => x.ToUpper().StartsWith(ProgramData.InputHints.ToUpper())).ToList());

                        if (ProgramData.HintsWin == null)
                        {
                            // Console.WriteLine(InputWord);
                            ProgramData.HintsWin = new StalkerClass.HintsWin(item.ToArray(), ProgramData.PasteLogic, ProgramData.InputHints);
                                ProgramData.HintsWin.Owner = this;
                            // ProgramData.HintsWin.Owner = this;
                            Point p = this.PointToScreen(Mouse.GetPosition(this));
                            

                            ProgramData.HintsWin.Show();
                            if (p.X != 0 && p.Y != 0)
                                if (!Properties.Settings.Default.StatPointHints)
                                    ProgramData.HintsWin.SetLocatePoint = p;
                            if (Properties.Settings.Default.StatPointHints)
                            {

                                Point position = scripts_list_func.PointToScreen(new Point(0d, 0d)),
                                controlPosition = this.PointToScreen(new Point(0d, 0d));
                                position = new Point(position.X, position.Y + scripts_list_func.RenderSize.Height);
                                ProgramData.HintsWin.SetLocatePoint = position;
                            }

                            txt_script.Focus();
                        }
                        else
                        {
                            Point lst = ProgramData.HintsWin.SetLocatePoint;
                            ProgramData.HintsWin.Close();
                            ProgramData.HintsWin = new StalkerClass.HintsWin(item.ToArray(), ProgramData.PasteLogic, ProgramData.InputHints);
                                ProgramData.HintsWin.Owner = this;
/*                            Point p = this.PointToScreen(Mouse.GetPosition(this));
                            if (p.X != 0 || p.Y != 0)
                            {
                                ProgramData.HintsWin.SetLocatePoint = p;
                            }
                            else
                                ProgramData.HintsWin.Owner = this;*/

                            ProgramData.HintsWin.Show();
                            if (lst.X != 0 && lst.Y != 0)
                                if (!Properties.Settings.Default.StatPointHints)
                                    ProgramData.HintsWin.SetLocatePoint = lst;
                            if (Properties.Settings.Default.StatPointHints)
                            {
                                Point position = scripts_list_func.PointToScreen(new Point(0d, 0d)),
                                controlPosition = this.PointToScreen(new Point(0d, 0d));
                                position = new Point(position.X, position.Y + scripts_list_func.RenderSize.Height);
                                ProgramData.HintsWin.SetLocatePoint = position;
                            }
                            txt_script.Focus();
                        }
                    }
                }
                else
                {
                    if (ProgramData.HintsWin != null)
                        ProgramData.HintsWin.Close();
                    ProgramData.HintsWin = null;
                }
            }


        }

        private void txt_script_KeyUp(object sender, KeyEventArgs e)
        {


            if (Properties.Settings.Default.UseHints)
                LogicHints(sender, e);


            if(e.Key == Key.F5)
            {
                Properties.Settings.Default.RemoveColorScript = !Properties.Settings.Default.RemoveColorScript;
                Properties.Settings.Default.Save();
                if (Properties.Settings.Default.RemoveColorScript)
                {
                    SyntaxBox.SetEnable(txt_script, false);
                    txt_script.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    txt_script.Foreground = new SolidColorBrush(Colors.Black);
                    SyntaxBox.SetEnable(txt_script, true);
                }
            }
            if(KeyLast_Script_Up == Key.LeftCtrl && e.Key == Key.S || e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                string origTitle = this.Title;

                if(SaveScript_Logic(false))
                {
                    if (!this.Title.Contains("(Скрипт сохранён)"))
                    {
                        this.Title += " (Скрипт сохранён)";
                        Thread th = new Thread(() =>
                        {
                            Thread.Sleep(600);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.Title = origTitle;
                            }));
                        });
                        th.IsBackground = true;
                        th.Start();
                    }
                }
                else
                {
                    if (!this.Title.Contains("(Ошибка сохранения скрипта)"))
                    {
                        this.Title += " (Ошибка сохранения скрипта)";
                        Thread th = new Thread(() =>
                        {
                            Thread.Sleep(500);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.Title = origTitle;
                            }));
                        });
                        th.IsBackground = true;
                        th.Start();
                    }
                }

            }


            KeyLast_Script_Up = e.Key;

        }

        private Key KeyLast_Script_Up;

        private void scripts_list_func_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (scripts_list_func.SelectedItem != null)
            {
                string sect = scripts_list_func.SelectedItem.ToString();

                //Вылет сука
/*                int a_offset = txt_script.Text.IndexOf(sect);
                int b_offset = sect.Length-1;*/
                int line_ = -1;
                int offset_line = 0;
                for (int i = 0; i < txt_script.Text.Split('\n').Length; i++)
                {
                    if (txt_script.Text.Split('\n')[i].Contains(sect) && txt_script.Text.Split('\n')[i].ToUpper().Contains("function".ToUpper()))
                    {
                        line_ = i;
                        offset_line += txt_script.Text.Split('\n')[i].Length;
                    }
                }

                //txt_script.Select(a_offset, b_offset);
                //   Console.WriteLine($"{offset_line} {line_} {txt_script.GetLineLength(line_)} {txt_script.GetLineText(line_)}");

                txt_script.Select(txt_script.GetCharacterIndexFromLineIndex(line_), txt_script.GetLineLength(line_));

                //txt_script.Select(offset_line, txt_script.GetLineLength(line_));
                if (line_ != -1)
                    txt_script.ScrollToLine(line_);
                else
                    txt_script.ScrollToLine(0);

                txt_script.Focus();
            }
        }

        private void txtSelectGamedata_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            //here we work with ignore path
         //   return;
            if (!string.IsNullOrWhiteSpace(txtSelectGamedata.Text) && Directory.Exists(txtSelectGamedata.Text))
            {


                    list_ignore.Items.Clear();

                    AddElementRecursive(ref list_ignore, txtSelectGamedata.Text);

                
            }
        }
      //  List<CheckBox> checking = new List<CheckBox>();
        private void AddElementRecursive(ref ListBox tree,string Dir,bool col = false)
        {

            DirectoryInfo dir = new DirectoryInfo(Dir);
            if (dir.Exists)
            {
                foreach (var vI in dir.GetDirectories())
                {
                    CheckBox treeW = new CheckBox();


                    treeW.Foreground = new SolidColorBrush(Colors.White);
                    treeW.Content = vI.Name;
                    treeW.Tag = vI.FullName;
                    treeW.Checked += TreeW_Checked;
                    treeW.Unchecked += TreeW_Unchecked;
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.IgnorePather) &&Properties.Settings.Default.IgnorePather.Split(',').Where(x => x.ToUpper() == $"{treeW.Tag}".ToUpper()).Count() > 0)
                    {
                        treeW.Foreground = new SolidColorBrush(Colors.Red);
                        treeW.IsChecked = true;
                    }
                    //   checking.Add(treeW);
                    if (col)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine($"{dir.Name} | {dir.FullName}");

                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    tree.Items.Add(treeW);
                }
            }
        }

        private void TreeW_Unchecked(object sender, RoutedEventArgs e)
        {
            (sender as CheckBox).Foreground = new SolidColorBrush(Colors.White);
            Properties.Settings.Default.IgnorePather = Properties.Settings.Default.IgnorePather.Replace((sender as CheckBox).Tag.ToString() + ",", "");
            Properties.Settings.Default.Save();
            return;
        }

        private void TreeW_Checked(object sender, RoutedEventArgs e)
        {
            string elem = (sender as CheckBox).Tag.ToString();
            
            if (Directory.Exists(elem))
            {
                (sender as CheckBox).Foreground = new SolidColorBrush(Colors.Red);
                Properties.Settings.Default.UseIgnorePather = true;

                if (Properties.Settings.Default.IgnorePather.Split(',').Where(x => x.ToUpper() == elem.ToString().ToUpper()).Count() <= 0)
                {
                    Properties.Settings.Default.IgnorePather += elem + ",";
                    Properties.Settings.Default.Save();
                }

            }
        }

        private void otladka_configs_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void toolTip_DialogGSC_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Файлы по типу:\ntext/rus/stable_dialogs_X.xml\ngameplay/dialogs_X.xml\n\nГГ - Синий.\nNpc - Зелёные", "Подсказка", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {
                System.Diagnostics.Process.Start($"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\XML_Dialogs\\DialogEditor.exe");
            }
            catch
            {

            }
            
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            string path = ProgramData.Gamedata;
            if (Directory.Exists($"{path}\\meshes"))
                path += "\\meshes";
            StalkerWin.convert.ogf.ogf_objectWin ogfObj = new StalkerWin.convert.ogf.ogf_objectWin(path);
            ogfObj.Owner = this;
            ogfObj.ShowDialog();
        }

        private void toolTip_Convert_DDS_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.convert.ConvertPythonWin win = new StalkerWin.convert.ConvertPythonWin(ProgramData.Gamedata);
            win.Owner = this;
            win.ShowDialog();
        }

        public static class ProgramData
        {
            public static string Gamedata;

            public static MainWindow MainWinThread;
            public static StalkerWin.PaternWin PaternWinThread;

            public static string GetLastSplash(string txt)
            {

                if (txt == null)
                    return null;

                return txt.Split('\\')[txt.Split('\\').Length - 1];

            }


            public static string GetGamedataFromInCatalogs(string path)
            {
                string vs = null;
                if (path.ToUpper().Contains("gamedata".ToUpper()))
                {
                    string[] splt = path.Split('\\');
                    for(int i = 0; i < splt.Length; i++)
                    {
                        if (splt[i].ToUpper().Contains("gamedata".ToUpper()))
                        {
                            vs += splt[i] + "\\";
                            break;
                        }
                        else
                            vs += splt[i] + "\\";
                    }
                }

                return vs.TrimEnd('\\');
            }

            public static Encoding Encoding_LTX = Encoding.GetEncoding(1251);
            public static Encoding Encoding_XML = Encoding.GetEncoding(1251);
            public static Encoding Encoding_Script
            {
                get
                {
                    if (Properties.Settings.Default.Encoding_Scripts == 0)
                    {
                        return Encoding.UTF8;
                    }
                    if (Properties.Settings.Default.Encoding_Scripts == 1)
                    {
                        return Encoding.GetEncoding(1251);
                    }
                    return Encoding.GetEncoding(1251);
                }
            }

            public static void InitializeComponentEncoding()
            {
                if (Properties.Settings.Default.Encoding_LTX == 0)
                {
                    Encoding_LTX = Encoding.UTF8;
                }
                if (Properties.Settings.Default.Encoding_LTX == 1)
                {
                    Encoding_LTX = Encoding.GetEncoding(1251);
                }

                if (Properties.Settings.Default.Encoding_XML == 0)
                {
                    Encoding_XML = Encoding.UTF8;
                }
                if (Properties.Settings.Default.Encoding_XML == 1)
                {
                    Encoding_XML = Encoding.GetEncoding(1251);
                }
            }

            public static bool Show_Setka_DDS = false;


            public static List<StalkerClass.Xml.Xml_Text_File> xmlStrings = new List<StalkerClass.Xml.Xml_Text_File>();


            public static StalkerClass.Xml.Xml_Text_File GetFileByIdText(string idText)
            {


                foreach (var lFile in xmlStrings)
                {
                    if (lFile.ExpressionsBlocks.Where(x => x.Id == idText).Count() > 0)
                        return lFile;
                }


                return null;

            }
            public static bool SeeDialogLoadXml = false;
            public static bool LoaderXml = false;

            public static string GetConfigOrConfigs(string pathGamedata)
            {
                if (Directory.Exists(pathGamedata + "\\config"))
                    return pathGamedata + "\\config";
                if (Directory.Exists(pathGamedata + "\\configs"))
                    return pathGamedata + "\\configs";
                return null;
            }

            public static string GetValueByLinkText(string id, bool contain = false)
            {
                string findDir = ProgramData.Gamedata;

                if (GetConfigOrConfigs(findDir) != null)
                    findDir = GetConfigOrConfigs(findDir);

                if (Directory.Exists(findDir + "\\text"))
                    findDir += "\\text";

                if (Directory.Exists(findDir + $"\\{Properties.Settings.Default.LocalizationShortPath}"))
                {
                    findDir += $"\\{Properties.Settings.Default.LocalizationShortPath}";
                }



                //  Console.WriteLine(findDir);


                string value = null;

                if (xmlStrings.Count <= 0)
                {


                    if (!SeeDialogLoadXml && !LoaderXml)
                    {
                        if (!Properties.Settings.Default.Replace_Link)
                            return id;
                        string img = $"{System.Windows.Forms.Application.StartupPath}\\Data\\Image\\XML-File.png";
                        StalkerWin.Dialogs.MessageOkCancelWin winM = new StalkerWin.Dialogs.MessageOkCancelWin("Дозагрузка xml-strings", "Загрузить файлы .xml для текста?", img);
                        winM.ShowDialog();
                        bool IsOk = winM.IsOk;
                        if (!IsOk)
                        {
                            SeeDialogLoadXml = true;
                            LoaderXml = false;
                            return id;
                        }
                        else
                        {
                            SeeDialogLoadXml = true;
                            LoaderXml = true;
                        }
                    }
                    else if (!LoaderXml)
                    {
                        return id;
                    }



                    //nothing have
                    //here load all find files
                    StalkerHierarchyElement.FinderElement ObjScanner = new StalkerHierarchyElement.FinderElement(findDir, ProgramData.Encoding_XML, ProgramData.Encoding_LTX);
                    foreach (var v4 in Properties.Settings.Default.text_Links.Split(','))
                    {
                        List<FileInfo> vs = ObjScanner.GetGlobalFilesOnName(v4, true).Where(x => x.FullName.EndsWith(".xml")).ToList();
                        Console.WriteLine(vs.Count);
                        foreach (var v in vs)
                        {
                            if (xmlStrings.Where(x => x._File.FullName == v.FullName).Count() > 0)
                                continue;

                            StalkerClass.Xml.Xml_Text_File f = new StalkerClass.Xml.Xml_Text_File(v, ProgramData.Encoding_XML, true);
                            xmlStrings.Add(f);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Первичная загрузка: {v.Name}");
                            Console.ForegroundColor = ConsoleColor.White;
                            if (!contain)
                            {
                                if (f.ExpressionsBlocks.Where(x => x.Id == id).Count() > 0)
                                {
                                    value = f.ExpressionsBlocks.Where(x => x.Id == id).First().Text;
                                }
                            }
                            else
                            {
                                if (f.ExpressionsBlocks.Where(x => x.Id.Contains(id)).Count() > 0)
                                {
                                    value = f.ExpressionsBlocks.Where(x => x.Id.Contains(id)).First().Text;
                                }
                            }
                        }
                    }
                }
                else if (xmlStrings.Count < Properties.Settings.Default.text_Links.Split(',').Length)
                {
                    StalkerHierarchyElement.FinderElement ObjScanner = new StalkerHierarchyElement.FinderElement(findDir, ProgramData.Encoding_XML, ProgramData.Encoding_LTX);
                    foreach (var v4 in Properties.Settings.Default.text_Links.Split(','))
                    {

                        List<FileInfo> vs = ObjScanner.GetGlobalFilesOnName(v4, true).Where(x => x.FullName.EndsWith(".xml")).ToList();
                        foreach (var v in vs)
                        {
                            if (xmlStrings.Where(x => x._File.FullName == v.FullName).Count() > 0)
                                continue;

                            StalkerClass.Xml.Xml_Text_File f = new StalkerClass.Xml.Xml_Text_File(v, ProgramData.Encoding_XML, true);
                            xmlStrings.Add(f);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Догрузка: {v.Name}");
                            Console.ForegroundColor = ConsoleColor.White;
                            if (!contain)
                            {
                                if (f.ExpressionsBlocks.Where(x => x.Id == id).Count() > 0)
                                {
                                    value = f.ExpressionsBlocks.Where(x => x.Id == id).First().Text;
                                }
                            }
                            else
                            {
                                if (f.ExpressionsBlocks.Where(x => x.Id.Contains(id)).Count() > 0)
                                {
                                    value = f.ExpressionsBlocks.Where(x => x.Id.Contains(id)).First().Text;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var vx in xmlStrings)
                    {
                        if (!contain)
                        {
                            if (vx.ExpressionsBlocks.Where(x => x.Id == id).Count() > 0)
                            {
                                value = vx.ExpressionsBlocks.Where(x => x.Id == id).First().Text;
                            }
                        }
                        else
                        {
                            if (vx.ExpressionsBlocks.Where(x => x.Id.Contains(id)).Count() > 0)
                            {
                                value = vx.ExpressionsBlocks.Where(x => x.Id.Contains(id)).First().Text;
                            }
                        }
                    }

                }

                return value;
            }



            public static void LoadDataBrowser(string path, TreeView treebrowser, List<FileInfo> patternFile = null, Window winTitle = null, bool UseIgonore = true)
            {
                if (!Properties.Settings.Default.UseIgnorePather)
                    UseIgonore = false;

                if (winTitle == null && ProgramData.MainWinThread != null)
                {
                    winTitle = ProgramData.MainWinThread;
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
                string txt = ProgramData.GetLastSplash(path.Trim('\\'));
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



                AddRecurceFolders(path, fItem, UseIgonore, patternFile,winTitle,orig);




                treebrowser.Items.Add(fItem);
                if (orig != null)
                    winTitle.Title = orig;
            }

            private static void AddRecurceFolders(string path, TreeViewItem item, bool UseIgonore = true, List<FileInfo> patternFile = null,Window title = null,string origtitle = null)
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
                    TreeViewItem fS = new TreeViewItem();
                    fS.Tag = v.FullName;

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
                    fS.Header = txt;
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
                    AddRecurceFolders(v.FullName, nE, UseIgonore,null,title,origtitle);
                }
            }


            public static StalkerClass.HintWordStack Hints = new StalkerClass.HintWordStack($"{System.Windows.Forms.Application.StartupPath}\\Hints.ltx");
            public static StalkerClass.HintsWin HintsWin = null;
            public static StalkerClass.HintsWin.PasteDelegate PasteLogic;
            public static string InputHints;
        }

        private void tp_setting_prm_Click(object sender, RoutedEventArgs e)
        {
            SettingGroupsElementWin win = new SettingGroupsElementWin();
            win.Owner = this;
            win.ShowDialog();
        }

        private void toolTip_Convert_DDSTGA_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.convert.conv.OutputDirWinConv outs = new StalkerWin.convert.conv.OutputDirWinConv();
            outs.Owner = this;
            outs.ShowDialog();
        }

        private Key KeyLast_Xml_Up = Key.None;

        private void txt_xml_strings_KeyUp(object sender, KeyEventArgs e)
        {
            if (KeyLast_Xml_Up == Key.LeftCtrl && e.Key == Key.S || e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                string origTitle = this.Title;
                try
                {
                    string nameFile_ = OpenFileInputNow;
                    File.WriteAllText(nameFile_, txt_xml_strings.Text, ProgramData.Encoding_XML);

                    if (!this.Title.Contains("(Xml сохранён)"))
                    {
                        this.Title += " (Xml сохранён)";
                        Thread th = new Thread(() =>
                        {
                            Thread.Sleep(600);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.Title = origTitle;
                            }));
                        });
                        th.IsBackground = true;
                        th.Start();
                    }

                }
                catch (Exception g)
                {
                    MessageBox.Show($"Не удалось сохранить файл!\n[{g.Message}]", "Сохранение файла", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            KeyLast_Xml_Up = e.Key;
        }

        private void ignoreShellAll_Click(object sender, RoutedEventArgs e)
        {
            foreach(CheckBox it in list_ignore.Items)
            {
                it.IsChecked = true;
            }
        }

        private void ignoreShellZeroAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox it in list_ignore.Items)
            {
                it.IsChecked = false;
            }
        }

        private void toolTip_import_files_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.Import.ImportFile_Win impW = new StalkerWin.Import.ImportFile_Win(ProgramData.Gamedata);
            impW.Owner = this;
            impW.ShowDialog();
        }

        private void brow_menu_import_files_Click(object sender, RoutedEventArgs e)
        {
            StalkerWin.Import.ImportFile_Win impW = new StalkerWin.Import.ImportFile_Win(ProgramData.Gamedata);
            impW.Owner = this;
            impW.ShowDialog();
        }

        private void Label_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ap-pro.ru/profile/611-valerok/");
        }

        private void brow_textures_copyPath_Click(object sender, RoutedEventArgs e)
        {
            if(browser_textures.SelectedItem != null)
            {
                string pathF = (browser_textures.SelectedItem as TreeViewItem).Tag.ToString();
                if (File.Exists(pathF))
                {
                    string vsP = "";
                    bool txt = false;
                    for(int io = 0; io < pathF.Split('\\').Length; io++)
                    {
                        if(txt)
                        {
                            vsP += pathF.Split('\\')[io] + "\\";
                        }

                        if (pathF.Split('\\')[io].ToUpper() == "textures".ToUpper())
                        {
                            txt = true;
                            continue;
                        }
                    }
                    vsP = vsP.TrimEnd('\\');
                    vsP = vsP.Replace(vsP.Split('.')[vsP.Split('.').Length - 1], "");
                    vsP = vsP.TrimEnd('.');
                    Clipboard.SetText(vsP);
                }
            }
        }
    }
}
