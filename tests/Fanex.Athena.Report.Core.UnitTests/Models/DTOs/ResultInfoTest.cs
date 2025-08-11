namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    using Core.Models.DTOs;
    using Xunit;

    public class ResultInfoTest
    {
        [Fact]
        public void IsSuccessful_WithEmptyMessage_ReturnsTrue()
        {
            // Arrange
            var resultInfo = new ResultInfo { Message = "", Code = 1 };

            // Act
            var result = resultInfo.IsSuccessful;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSuccessful_WithNullMessage_ReturnsTrue()
        {
            // Arrange
            var resultInfo = new ResultInfo { Message = null, Code = 1 };

            // Act
            var result = resultInfo.IsSuccessful;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSuccessful_WithCodeZero_ReturnsTrue()
        {
            // Arrange
            var resultInfo = new ResultInfo { Message = "Some error", Code = 0 };

            // Act
            var result = resultInfo.IsSuccessful;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSuccessful_WithNonZeroCodeAndMessage_ReturnsFalse()
        {
            // Arrange
            var resultInfo = new ResultInfo { Message = "Error occurred", Code = 1 };

            // Act
            var result = resultInfo.IsSuccessful;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSuccessful_WithEmptyMessageAndCodeZero_ReturnsTrue()
        {
            // Arrange
            var resultInfo = new ResultInfo { Message = "", Code = 0 };

            // Act
            var result = resultInfo.IsSuccessful;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Code_GetSet_WorksCorrectly()
        {
            // Arrange
            var resultInfo = new ResultInfo();
            const int expectedCode = 123;

            // Act
            resultInfo.Code = expectedCode;

            // Assert
            Assert.Equal(expectedCode, resultInfo.Code);
        }

        [Fact]
        public void Message_GetSet_WorksCorrectly()
        {
            // Arrange
            var resultInfo = new ResultInfo();
            const string expectedMessage = "Test message";

            // Act
            resultInfo.Message = expectedMessage;

            // Assert
            Assert.Equal(expectedMessage, resultInfo.Message);
        }

        [Fact]
        public void ExtendedData_GetSet_WorksCorrectly()
        {
            // Arrange
            var resultInfo = new ResultInfo();
            const string expectedData = "Extended data content";

            // Act
            resultInfo.ExtendedData = expectedData;

            // Assert
            Assert.Equal(expectedData, resultInfo.ExtendedData);
        }

        [Fact]
        public void ResultInfo_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var resultInfo = new ResultInfo();

            // Assert
            Assert.Equal(0, resultInfo.Code);
            Assert.Null(resultInfo.Message);
            Assert.Null(resultInfo.ExtendedData);
            Assert.True(resultInfo.IsSuccessful);
        }

        [Fact]
        public void ResultInfo_IsSerializable()
        {
            // Arrange & Act
            var resultInfoType = typeof(ResultInfo);

            // Assert
            Assert.True(resultInfoType.IsSerializable);
        }

        [Theory]
        [InlineData(null, 0, true)]
        [InlineData("", 0, true)]
        [InlineData("Error", 0, true)]
        [InlineData(null, 1, true)]
        [InlineData("", 1, true)]
        [InlineData("Error", 1, false)]
        [InlineData("Error", -1, false)]
        [InlineData("Error", 100, false)]
        public void IsSuccessful_WithVariousCombinations_ReturnsExpectedResult(string message, int code, bool expected)
        {
            // Arrange
            var resultInfo = new ResultInfo { Message = message, Code = code };

            // Act
            var result = resultInfo.IsSuccessful;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}