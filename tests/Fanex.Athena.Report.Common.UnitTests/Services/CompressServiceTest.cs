namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AutoFixture;
    using Common.Excel;
    using Common.Services;
    using Common.Services.Interfaces;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;
    using Model.MetaModels;
    using Moq;
    using Xunit;

    public class CompressServiceTest : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IWebHostEnvironment> _webHostEnvironment;
        private readonly Mock<IFileService> _fileService;
        private readonly Mock<IOptions<ExcelOptions>> _options;
        private readonly ExcelOptions _excelOptions;
        private readonly string _tempDirectory;
        private readonly List<string> _filesToCleanup = new List<string>();

        public CompressServiceTest()
        {
            _webHostEnvironment = new Mock<IWebHostEnvironment>();
            _fileService = new Mock<IFileService>();
            _options = new Mock<IOptions<ExcelOptions>>();
            _excelOptions = new ExcelOptions { Path = "TestExcelPath" };
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            _options.Setup(x => x.Value).Returns(_excelOptions);
            _webHostEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);
            _fileService.Setup(x => x.GetParentDirectory(It.IsAny<string>())).Returns(new DirectoryInfo(_tempDirectory));

            Directory.CreateDirectory(_tempDirectory);
        }

        [Fact]
        public void CompressDataInByteFormat_CreatesZipAndReturnsBytes()
        {
            // Arrange
            var entries = new List<ZipEntry>
            {
                new ZipEntry { Path = "test1.txt", FileName = "test1.txt" },
                new ZipEntry { Path = "test2.txt", FileName = "test2.txt" }
            };
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
            var zipPath = Path.Combine(_tempDirectory, "test.zip");

            _fileService.Setup(x => x.ReadAllBytes(It.IsAny<string>())).Returns(expectedBytes);

            var service = CreateService();

            // Act
            var result = service.CompressDataInByteFormat(entries, false);

            // Assert
            Assert.Equal(expectedBytes, result);
            _fileService.Verify(x => x.Compress(entries, It.IsAny<string>(), false), Times.Once);
            _fileService.Verify(x => x.ReadAllBytes(It.IsAny<string>()), Times.Once);
            _fileService.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void CompressDataInByteFormat_WithDeleteOriginal_PassesCorrectFlag()
        {
            // Arrange
            var entries = new List<ZipEntry>
            {
                new ZipEntry { Path = "test1.txt", FileName = "test1.txt" }
            };
            var expectedBytes = new byte[] { 1, 2, 3 };

            _fileService.Setup(x => x.ReadAllBytes(It.IsAny<string>())).Returns(expectedBytes);

            var service = CreateService();

            // Act
            var result = service.CompressDataInByteFormat(entries, true);

            // Assert
            _fileService.Verify(x => x.Compress(entries, It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public void CompressDataToFile_CreatesZipAndReturnsPath()
        {
            // Arrange
            var entries = new List<ZipEntry>
            {
                new ZipEntry { Path = "test1.txt", FileName = "test1.txt" }
            };

            var service = CreateService();

            // Act
            var result = service.CompressDataToFile(entries, false);

            // Assert
            Assert.Contains(_tempDirectory, result);
            Assert.EndsWith(".zip", result);
            _fileService.Verify(x => x.Compress(entries, It.IsAny<string>(), false), Times.Once);
        }

        [Fact]
        public void EnsureFolderPath_CreatesDirectoryAndReturnsPath()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.EnsureFolderPath();

            // Assert
            Assert.Contains(_tempDirectory, result);
            Assert.Contains(_excelOptions.Path, result);
            _fileService.Verify(x => x.CreateFolder(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void EnsureFolderPath_CallsGetParentDirectory()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.EnsureFolderPath();

            // Assert
            _fileService.Verify(x => x.GetParentDirectory(_tempDirectory), Times.Once);
        }

        private CompressService CreateService()
        {
            return new CompressService(_webHostEnvironment.Object, _fileService.Object, _options.Object);
        }

        public void Dispose()
        {
            foreach (var file in _filesToCleanup)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
    }
}