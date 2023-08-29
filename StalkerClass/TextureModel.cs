using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Stalker_Studio.StalkerClass
{
    internal class TextureModel : Common.FileModel
    {
        DDSImage _image = null;

        public TextureModel() { }

        public TextureModel(string fullName) : base(fullName)
        {
            if (!Path.GetExtension(fullName).ToLower().Contains("dds"))
                throw new Exception("Неверное расширение файла");
        }

        public TextureModel(FileInfo file) : base(file)
        {

            if (!file.Extension.ToLower().Contains("dds"))
                throw new Exception("Неверное расширение файла");
        }

        public System.Drawing.Bitmap Bitmap 
        {
            get 
            {
                if (_image == null)
                    _image = new DDSImage(File.ReadAllBytes(_fullName));
                if(!_image.IsValid)
                    ViewModel.Workspace.This.MessageList.Add(this, "Не удалось открыть текстуру .dds!", ViewModel.ErrorCategory.Error, this);
                return _image.BitmapImage;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _image = null;
            OnPropertyChanged(nameof(Bitmap));
        }

        protected override void OnSave()
        {
            // УБРАТЬ ОТСЮДА

            System.Windows.Forms.SaveFileDialog saveF = new System.Windows.Forms.SaveFileDialog();

            saveF.Title = "Сохранить как .png";
            saveF.Filter = "PNG|*.png";
            if (saveF.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string output = saveF.FileName;

                Bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
