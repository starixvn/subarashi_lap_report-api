using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Middleware
{
    public class ErrorHandlingTest
    {
        private readonly Mock<ILogger<ErrorHandling>> logger;

        private const string ExpectedResponseBody = "Completed";

        private const string ContentType = "application/json";

        private const string ExceptionMessage = "ExceptionMessage";

        public ErrorHandlingTest()
        {
            logger = new Mock<ILogger<ErrorHandling>>();
        }

        [Fact]
        public async Task Invoke_FeatureIsIExceptionHandlerFeature_ResponseHasCorrectContent()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = CreateHttpContext();
            var expectedResponseContent = JsonConvert.SerializeObject(
                    new
                    {
                        State = "Internal Server Error",
                        Msg = ExceptionMessage
                    });

            // Action
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var stream = new StreamReader(context.Response.Body))
            {
                var result = stream.ReadToEnd();

                // Assert
                Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
                Assert.Equal(ContentType, context.Response.ContentType);
                Assert.Equal(expectedResponseContent, result);
            }
        }

        [Fact]
        public async Task Invoke_FeatureIsOthers_ResponseHasCorrectContent()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = CreateHttpContext(hasFeature: false);

            // Action
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var stream = new StreamReader(context.Response.Body))
            {
                var result = stream.ReadToEnd();

                // Assert
                Assert.Equal(ExpectedResponseBody, result);
            }
        }

        [Fact]
        public async Task Invoke_ThrowExceptionWhenCallingNext_ErrorIsLoggedAndResponseHasCorrectContent()
        {
            // Arrange
            var middleware = CreateMiddleware(throwException: true);
            var context = CreateHttpContext(hasFeature: false);
            var expectedResponseContent = JsonConvert.SerializeObject(new
            {
                State = "Internal Server Error",
                Msg = ExceptionMessage,
                Code = HttpStatusCode.InternalServerError
            });

            // Action
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var stream = new StreamReader(context.Response.Body))
            {
                var result = stream.ReadToEnd();

                // Assert
                Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
                Assert.Equal(ContentType, context.Response.ContentType);
                Assert.Equal(expectedResponseContent, result);
                logger.Verify(r =>
                    r.Log(LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                    Times.Once);
            }
        }

        private ErrorHandling CreateMiddleware(bool throwException = false)
        {
            return new ErrorHandling(
                next: (innerHttpContext) =>
                {
                    if (throwException)
                    {
                        throw new Exception(ExceptionMessage);
                    }
                    else
                    {
                        innerHttpContext.Response.WriteAsync(ExpectedResponseBody);
                        return Task.CompletedTask;
                    }
                },
                logger: logger.Object
            );
        }

        private HttpContext CreateHttpContext(bool hasFeature = true)
        {
            var response = Mock.Of<HttpResponse>(r => r.Body == new MemoryStream());
            var context = Mock.Of<HttpContext>(ctx => ctx.Response == response && ctx.Features == Mock.Of<IFeatureCollection>());

            if (hasFeature)
            {
                var exception = Mock.Of<IExceptionHandlerFeature>(e => e.Error == new Exception(ExceptionMessage));
                var features = Mock.Of<IFeatureCollection>(f => f[typeof(IExceptionHandlerFeature)] == exception);
                Mock.Get(context).Setup(x => x.Features).Returns(features);
            }

            return context;
        }
    }
}