namespace Fanex.Athena.Report.Common.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Fanex.Athena.Report.Common.Model.MetaModels;

    public class FileService : IFileService
    {
        public DirectoryInfo GetParentDirectory(string path)
        {
            return Directory.GetParent(path);
        }

        public void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public StreamWriter CreateStreamWriter(string path, bool append, Encoding encoding)
        {
            return new StreamWriter(path, append, encoding);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void Compress(IEnumerable<ZipEntry> entries, string zipPath, bool deleteOriginalFiles)
        {
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var entry in entries)
                {
                    archive.CreateEntryFromFile(entry.Path, entry.FileName);
                    if (deleteOriginalFiles)
                    {
                        File.Delete(entry.Path);
                    }
                }
            }
        }
    }
}
