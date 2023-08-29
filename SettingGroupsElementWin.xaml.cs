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
    /// Логика взаимодействия для SettingGroupsElementWin.xaml
    /// </summary>
    public partial class SettingGroupsElementWin : Window
    {
        public SettingGroupsElementWin()
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            InitializerComponentsLtx();
        }
        Dictionary<string, string> ltx_parametr = new Dictionary<string, string>();
        string pathPrm = $"{System.Windows.Forms.Application.StartupPath}\\GroupsParametr.ltx";
        private void InitializerComponentsLtx()
        {
            tree_parametr.Items.Clear();
            ltx_parametr.Clear();
            if (File.Exists(pathPrm))
            {
                StalkerClass.HierarchyLtx.LtxFile _prm = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                foreach (var v in _prm.Sections)
                {
                    string nameGroup = v.Name_Section;

                    TreeViewItem objI = new TreeViewItem();
                    objI.Header = nameGroup;
                    List<string> prms = new List<string>();
                    foreach (var vPrm in v.Parametrs)
                    {
                        int Ispace = int.Parse(v.Heir_Section.Trim().Split(';')[1]);
                        int ISpace2 = int.Parse(v.Heir_Section.Trim().Split(';')[0]);

                        ltx_parametr.Add(vPrm.Name_Parametr.Trim(), vPrm.Value_Parametr.Trim());
                        if (vPrm.Desription_Parametr != null && vPrm.Desription_Parametr.Split(':').Length > 1)
                        {
                            Ispace = int.Parse(vPrm.Desription_Parametr.Trim().Split(':')[1]);
                            ISpace2 = int.Parse(vPrm.Desription_Parametr.Trim().Split(':')[0]);
                        }
                        AddToGroupElements(objI, Ispace, new string[] { vPrm.Value_Parametr.Trim() }, new FrameworkElement[] { new TextBox() { Name = $"prm_{vPrm.Name_Parametr.Trim()}", Tag = vPrm.Name_Parametr.Trim(), Width = ISpace2 } });
                    }
                    tree_parametr.Items.Add(objI);
                }

            }
        }

        private void InitializerComponentsLtx(bool saveRec)
        {
            List<string> headers = new List<string>();
            foreach(TreeViewItem v in tree_parametr.Items)
            {
                if (v.IsExpanded)
                    headers.Add(v.Header.ToString());
            }
            TreeViewItem objS = (tree_parametr.SelectedItem as TreeViewItem);
            InitializerComponentsLtx();
            foreach(TreeViewItem it in tree_parametr.Items)
            {
                foreach(string v in headers)
                {
                    if (it.Header.ToString() == v)
                        it.IsExpanded = true;
                }
                if (objS != null)
                    if (it.Header.ToString() == objS.Header.ToString())
                        it.IsSelected = true;
            }
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


        private void ClearData()
        {
            txt_description.Text = "";
            txt_parametr.Text = "";
            txt_space.Text = "";
            txt_spaceTextBox.Text = "";
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(tree_parametr.SelectedItem != null && (tree_parametr.SelectedItem is TreeViewItem))
            {
                string Group = (tree_parametr.SelectedItem as TreeViewItem).Header.ToString();
                if (MessageBox.Show($"Вы действительно хотите удалить группу?\n\"{Group}\"","Удаления группы",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                    fls.Sections.Remove(fls.Sections.Where(x => x.Name_Section == Group).First());
                    fls.SaveFile(null, Encoding.UTF8);
                    InitializerComponentsLtx(true);
                    ClearData();
                   // ((TreeViewItem)tree_parametr.SelectedItem).IsSelected = false;
                }
            }
            else if(tree_parametr.SelectedItem != null)
            {
                StackPanel pnl = (tree_parametr.SelectedItem as StackPanel);
                string parentGroup = (pnl.Parent as TreeViewItem).Header.ToString();
                Label lbl = (pnl.Children[0] as Label);
                string nameParametr = lbl.Content.ToString();
                string parametr = lbl.Name.ToString().Replace("lab_prm_", "");
                if(MessageBox.Show($"Вы действительно хотите удалить параметр?\n\"{nameParametr}\" - \"{parametr}\"","Удаления параметра",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                    fls.Sections.Where(x => x.Name_Section == parentGroup).First().Parametrs.Remove(fls.Sections.Where(x => x.Name_Section == parentGroup).First().Parametrs.Where(x => x.Name_Parametr == parametr && x.Value_Parametr == nameParametr).First());
                    fls.SaveFile(null, Encoding.UTF8);

                    InitializerComponentsLtx(true);
                    ClearData();
                    //((TreeViewItem)tree_parametr.SelectedItem).IsSelected = false;
                }
            }
        }

        public StalkerClass.HierarchyLtx.Ltx_Parametr PrmInvoke;
        private string PrmParentI;
        public StalkerClass.HierarchyLtx.Ltx_Section PrmSectInvoke;

        private void tree_parametr_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //ClearData();

            if (grid_workParametr.Visibility == Visibility.Hidden)
                grid_workParametr.Visibility = Visibility.Visible;

            if (tree_parametr.SelectedItem == null)
                return;

            if(!(tree_parametr.SelectedItem is TreeViewItem))
            {
                PrmInvoke = null;
                PrmSectInvoke = null;

                lab_description.Visibility = Visibility.Visible;
                lab_parametr.Visibility = Visibility.Visible;

                txt_description.Visibility = Visibility.Visible;
                txt_parametr.Visibility = Visibility.Visible;


                StackPanel pnl = (tree_parametr.SelectedItem as StackPanel);
                string parentGroup = (pnl.Parent as TreeViewItem).Header.ToString();
                Label lbl = (pnl.Children[0] as Label);
                string nameParametr = lbl.Content.ToString();
                string parametr = lbl.Name.ToString().Replace("lab_prm_", "");

                txt_description.Text = nameParametr;
                txt_description.Tag = parametr;

                txt_parametr.Text = parametr;
                txt_parametr.Tag = parametr;

                StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);

                StalkerClass.HierarchyLtx.Ltx_Parametr prm = fls.Sections.Where(x => x.Name_Section == parentGroup).First().Parametrs.Where(x => x.Name_Parametr == parametr && x.Value_Parametr == nameParametr).First();
                PrmInvoke = prm;
                PrmParentI = parentGroup;
                if(prm != null)
                {
                    //change
                    if(prm.Desription_Parametr != null && prm.Desription_Parametr.Split(':').Length > 1)
                    {
                        int Ispace = int.Parse(prm.Desription_Parametr.Trim().Split(':')[1]);
                        int ISpace2 = int.Parse(prm.Desription_Parametr.Trim().Split(':')[0]);

                        txt_space.Text = Ispace.ToString();
                        txt_spaceTextBox.Text = ISpace2.ToString();
                    }
                    else
                    {
                        txt_space.Text = "";
                        txt_spaceTextBox.Text = "";
                    }
                }
            }
            else
            {
                lab_description.Visibility = Visibility.Hidden;
                lab_parametr.Visibility = Visibility.Hidden;

                txt_description.Visibility = Visibility.Hidden;
                txt_parametr.Visibility = Visibility.Hidden;



                PrmInvoke = null;
                PrmSectInvoke = null;
                //change group
                string group = (tree_parametr.SelectedItem as TreeViewItem).Header.ToString();
                StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                PrmSectInvoke = fls.Sections.Where(x => x.Name_Section == group).First();


                int Ispace = int.Parse(PrmSectInvoke.Heir_Section.Trim().Split(';')[1]);
                int ISpace2 = int.Parse(PrmSectInvoke.Heir_Section.Trim().Split(';')[0]);

                txt_space.Text = Ispace.ToString();
                txt_spaceTextBox.Text = ISpace2.ToString();
            }
        }

        private void btn_inv_Click(object sender, RoutedEventArgs e)
        {
            if(tree_parametr.SelectedItem == null)
            {
                ClearData();
                return;
            }

            if (!(tree_parametr.SelectedItem is TreeViewItem))
            {
                //save parametr
                string descr = txt_description.Text;
                string parametr = txt_parametr.Text;
                
                if(!string.IsNullOrWhiteSpace(descr) && !string.IsNullOrWhiteSpace(parametr))
                {
                    if (PrmInvoke != null)
                    {
                        string OparentGroup = PrmParentI;
                        string OnameParametr = PrmInvoke.Value_Parametr;
                        string Oparametr = PrmInvoke.Name_Parametr.Replace("lab_prm_", "");//сам параметр
                        //change
                        StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);


                        if (!string.IsNullOrWhiteSpace(txt_space.Text) && !string.IsNullOrWhiteSpace(txt_spaceTextBox.Text))
                        {
                            int a = int.Parse(txt_space.Text);
                            int b = int.Parse(txt_spaceTextBox.Text);
                            fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Where(x => x.Name_Parametr == Oparametr && x.Value_Parametr == OnameParametr).First().Desription_Parametr = $"{b}:{a}";
                        }
                        else
                        {

                            fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Where(x => x.Name_Parametr == Oparametr && x.Value_Parametr == OnameParametr).First().Desription_Parametr = null;
                        }

                        fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Where(x => x.Name_Parametr == Oparametr && x.Value_Parametr == OnameParametr).First().Value_Parametr = descr;

                        fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Where(x => x.Name_Parametr == Oparametr).First().Name_Parametr = parametr;

                        fls.SaveFile(null, Encoding.UTF8,false);
                        InitializerComponentsLtx(true);
                    }
                    else
                    {
                        //create
                        /*
                        StackPanel pnl = (tree_parametr.SelectedItem as StackPanel);
                        string OparentGroup = (pnl.Parent as TreeViewItem).Header.ToString();
                        StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                        fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Add(new StalkerClass.HierarchyLtx.Ltx_Parametr("ПАРАМЕТР = ОПИСАНИЕ"));
                        fls.SaveFile(null, Encoding.UTF8);
                        InitializerComponentsLtx(true);*/

                    }
                }
                else
                {
                    MessageBox.Show("Введите данные!\nОбязательные: описание,параметр", "Проблема с данными!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                //save group
                string group = PrmSectInvoke.Name_Section;
                if (!string.IsNullOrWhiteSpace(txt_space.Text) && !string.IsNullOrWhiteSpace(txt_spaceTextBox.Text))
                {
                    int a = int.Parse(txt_space.Text);
                    int b = int.Parse(txt_spaceTextBox.Text);

                    StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                    if (PrmSectInvoke != null)
                    {
                        fls.Sections.Where(x => x.Name_Section == PrmSectInvoke.Name_Section).First().Heir_Section = $"{b};{a}";
                        fls.SaveFile(null,Encoding.UTF8,false);
                        InitializerComponentsLtx(true);
                    }

                }
                else
                {
                    MessageBox.Show("Введите данные!\nОбязательные: пространство,пространство TextBox", "Проблема с данными!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Where(x => x.Name_Parametr == Oparametr && x.Value_Parametr == OnameParametr).First().Desription_Parametr = "";
                }


            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if(PrmSectInvoke != null)
            {
                string OparentGroup = PrmSectInvoke.Name_Section;
                StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                if (fls.Sections.Where(x => x.Name_Section == OparentGroup).Count() > 0)
                {
                    fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Add(new StalkerClass.HierarchyLtx.Ltx_Parametr("ПАРАМЕТР = ОПИСАНИЕ"));
                    fls.SaveFile(null, Encoding.UTF8);
                    InitializerComponentsLtx(true);
                }
                else
                {
                    MessageBox.Show("Не удалось создать параметр по причине: \"Нет группы\"", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if(tree_parametr.SelectedItem is StackPanel)
            {
                StackPanel pnl = (tree_parametr.SelectedItem as StackPanel);
                string OparentGroup = (pnl.Parent as TreeViewItem).Header.ToString();
                StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
                if (fls.Sections.Where(x => x.Name_Section == OparentGroup).Count() > 0)
                {
                    fls.Sections.Where(x => x.Name_Section == OparentGroup).First().Parametrs.Add(new StalkerClass.HierarchyLtx.Ltx_Parametr("ПАРАМЕТР = ОПИСАНИЕ"));
                    fls.SaveFile(null, Encoding.UTF8);
                    InitializerComponentsLtx(true);
                }
                else
                {
                    MessageBox.Show("Не удалось создать параметр по причине: \"Нет группы\"", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            StalkerClass.HierarchyLtx.LtxFile fls = new StalkerClass.HierarchyLtx.LtxFile(new FileInfo(pathPrm), Encoding.UTF8);
            StalkerWin.Dialogs.Enter_string_win stringText = new StalkerWin.Dialogs.Enter_string_win("Введите название группы", "Название группы");
            stringText.Owner = this;
            stringText.ShowDialog();
            if (stringText.IsOk)
            {
                fls.Sections.Add(new StalkerClass.HierarchyLtx.Ltx_Section()
                {
                    Name_Section = stringText.TextBody,
                    Heir_Section = "100;170"
                });
                fls.SaveFile(null, Encoding.UTF8, false);
                InitializerComponentsLtx(true);
            }
        }
    }
}
