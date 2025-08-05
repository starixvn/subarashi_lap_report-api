using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AutoFixture;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Common.Services;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    public class FileServiceTest : IDisposable
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly string _tempDirectory;
        private readonly List<string> _filesToCleanup = new List<string>();
        private readonly List<string> _directoriesToCleanup = new List<string>();

        public FileServiceTest()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            _directoriesToCleanup.Add(_tempDirectory);
        }

        [Fact]
        public void GetParentDirectory_WithValidPath_ReturnsParentDirectory()
        {
            // Arrange
            var childPath = Path.Combine(_tempDirectory, "child");
            Directory.CreateDirectory(childPath);
            _directoriesToCleanup.Add(childPath);

            var service = new FileService();

            // Act
            var result = service.GetParentDirectory(childPath);

            // Assert
            Assert.Equal(_tempDirectory, result.FullName);
        }

        [Fact]
        public void CreateFolder_WithNonExistentPath_CreatesDirectory()
        {
            // Arrange
            var newFolderPath = Path.Combine(_tempDirectory, "newfolder");
            var service = new FileService();

            // Act
            service.CreateFolder(newFolderPath);

            // Assert
            Assert.True(Directory.Exists(newFolderPath));
            _directoriesToCleanup.Add(newFolderPath);
        }

        [Fact]
        public void CreateFolder_WithExistingPath_DoesNotThrow()
        {
            // Arrange
            var existingFolderPath = Path.Combine(_tempDirectory, "existing");
            Directory.CreateDirectory(existingFolderPath);
            _directoriesToCleanup.Add(existingFolderPath);

            var service = new FileService();

            // Act & Assert - Should not throw
            service.CreateFolder(existingFolderPath);
            Assert.True(Directory.Exists(existingFolderPath));
        }

        [Fact]
        public void CreateStreamWriter_WithValidParameters_ReturnsStreamWriter()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "test.txt");
            var service = new FileService();

            // Act
            using var writer = service.CreateStreamWriter(filePath, false, Encoding.UTF8);

            // Assert
            Assert.NotNull(writer);
            _filesToCleanup.Add(filePath);
        }

        [Fact]
        public void DeleteFile_WithExistingFile_DeletesFile()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "todelete.txt");
            File.WriteAllText(filePath, "test content");
            var service = new FileService();

            // Act
            service.DeleteFile(filePath);

            // Assert
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public void WriteAllBytes_WithValidData_WritesFile()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "bytes.bin");
            var testBytes = _fixture.Create<byte[]>();
            var service = new FileService();

            // Act
            service.WriteAllBytes(filePath, testBytes);

            // Assert
            Assert.True(File.Exists(filePath));
            var writtenBytes = File.ReadAllBytes(filePath);
            Assert.Equal(testBytes, writtenBytes);
            _filesToCleanup.Add(filePath);
        }

        [Fact]
        public void ReadAllBytes_WithExistingFile_ReturnsFileContent()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "read.bin");
            var testBytes = _fixture.Create<byte[]>();
            File.WriteAllBytes(filePath, testBytes);
            _filesToCleanup.Add(filePath);

            var service = new FileService();

            // Act
            var result = service.ReadAllBytes(filePath);

            // Assert
            Assert.Equal(testBytes, result);
        }

        [Fact]
        public void Compress_WithValidEntries_CreatesZipFile()
        {
            // Arrange
            var file1Path = Path.Combine(_tempDirectory, "file1.txt");
            var file2Path = Path.Combine(_tempDirectory, "file2.txt");
            var zipPath = Path.Combine(_tempDirectory, "test.zip");

            File.WriteAllText(file1Path, "Content 1");
            File.WriteAllText(file2Path, "Content 2");

            var entries = new List<ZipEntry>
            {
                new ZipEntry { Path = file1Path, FileName = "file1.txt" },
                new ZipEntry { Path = file2Path, FileName = "file2.txt" }
            };

            _filesToCleanup.AddRange(new[] { file1Path, file2Path, zipPath });

            var service = new FileService();

            // Act
            service.Compress(entries, zipPath, false);

            // Assert
            Assert.True(File.Exists(zipPath));
            Assert.True(File.Exists(file1Path)); // Should not be deleted
            Assert.True(File.Exists(file2Path)); // Should not be deleted

            // Verify zip contents
            using var archive = ZipFile.OpenRead(zipPath);
            Assert.Equal(2, archive.Entries.Count);
            Assert.Contains(archive.Entries, e => e.Name == "file1.txt");
            Assert.Contains(archive.Entries, e => e.Name == "file2.txt");
        }

        [Fact]
        public void Compress_WithDeleteOriginalFiles_DeletesSourceFiles()
        {
            // Arrange
            var file1Path = Path.Combine(_tempDirectory, "deleteme1.txt");
            var file2Path = Path.Combine(_tempDirectory, "deleteme2.txt");
            var zipPath = Path.Combine(_tempDirectory, "delete_test.zip");

            File.WriteAllText(file1Path, "Content 1");
            File.WriteAllText(file2Path, "Content 2");

            var entries = new List<ZipEntry>
            {
                new ZipEntry { Path = file1Path, FileName = "deleteme1.txt" },
                new ZipEntry { Path = file2Path, FileName = "deleteme2.txt" }
            };

            _filesToCleanup.Add(zipPath);

            var service = new FileService();

            // Act
            service.Compress(entries, zipPath, true);

            // Assert
            Assert.True(File.Exists(zipPath));
            Assert.False(File.Exists(file1Path)); // Should be deleted
            Assert.False(File.Exists(file2Path)); // Should be deleted
        }

        [Fact]
        public void Compress_WithEmptyEntries_CreatesEmptyZip()
        {
            // Arrange
            var zipPath = Path.Combine(_tempDirectory, "empty.zip");
            var entries = new List<ZipEntry>();
            _filesToCleanup.Add(zipPath);

            var service = new FileService();

            // Act
            service.Compress(entries, zipPath, false);

            // Assert
            Assert.True(File.Exists(zipPath));
            using var archive = ZipFile.OpenRead(zipPath);
            Assert.Empty(archive.Entries);
        }

        [Fact]
        public void GetParentDirectory_WithNullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new FileService();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.GetParentDirectory(null));
        }

        [Fact]
        public void CreateFolder_WithInvalidCharacters_ThrowsException()
        {
            // Arrange
            var invalidPath = "invalid\0path"; // null character is invalid on both Windows and Linux
            var service = new FileService();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => service.CreateFolder(invalidPath));
        }

        [Fact]
        public void ReadAllBytes_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.bin");
            var service = new FileService();

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => service.ReadAllBytes(nonExistentPath));
        }

        [Fact]
        public void DeleteFile_WithNonExistentFile_DoesNotThrow()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.txt");
            var service = new FileService();

            // Act
            service.DeleteFile(nonExistentPath);

            // Assert
            Assert.False(File.Exists(nonExistentPath));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up test files
                foreach (var file in _filesToCleanup.Where(File.Exists))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                // Clean up test directories
                foreach (var directory in _directoriesToCleanup.Where(Directory.Exists).OrderByDescending(d => d.Length))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}