using Fanex.Athena.Report.ByPlayer.DTO;
using Microsoft.Extensions.Options;
using System.IO;

namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public class TempFileService : ITempFileService
    {
        private readonly ExcelFileOptions _excelFileOptions;

        private readonly IFileSystem _fileSystem;

        public TempFileService(IFileSystem fileSystem, IOptions<ExcelFileOptions> excelFileOptions)
        {
            _fileSystem = fileSystem;
            _excelFileOptions = excelFileOptions.Value;
        }

        public void Delete(string path)
        {
            try
            {
                var realPath = Path.Combine(_excelFileOptions.WebAppFileLAP, path);

                _fileSystem.Delete(realPath);
            }
            catch
            {
                // Ignore it
            }
        }

        public string ReadString(string path)
        {
            var realPath = Path.Combine(_excelFileOptions.WebAppFileLAP, path);
            return _fileSystem.ReadString(realPath);
        }

        public void Save(string path, string content)
        {
            var realPath = Path.Combine(_excelFileOptions.WebAppFileLAP, path);
            _fileSystem.Save(realPath, content);
        }

        public byte[] ReadBytes(string path)
        {
            var realPath = Path.Combine(_excelFileOptions.WebAppFileLAP, path);
            return _fileSystem.ReadAllBytes(realPath);
        }
    }
}