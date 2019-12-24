using System;
using System.IO;
using AutoFixture;
using Fanex.Athena.Report.Common.Excels;
using Fanex.Athena.Report.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using OfficeOpenXml;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers
{
    public class BaseReportExcelTest
    {
        protected static readonly IFixture fixture = new Fixture();
        protected readonly string RootPath = Directory.GetCurrentDirectory();

        protected readonly ExcelOptions ExcelOptions = new ExcelOptions()
        {
            Path = "",
            CSVSeparateCharacter = ",",
            MaximumRowPerFile = 10
        };

        protected readonly Mock<IHostingEnvironment> ExcelEnviroment;

        protected Mock<IOptions<ExcelOptions>> ExcelOptionAccessor;

        protected Mock<IFileService> FileService;

        protected BaseReportExcelTest()
        {
            ExcelEnviroment = new Mock<IHostingEnvironment>();
            ExcelEnviroment.Setup(e => e.ContentRootPath).Returns(RootPath);

            ExcelOptionAccessor = CreateMockExcelOption();
            FileService = CreateMockFileService();
        }

        protected Mock<IFileService> CreateMockFileService()
        {
            var fileService = new Mock<IFileService>();
            var directoryInfo = new DirectoryInfo(RootPath);
            fileService.Setup(s => s.GetParentDirectory(RootPath)).Returns(directoryInfo);

            return fileService;
        }

        protected Mock<IOptions<ExcelOptions>> CreateMockExcelOption()
        {
            var excelOptionsAccessor = new Mock<IOptions<ExcelOptions>>();
            excelOptionsAccessor.Setup(eoa => eoa.Value).Returns(ExcelOptions);

            return excelOptionsAccessor;
        }

        protected string CreateFile(string directoryPath = "", string fileExtension = "dat")
        {
            var fileName = Path.Combine(directoryPath, $"{Guid.NewGuid()}.{fileExtension}");
            File.Create(fileName).Dispose();

            return fileName;
        }

        protected byte[] GetStreamContent(Stream fileStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        protected void DeleteFile(params string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        protected string CreateWorkBook()
        {
            var fileTemplate = CreateFile(fileExtension: "xlsx");
            using (var excel = new ExcelPackage())
            {
                excel.Workbook.Worksheets.Add("Template");

                FileInfo excelFile = new FileInfo(fileTemplate);
                excel.SaveAs(excelFile);
            }

            return fileTemplate;
        }
    }
}