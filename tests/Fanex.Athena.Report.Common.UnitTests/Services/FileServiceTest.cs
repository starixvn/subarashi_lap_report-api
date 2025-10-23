using AutoFixture;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    public class FileServiceTest
    {
        private readonly Fixture _fixture;

        public FileServiceTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Compress_WithValidEntries_CreatesZipFile()
        {
            // Arrange
            var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testDir);

            var testFile1 = Path.Combine(testDir, "test1.txt");
            var testFile2 = Path.Combine(testDir, "test2.txt");
            File.WriteAllText(testFile1, "Test content 1");
            File.WriteAllText(testFile2, "Test content 2");

            var entries = new List<ZipEntry>
            {
                new ZipEntry { Path = testFile1, FileName = "test1.txt" },
                new ZipEntry { Path = testFile2, FileName = "test2.txt" }
            };
            var zipPath = Path.Combine(testDir, "test.zip");

            try
            {
                var service = new FileService();

                // Act
                service.Compress(entries, zipPath, false);

                // Assert
                Assert.True(File.Exists(zipPath));
            }
            finally
            {
                if (Directory.Exists(testDir))
                    Directory.Delete(testDir, true);
            }
        }

        [Fact]
        public void ReadAllBytes_WithValidPath_ReturnsFileBytes()
        {
            // Arrange
            var testPath = Path.GetTempFileName();
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };

            try
            {
                File.WriteAllBytes(testPath, expectedBytes);
                var service = new FileService();

                // Act
                var result = service.ReadAllBytes(testPath);

                // Assert
                Assert.Equal(expectedBytes, result);
            }
            finally
            {
                if (File.Exists(testPath))
                    File.Delete(testPath);
            }
        }

        [Fact]
        public void DeleteFile_WithValidPath_DeletesFile()
        {
            // Arrange
            var testPath = Path.GetTempFileName();
            File.WriteAllBytes(testPath, new byte[] { 1, 2, 3 });
            var service = new FileService();

            // Act
            service.DeleteFile(testPath);

            // Assert
            Assert.False(File.Exists(testPath));
        }

        [Fact]
        public void DeleteFile_WithNonExistentPath_DoesNotThrow()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "non_existent_file.txt");
            var service = new FileService();

            // Act & Assert
            service.DeleteFile(nonExistentPath); // Should not throw
        }
    }
}