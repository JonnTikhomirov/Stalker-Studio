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

namespace Stalker_Studio.StalkerWin.xmlstring
{
    /// <summary>
    /// Логика взаимодействия для XmlStringWin.xaml
    /// </summary>
    public partial class XmlStringWin : Window
    {
        public XmlStringWin()
        {
            InitializeComponent();
            InterfaceHelper.InitializeWindow(this);
            Initialize_ListFile(MainWindow.ProgramData.xmlStrings);
        }

        private List<StalkerClass.Xml.Xml_Text_File> XmlFiles;
        private StalkerClass.Xml.Xml_Text_File InputXml;
        private string InputLastID;

        private void Initialize_ListFile(List<StalkerClass.Xml.Xml_Text_File> fls)
        {
            XmlFiles = fls;
            list_files.Items.Clear();
            foreach(var fileItem in fls)
            {
                list_files.Items.Add(fileItem._File.Name);
            }
        }

        private void list_files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(list_files.SelectedIndex != -1)
            {
                InputXml = XmlFiles[list_files.SelectedIndex];
                list_id.Items.Clear();
                foreach(var Items in InputXml.ExpressionsBlocks)
                {
                    list_id.Items.Add(Items.Id);
                }
            }
        }

        private void list_id_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(list_id.SelectedIndex != -1 && InputXml != null)
            {
                InputLastID = InputXml.ExpressionsBlocks[list_id.SelectedIndex].Id;
                txt_id.Text = InputXml.ExpressionsBlocks[list_id.SelectedIndex].Id;
                txt_text.Text = InputXml.ExpressionsBlocks.Where(x => x.Id == txt_id.Text).First().Text;

              //  txt_text.Text = InputXml.ExpressionsBlocks[list_id.SelectedIndex].Text;

            }
        }

        private void mn_addFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            file.Filter = "XML|*.xml|All Files|*.*";
            file.InitialDirectory = MainWindow.ProgramData.Gamedata;
            file.Title = "Выберите .Xml файл";
            if(file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StalkerClass.Xml.Xml_Text_File fls = new StalkerClass.Xml.Xml_Text_File(new System.IO.FileInfo(file.FileName), MainWindow.ProgramData.Encoding_XML, true);
                XmlFiles.Add(fls);
                Initialize_ListFile(XmlFiles);
            }
        }

        private void nm_addID_Click(object sender, RoutedEventArgs e)
        {
            if(InputXml != null && XmlFiles.Where(x => x._File.FullName == InputXml._File.FullName).Count() > 0)
            {
                InputXml.ExpressionsBlocks.Add(new StalkerClass.Xml.Xml_Text_String("NewId", "NewValue"));

                list_id.Items.Add(InputXml.ExpressionsBlocks[InputXml.ExpressionsBlocks.Count - 1].Id);
                list_id.SelectedIndex = list_id.Items.Count - 1;
                list_id.ScrollIntoView(list_id.Items[list_id.Items.Count - 1]);
                
            }
        }

        private void txt_id_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(InputLastID != null && !string.IsNullOrWhiteSpace(txt_id.Text) && false)
            {
                if (InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).Count() > 0)
                {
                    InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).First().Id = txt_id.Text;
                    InputLastID = txt_id.Text;
                    if (InputXml.ExpressionsBlocks.Where(x => x.Id == txt_id.Text).Count() > 0)
                    {
                        int index = InputXml.ExpressionsBlocks.IndexOf(InputXml.ExpressionsBlocks.Where(x => x.Id == txt_id.Text).First());
                        if (index != -1)
                        {
                            list_id.Items[index] = txt_id.Text;

                        }
                    }
                }
            }
        }

        private void txt_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(false && InputXml != null && InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).Count() > 0 && !string.IsNullOrWhiteSpace(txt_text.Text))
            {
                Console.WriteLine(InputLastID);
                InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).First().Text = txt_text.Text;
            }
        }

        private void txt_id_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (InputLastID != null && !string.IsNullOrWhiteSpace(txt_id.Text))
                {
                    if (InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).Count() > 0)
                    {
                        InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).First().Id = txt_id.Text;
                        InputLastID = txt_id.Text;
                        if (InputXml.ExpressionsBlocks.Where(x => x.Id == txt_id.Text).Count() > 0)
                        {
                            int index = InputXml.ExpressionsBlocks.IndexOf(InputXml.ExpressionsBlocks.Where(x => x.Id == txt_id.Text).First());
                            if (index != -1)
                            {
                                list_id.Items[index] = txt_id.Text;

                            }
                        }
                    }
                }
            }
        }

        private void txt_text_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (InputXml != null && InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).Count() > 0 && !string.IsNullOrWhiteSpace(txt_text.Text))
                {
                    InputXml.ExpressionsBlocks.Where(x => x.Id == InputLastID).First().Text = txt_text.Text;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if(list_files.SelectedIndex != -1 && InputXml != null)
            {
                try
                {
                    System.IO.File.WriteAllText(InputXml._File.FullName, InputXml.ToString(), MainWindow.ProgramData.Encoding_XML);
                    MessageBox.Show($"Файл \"{InputXml._File.Name}\" успешно сохранён!", "Сохранение успешно!", MessageBoxButton.OK, MessageBoxImage.Information); 
                }
                catch(Exception g)
                {
                    MessageBox.Show(g.Message, "Ошибка сохранения xml файла!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void nm_delId_Click(object sender, RoutedEventArgs e)
        {
            if(list_id.SelectedIndex != -1)
            {
                InputXml.ExpressionsBlocks.RemoveAt(list_id.SelectedIndex);
                list_id.Items.RemoveAt(list_id.SelectedIndex);
                InputLastID = null;
                txt_id.Text = "";
                txt_text.Text = "";

            }
        }
    }
}
