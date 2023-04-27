using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Fanex.Athena.Report.Common.Services
{
    public class CompressService : ICompressService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileService _fileService;
        private readonly ExcelOptions _option;

        public CompressService(IWebHostEnvironment environment, IFileService fileService, IOptions<ExcelOptions> options) 
        {
            _webHostEnvironment = environment;
            _fileService = fileService;
            _option = options.Value;
        }

        public byte[] CompressCompressDataInByteFormssData(IEnumerable<ZipEntry> entries, bool isDeleteOriginalAfterCompress)
        { 
            var zipPath = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.zip");
            _fileService.Compress(entries, zipPath, isDeleteOriginalAfterCompress);
            var bytes = _fileService.ReadAllBytes(zipPath);
            _fileService.DeleteFile(zipPath);
            return bytes;
        }

        public string CompressDataToFile(IEnumerable<ZipEntry> entries, bool isDeleteOriginalAfterCompress)
        {
            var zipPath = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.zip");
            _fileService.Compress(entries, zipPath, isDeleteOriginalAfterCompress);
            return zipPath;
        }

        public string EnsureFolderPath()
        {
            var path = GetTempFolderPath();
            _fileService.CreateFolder(path);
            return path;
        }

        private string GetTempFolderPath()
        {
            var root = _fileService.GetParentDirectory(_webHostEnvironment.ContentRootPath);

            return Path.Combine(root.FullName, _option.Path);
        }
    }
}
