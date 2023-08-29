using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.StalkerClass
{
    internal class OGFFile : Common.FileModel
    {
        public OGFFile()
        {
        }

        public OGFFile(string fullName) : base(fullName)
        {
            if (!Path.GetExtension(fullName).ToLower().Contains("ogf"))
                throw new Exception("Неверное расширение файла");
        }

        public OGFFile(FileInfo file) : base(file)
        {

            if (!file.Extension.ToLower().Contains("ogf"))
                throw new Exception("Неверное расширение файла");
        }
    }
}
