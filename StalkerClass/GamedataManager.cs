using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Stalker_Studio.Common;
using System.IO;

namespace Stalker_Studio.StalkerClass
{
    public class GamedataManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Возможные расширения файлов
        /// </summary>
        public static readonly string[] FileExtentions = new string[]
        {
            "ltx", "xml", "script","txt", "ini", "bat", "cmd", "ogf", "dds"
        };

        /// <summary>
        /// Создает объект по пути к файлу, соответствующий расширению
        /// </summary>
        public static FileModel CreateFileSystemNodeFromExtension(string fullName, string extension = "") 
        {
            string _extension;
            if (extension == "" || extension == null)
            {
                int index = fullName.LastIndexOf('.');
                if (index == -1)
                    _extension = "";
                else
                    _extension = fullName.Substring(index + 1);
            }
            else
                _extension = extension.ToLower().Replace(".", "");

            if (_extension == "ltx")
                return new LtxModel(fullName);
            else if (_extension == "xml")
                return new TextFileModel(fullName);
            else if (_extension == "script")
                return new LuaModel(fullName);
            else if (_extension == "txt")
                return new TextFileModel(fullName);
            else if (_extension == "ini")
                return new TextFileModel(fullName);
            else if (_extension == "ogf")
                return new OGFFile(fullName);
            else if (_extension == "dds")
                return new TextureModel(fullName);
            return new FileModel(fullName);
        }
        /// <summary>
        /// Создает объект соответствующий расширению файла
        /// </summary>
        public static FileModel CreateFileSystemNodeFromExtension(FileInfo info)
        {
            string _extension = info.Extension.ToLower().Replace(".", "");

            if (_extension == "ltx")
                return new LtxModel(info);
            else if (_extension == "xml")
                return new TextFileModel(info);
            else if (_extension == "script")
                return new LuaModel(info);
            else if (_extension == "txt")
                return new TextFileModel(info);
            else if (_extension == "ini")
                return new TextFileModel(info);
            else if (_extension == "bat")
                return new TextFileModel(info);
            else if (_extension == "cmd")
                return new TextFileModel(info);
            else if (_extension == "ogf")
                return new OGFFile(info);
            else if (_extension == "dds")
                return new TextureModel(info);
            return new FileModel(info);
        }

        public static GamedataManager This => _this;

        private static GamedataManager _this = new GamedataManager();
        private DirectoryModel _root = null;

        public GamedataManager() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public DirectoryModel Root
        {
            get { return _root; }
            set {
                FileSystemInfo info = value.Info;
                if (!info.Exists || !info.Attributes.HasFlag(FileAttributes.Directory))
                    throw new System.IO.IOException($"Дериктория не найдена { value.FullName } <{ this.GetType().FullName }>");
                _root = value;
                OnPropertyChanged();
            }
        }
        public void SetRootAtPath(string path) 
        {
            Root = new DirectoryModel(path);
        }
        public FileModel GetFileAtPath(string path)
        {
            if (Root == null)
                return default;
            var nodes = Root.FindNodes(
                (IHierarchical x)=>
                {
                    if (x is FileModel)
                        return (x as FileModel).FullName == path;
                    else
                        return false;
                }, 
                true);
            return nodes.FirstOrDefault() as FileModel;
        }
    }
}
