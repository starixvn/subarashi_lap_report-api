namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Fanex.Athena.Report.Common.Model.MetaModels;

    public interface IFileService
    {
        DirectoryInfo GetParentDirectory(string path);

        void DeleteFile(string path);

        void CreateFolder(string path);

        StreamWriter CreateStreamWriter(string path, bool append, Encoding encoding);

        void Compress(IEnumerable<ZipEntry> entries, string zipPath, bool deleteOriginalFiles);

        void WriteAllBytes(string path, byte[] bytes);

        byte[] ReadAllBytes(string path);
    }
}
