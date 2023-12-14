using System.IO;
using Fanex.Athena.Report.Core.Utility.Interfaces;

namespace Fanex.Athena.Report.Core.Utility
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadFile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}