using Fanex.Athena.Report.Core.ApiClients;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using HttpClient = System.Net.Http.HttpClient;

namespace Fanex.Athena.Report.Core.UnitTests.ApiClients
{
    public class BetListApiClientTest
    {
        private readonly Mock<HttpClient> _client;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;

        private readonly Mock<HttpMessageHandler> _httpMessageHandler;

        public BetListApiClientTest()
        {
            _contextService = new Mock<IContextService>();

            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContextAccessor.SetReturnsDefault<HttpContext>(null);

            _httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _client = new Mock<HttpClient>(_httpMessageHandler.Object);
            _client.Object.BaseAddress = new Uri("http://test.com");
        }

        [Fact]
        public async Task PostByteArrayAsync_ThrowException_ThrowServiceException()
        {
            //Arrange
            const string Language = "en-US";

            const string Expected = "R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==";
            var expectedContent = Encoding.UTF8.GetBytes(Expected);

            using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new ByteArrayContent(expectedContent), };

            _httpMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>(
                                    "SendAsync",
                                    ItExpr.IsAny<HttpRequestMessage>(),
                                    ItExpr.IsAny<CancellationToken>()
                                )
                               .ReturnsAsync(httpResponseMessage);

            var apiClient = new BetListApiClient(_httpContextAccessor.Object, _client.Object,
                _contextService.Object);

            //Action
            var result = await apiClient.PostByteArrayAsync("/test", "test-data", Language).ConfigureAwait(false);

            //Assert
            Assert.Equal(Expected, Encoding.Default.GetString(result));
        }
    }
}
