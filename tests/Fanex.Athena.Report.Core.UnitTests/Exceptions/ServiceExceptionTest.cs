using Fanex.Athena.Report.Core.Exceptions;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Exceptions
{
    public class ServiceExceptionTest
    {
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
    }
}
