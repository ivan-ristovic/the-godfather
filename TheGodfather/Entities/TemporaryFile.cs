using System;
using System.IO;

namespace TheGodfather.Entities
{
    public sealed class TemporaryFile : IDisposable
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string DirPath { get; } = "Temp/";
        public string FullPath => DirPath + FileName + Extension;


        public TemporaryFile(string extension, string filename = null)
        {
            if (string.IsNullOrWhiteSpace(filename))
                filename = DateTime.Now.Ticks.ToString();
            FileName = filename;
            Extension = extension ?? "";
        }


        public void Save(Action saveAction)
        {
            if (!Directory.Exists(DirPath))
                Directory.CreateDirectory(DirPath);
            saveAction();
        }

        public FileStream OpenFileStream()
        {
            if (File.Exists(FullPath))
                return new FileStream(FullPath, FileMode.Open);
            else
                return null;
        }

        public void Dispose()
        {
            if (File.Exists(FullPath))
                File.Delete(FullPath);
        }
    }
}
