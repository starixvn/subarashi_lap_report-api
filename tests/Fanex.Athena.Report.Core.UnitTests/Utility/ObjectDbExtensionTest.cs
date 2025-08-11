namespace Fanex.Athena.Report.Core.UnitTests.Utility
{
    using Core.Utility;
    using Data;
    using Moq;
    using Xunit;

    public class ObjectDbExtensionTest
    {
        private readonly Mock<IObjectDb> _mockObjectDb;

        public ObjectDbExtensionTest()
        {
            _mockObjectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public void GetResult_WithValidParameters_ReturnsResultInfo()
        {
            // Arrange
            var expectedCode = 0;
            var expectedMessage = "Success";
            
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(expectedCode);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns(expectedMessage);

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCode, result.Code);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public void GetResult_WithNullErrorCode_ReturnsResultWithZeroCode()
        {
            // Arrange
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(null);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns("Error message");

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Code);
        }

        [Fact]
        public void GetResult_WithNullErrorMessage_ReturnsResultWithEmptyMessage()
        {
            // Arrange
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(1);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns(null);

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void GetResult_WithErrorCode_ReturnsFailedResult()
        {
            // Arrange
            var errorCode = 1001;
            var errorMessage = "Database connection failed";
            
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(errorCode);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns(errorMessage);

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(errorCode, result.Code);
            Assert.Equal(errorMessage, result.Message);
            Assert.False(result.IsSuccessful);
        }

        [Fact]
        public void GetResult_WithZeroErrorCode_ReturnsSuccessfulResult()
        {
            // Arrange
            var errorCode = 0;
            var errorMessage = "Operation completed successfully";
            
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(errorCode);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns(errorMessage);

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(errorCode, result.Code);
            Assert.Equal(errorMessage, result.Message);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void AddResultParam_CallsSetParameterValue_ForBothParameters()
        {
            // Act
            _mockObjectDb.Object.AddResultParam();

            // Assert
            _mockObjectDb.Verify(x => x.SetParameterValue("ErrCode", null), Times.Once);
            _mockObjectDb.Verify(x => x.SetParameterValue("ErrMsg", null), Times.Once);
        }

        [Theory]
        [InlineData(0, "Success")]
        [InlineData(404, "Not Found")]
        [InlineData(500, "Internal Server Error")]
        [InlineData(-1, "Unknown Error")]
        public void GetResult_WithVariousErrorCodes_ReturnsExpectedResult(int errorCode, string errorMessage)
        {
            // Arrange
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(errorCode);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns(errorMessage);

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(errorCode, result.Code);
            Assert.Equal(errorMessage, result.Message);
        }

        [Fact]
        public void GetResult_WithStringErrorCode_ConvertsToInteger()
        {
            // Arrange
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns("404");
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns("Not Found");

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.Code);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public void GetResult_WithInvalidStringErrorCode_ConvertsToZero()
        {
            // Arrange
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns("invalid");
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns("Invalid error code");

            // Act
            var result = _mockObjectDb.Object.GetResult();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Code);
            Assert.Equal("Invalid error code", result.Message);
        }

        [Fact]
        public void GetResult_VerifiesParameterAccess()
        {
            // Arrange
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrCode")).Returns(0);
            _mockObjectDb.Setup(x => x.GetParameterValue("ErrMsg")).Returns("Success");

            // Act
            _mockObjectDb.Object.GetResult();

            // Assert
            _mockObjectDb.Verify(x => x.GetParameterValue("ErrCode"), Times.Once);
            _mockObjectDb.Verify(x => x.GetParameterValue("ErrMsg"), Times.Once);
        }
    }
}