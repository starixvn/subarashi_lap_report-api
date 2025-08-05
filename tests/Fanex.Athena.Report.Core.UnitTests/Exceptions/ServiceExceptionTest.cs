using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Fanex.Athena.Report.Core.Exceptions;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Exceptions
{
    public class ServiceExceptionTest
    {
        [Fact]
        public void Constructor_WithMessageAndServiceName_SetsMessageCorrectly()
        {
            // Arrange
            const string serviceName = "TestService";
            const string message = "Test Message";
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_WithMessageServiceNameAndInnerException_SetsAllPropertiesCorrectly()
        {
            // Arrange
            const string serviceName = "TestService";
            const string message = "Test Message";
            var innerException = new ArgumentException("Inner exception");
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName, innerException);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_WithNullServiceName_HandlesGracefully()
        {
            // Arrange
            const string message = "Test Message";
            const string serviceName = null;
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyServiceName_HandlesGracefully()
        {
            // Arrange
            const string message = "Test Message";
            const string serviceName = "";
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        // [Fact]
        // public void Constructor_WithNullMessage_HandlesGracefully()
        // {
        //     // Arrange
        //     const string serviceName = "TestService";
        //     const string message = null;
        //     var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

        //     // Act
        //     var exception = new ServiceException(message, serviceName);

        //     // Assert
        //     Assert.Equal(expectedMessage, exception.Message);
        // }

        [Fact]
        public void Constructor_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            const string serviceName = "Service@#$%";
            const string message = "Message with special chars: !@#$%^&*()";
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_WithVeryLongStrings_HandlesCorrectly()
        {
            // Arrange
            var serviceName = new string('S', 1000);
            var message = new string('M', 1000);
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void InnerException_WithInnerException_ReturnsCorrectException()
        {
            // Arrange
            const string serviceName = "TestService";
            const string message = "Test Message";
            var innerException = new InvalidOperationException("Inner message");

            // Act
            var exception = new ServiceException(message, serviceName, innerException);

            // Assert
            Assert.Same(innerException, exception.InnerException);
            Assert.Equal("Inner message", exception.InnerException.Message);
        }

        [Fact]
        public void InnerException_WithoutInnerException_ReturnsNull()
        {
            // Arrange
            const string serviceName = "TestService";
            const string message = "Test Message";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void GetObjectData_SerializesServiceNameCorrectly()
        {
            // Arrange
            const string serviceName = "TestService";
            const string message = "Test Message";
            var exception = new ServiceException(message, serviceName);
            var serializationInfo = new SerializationInfo(typeof(ServiceException), new FormatterConverter());
            var streamingContext = new StreamingContext();

            // Act
            exception.GetObjectData(serializationInfo, streamingContext);

            // Assert
            Assert.Equal(serviceName, serializationInfo.GetString("_serviceName"));
        }

        [Fact]
        public void GetObjectData_WithNullServiceName_SerializesCorrectly()
        {
            // Arrange
            const string serviceName = null;
            const string message = "Test Message";
            var exception = new ServiceException(message, serviceName);
            var serializationInfo = new SerializationInfo(typeof(ServiceException), new FormatterConverter());
            var streamingContext = new StreamingContext();

            // Act
            exception.GetObjectData(serializationInfo, streamingContext);

            // Assert
            Assert.Null(serializationInfo.GetString("_serviceName"));
        }

        [Fact]
        public void Exception_IsSerializable()
        {
            // Arrange & Act
            var exceptionType = typeof(ServiceException);

            // Assert
            Assert.True(exceptionType.IsSerializable);
        }

        [Fact]
        public void Message_WhenCall_ReturnCorrectValue()
        {
            // Arrange
            const string ServiceName = "ServiceName";
            const string Message = "Test Message";

            var serviceException = new ServiceException(Message, ServiceName);
            var expectedMessage = $"Exception when call Service: {ServiceName}. Exception Message: {Message}";

            // Action
            var result = serviceException.Message;

            // Assert
            Assert.Equal(expectedMessage, result);
        }

        [Theory]
        [InlineData("UserService", "User not found")]
        [InlineData("PaymentService", "Payment failed")]
        [InlineData("AuthService", "Authentication error")]
        public void Constructor_WithDifferentServices_CreatesCorrectMessage(string serviceName, string message)
        {
            // Arrange
            var expectedMessage = $"Exception when call Service: {serviceName}. Exception Message: {message}";

            // Act
            var exception = new ServiceException(message, serviceName);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_WithNestedInnerExceptions_MaintainsExceptionChain()
        {
            // Arrange
            var deepestException = new ArgumentException("Deepest exception");
            var middleException = new InvalidOperationException("Middle exception", deepestException);
            var exception = new ServiceException("Service exception", "TestService", middleException);

            // Act & Assert
            Assert.Same(middleException, exception.InnerException);
            Assert.Same(deepestException, exception.InnerException.InnerException);
            Assert.Equal("Middle exception", exception.InnerException.Message);
            Assert.Equal("Deepest exception", exception.InnerException.InnerException.Message);
        }
    }
}
