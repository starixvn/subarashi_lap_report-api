using System.IO;
using System.Net;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.DTO;
using Fanex.Athena.Report.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Middleware
{
    public class CustomApiAuthenticationTest
    {
        private readonly Mock<IOptions<ApplicationOptions>> applicationOptionAccessor;

        private const string ExpectedResponseBody = "Completed";

        public CustomApiAuthenticationTest()
        {
            applicationOptionAccessor = new Mock<IOptions<ApplicationOptions>>();
            applicationOptionAccessor.Setup(app => app.Value).Returns(new ApplicationOptions
            {
                AuthKey = "AuthKey"
            });
        }

        [Fact]
        public async Task Invoke_WhenCallWithAuthorized_ResponseHasCorrectContent()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = CreateHttpContext();

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
        public async Task Invoke_NotAuthorized_ResponseHasCorrectContent()
        {
            // Arrange
            var middleware = CreateMiddleware();
            const string WrongAuthKey = "WrongAuthKey";
            var context = CreateHttpContext(WrongAuthKey);

            // Action
            await middleware.Invoke(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        }

        [Fact]
        public async Task Invoke_PathIsIgnored_ResponseHasCorrectContent()
        {
            // Arrange
            var middleware = CreateMiddleware();
            const string Path = "/favicon.ico";
            var context = CreateHttpContext(requestPath: Path);

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

        private CustomApiAuthentication CreateMiddleware()
        {
            return new CustomApiAuthentication(
                next: (innerHttpContext) =>
                {
                    innerHttpContext.Response.WriteAsync(ExpectedResponseBody);
                    return Task.CompletedTask;
                },
                applicationOptionAccessor: applicationOptionAccessor.Object
            );
        }

        private HttpContext CreateHttpContext(string authKey = "AuthKey", string requestPath = "/")
        {
            var header = Mock.Of<IHeaderDictionary>(x => x["AUTH_TOKEN"] == new StringValues(authKey));
            var request = Mock.Of<HttpRequest>(r =>
                r.Headers == header &&
                r.Path == new PathString(requestPath));
            var response = Mock.Of<HttpResponse>(r => r.Body == new MemoryStream());
            var context = Mock.Of<HttpContext>(ctx => ctx.Request == request && ctx.Response == response);

            return context;
        }
    }
}