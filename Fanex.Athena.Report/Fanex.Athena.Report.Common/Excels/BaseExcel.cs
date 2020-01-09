using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Fanex.Athena.Report.Common.Excels
{
    public class BaseExcel
    {
        protected readonly IHostingEnvironment Enviroment;
        protected readonly ExcelOptions ExcelOptions;

        protected readonly IFileService FileService;

        protected BaseExcel(IHostingEnvironment environment, IFileService fileService, IOptions<ExcelOptions> excelOptionsAccessor)
        {
            Enviroment = environment;
            FileService = fileService;
            ExcelOptions = excelOptionsAccessor.Value;
        }

        protected string EnsureFolderPath()
        {
            var path = GetTempFolderPath();
            FileService.CreateFolder(path);
            return path;
        }

        private string GetTempFolderPath()
        {
            var root = FileService.GetParentDirectory(Enviroment.ContentRootPath);

            return Path.Combine(root.FullName, ExcelOptions.Path);
        }

        protected string ZipExcelFile(List<ZipEntry> files, string tempFolderPath)
        {
            var zipPath = Path.Combine(tempFolderPath, $"{Guid.NewGuid()}.zip");

            FileService.Compress(files, zipPath, true);

            return zipPath;
        }

        protected void BuildItemWithTextDelimiter(StringBuilder rowBuilder, object value)
        {
            rowBuilder.Append(ExcelOptions.CSVTextDelimiter)
            .Append(value)
            .Append(ExcelOptions.CSVTextDelimiter)
            .Append(ExcelOptions.CSVSeparateCharacter);
        }

        protected void BuildItemAsText(StringBuilder rowBuilder, object value)
        {
            rowBuilder.Append("=");
            BuildItemWithTextDelimiter(rowBuilder, value);
        }
    }
}