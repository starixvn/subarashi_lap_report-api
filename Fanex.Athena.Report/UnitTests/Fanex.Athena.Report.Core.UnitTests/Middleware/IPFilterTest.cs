using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.DTO;
using Fanex.Athena.Report.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Middleware
{
    public class IPFilterTest
    {
        private readonly Mock<IOptions<ApplicationOptions>> applicationOptionAccessor;

        private const string ExpectedResponseBody = "Completed";

        private const string AllowedIP = "10.10.19.72";

        public IPFilterTest()
        {
            applicationOptionAccessor = new Mock<IOptions<ApplicationOptions>>();
            applicationOptionAccessor.Setup(app => app.Value).Returns(new ApplicationOptions
            {
                WhiteList = new List<string> { AllowedIP }
            });
        }

        [Fact]
        public async Task Invoke_IPIsAllowed_ResponseHasCorrectContent()
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
        public async Task Invoke_IPIsNotAllowed_ResponseHasCorrectStatusCode()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = CreateHttpContext(remoteIpAddress: "19.1.1.55");

            // Action
            await middleware.Invoke(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        }

        private IPFilter CreateMiddleware()
        {
            return new IPFilter(
                next: (innerHttpContext) =>
                {
                    innerHttpContext.Response.WriteAsync(ExpectedResponseBody);
                    return Task.CompletedTask;
                },
                applicationOptionAccessor: applicationOptionAccessor.Object
            );
        }

        private HttpContext CreateHttpContext(string remoteIpAddress = AllowedIP)
        {
            var connectionInfo = Mock.Of<ConnectionInfo>(x => x.RemoteIpAddress == IPAddress.Parse(remoteIpAddress));
            var response = Mock.Of<HttpResponse>(r => r.Body == new MemoryStream());
            var context = Mock.Of<HttpContext>(ctx => ctx.Response == response && ctx.Connection == connectionInfo);

            return context;
        }
    }
}