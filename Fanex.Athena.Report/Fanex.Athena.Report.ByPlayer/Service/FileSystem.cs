using System.IO;

namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public class FileSystem : IFileSystem
    {
        public void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public byte[] ReadAllBytes(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            return new byte[0];
        }

        public string ReadString(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        public void Save(string path, string content)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
            }
        }
    }
}