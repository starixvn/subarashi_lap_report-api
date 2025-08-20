namespace Fanex.Athena.Report.Core.UnitTests.Utility
{
    using System;
    using System.IO;
    using Core.Utility;
    using Xunit;

    public class FileSystemTest : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testFilePath;
        private readonly FileSystem _fileSystem;

        public FileSystemTest()
        {
            _fileSystem = new FileSystem();
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _testFilePath = Path.Combine(_testDirectory, "test.txt");
        }

        [Fact]
        public void Exists_FileExists_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "test content");

            // Act
            var result = _fileSystem.Exists(_testFilePath);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Exists_FileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

            // Act
            var result = _fileSystem.Exists(nonExistentPath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Exists_NullPath_ReturnsFalse()
        {
            // Act
            var result = _fileSystem.Exists(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Exists_EmptyPath_ReturnsFalse()
        {
            // Act
            var result = _fileSystem.Exists(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ReadFile_FileExists_ReturnsContent()
        {
            // Arrange
            var expectedContent = "This is test content\nWith multiple lines\nAnd special characters: !@#$%";
            File.WriteAllText(_testFilePath, expectedContent);

            // Act
            var result = _fileSystem.ReadFile(_testFilePath);

            // Assert
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public void ReadFile_EmptyFile_ReturnsEmptyString()
        {
            // Arrange
            File.WriteAllText(_testFilePath, string.Empty);

            // Act
            var result = _fileSystem.ReadFile(_testFilePath);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ReadFile_FileDoesNotExist_ThrowsException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => _fileSystem.ReadFile(nonExistentPath));
        }

        [Fact]
        public void ReadFile_NullPath_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _fileSystem.ReadFile(null));
        }

        [Fact]
        public void ReadFile_EmptyPath_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => _fileSystem.ReadFile(string.Empty));
        }

        [Fact]
        public void ReadFile_LargeFile_ReturnsFullContent()
        {
            // Arrange
            var largeContent = new string('A', 10000) + "\n" + new string('B', 10000);
            File.WriteAllText(_testFilePath, largeContent);

            // Act
            var result = _fileSystem.ReadFile(_testFilePath);

            // Assert
            Assert.Equal(largeContent, result);
            Assert.Equal(20001, result.Length); // 10000 + 1 + 10000
        }

        [Fact]
        public void ReadFile_FileWithUnicodeCharacters_ReturnsCorrectContent()
        {
            // Arrange
            var unicodeContent = "Hello 世界 🌍 Ñoël café résumé";
            File.WriteAllText(_testFilePath, unicodeContent);

            // Act
            var result = _fileSystem.ReadFile(_testFilePath);

            // Assert
            Assert.Equal(unicodeContent, result);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
    }
}
