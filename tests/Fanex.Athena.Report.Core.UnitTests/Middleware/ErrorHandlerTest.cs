using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.Core.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Middleware
{
    public class ErrorHandlerTest
    {
        private Fixture _fixture = new();
        private Mock<ILogger<ErrorHandler>> _logger;
        private Mock<RequestDelegate> _next;

        public ErrorHandlerTest()
        {
            _next = new Mock<RequestDelegate>();
            _logger = new Mock<ILogger<ErrorHandler>>();
        }

        [Fact]
        public async Task Invoke_WhenCall_InvokeNext()
        {
            // Arrange
            var ctx = new DefaultHttpContext();

            var handler = new ErrorHandler(_next.Object, _logger.Object);

            // Act
            await handler.Invoke(ctx);

            // Assert
            _next.Verify(x => x.Invoke(ctx));
        }

        [Fact]
        public async Task Invoke_ContextError_InternalServerError()
        {
            // Arrange
            var error = _fixture.Create<ExceptionHandlerFeature>();
            var ctx = new DefaultHttpContext()
            {
                Features = { [typeof(IExceptionHandlerFeature)] = error }
            };


            var handler = new ErrorHandler(_next.Object, _logger.Object);

            // Act
            await handler.Invoke(ctx);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ctx.Response.StatusCode);
            Assert.Equal("application/json", ctx.Response.ContentType);
            Assert.NotNull(ctx.Response.Body);
        }

        [Fact]
        public async Task Invoke_ThrowException_HandleException()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            var ctx = new DefaultHttpContext();

            _next.Setup(x => x.Invoke(ctx)).ThrowsAsync(exception);


            var handler = new ErrorHandler(_next.Object, _logger.Object);


            // Act
            await handler.Invoke(ctx);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ctx.Response.StatusCode);
            Assert.Equal("application/json", ctx.Response.ContentType);
            Assert.NotNull(ctx.Response.Body);
            _logger.Verify(
                          x => x.Log(
                                     It.Is<LogLevel>(l => l == LogLevel.Critical),
                                     It.IsAny<EventId>(),
                                     It.Is<It.IsAnyType>((v, t) => true),
                                     exception,
                                     It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
