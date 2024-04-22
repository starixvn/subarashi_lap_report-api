using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Exceptions;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.ApiClients.Abstraction
{
    public class BaseApiClientTest
    {
        private readonly Mock<HttpClient> _client;

        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;

        private readonly Mock<HttpMessageHandler> _httpMessageHandler;

        public BaseApiClientTest()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _client = new Mock<HttpClient>(_httpMessageHandler.Object);
            _client.Object.BaseAddress = new Uri("http://test.com");

            _httpContextAccessor.SetReturnsDefault<HttpContext>(null);
        }

        [Fact]
        public async Task PostStringAsync_WhenCallSuccess_ReturnResult()
        {
            //Arrange
            const string language = "en-US";
            const string FakeContent = "ANY";

            using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(FakeContent), };

            _httpMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>(
                                    "SendAsync",
                                    ItExpr.IsAny<HttpRequestMessage>(),
                                    ItExpr.IsAny<CancellationToken>()
                                )
                               .ReturnsAsync(httpResponseMessage).Verifiable();

            var apiClient = new MockApiClient(_httpContextAccessor.Object, _client.Object);

            //Action
            var result = await apiClient.PostStringAsync("/test", "test-data", language).ConfigureAwait(false);

            //Assert
            Assert.Equal(FakeContent, result);
        }

        [Fact]
        public async Task PostAsync_WhenCallSuccess_ReturnResult()
        {
            //Arrange
            const string language = "en-US";
            var expected = FakeHelper.CreateMany<ResponseObject>().ToList();
            var expectedContent = JsonConvert.SerializeObject(expected);

            using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(expectedContent), };

            _httpMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>(
                                    "SendAsync",
                                    ItExpr.IsAny<HttpRequestMessage>(),
                                    ItExpr.IsAny<CancellationToken>()
                                )
                               .ReturnsAsync(httpResponseMessage).Verifiable();

            var apiClient = new MockApiClient(_httpContextAccessor.Object, _client.Object);

            //Action
            var result = await apiClient.PostAsync<List<ResponseObject>>("/test", "test-data", language)
                                        .ConfigureAwait(false);

            //Assert
            Assert.Equal(expected[0].Name, result[0].Name);
            Assert.Equal(expected[0].Detail, result[0].Detail);
            Assert.Equal(expected[1].Name, result[1].Name);
            Assert.Equal(expected[1].Detail, result[1].Detail);
            Assert.Equal(expected[2].Name, result[2].Name);
            Assert.Equal(expected[2].Detail, result[2].Detail);
        }

        [Fact]
        public async Task PostStringAsync_ThrowException_ThrowServiceException()
        {
            //Arrange
            const string FakeContent = "ANY";

            using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(FakeContent), };

            _httpMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>(
                                    "SendAsync",
                                    ItExpr.IsAny<HttpRequestMessage>(),
                                    ItExpr.IsAny<CancellationToken>()
                                )
                               .ThrowsAsync(new Exception());

            var apiClient = new MockApiClient(_httpContextAccessor.Object, _client.Object);

            //Action && Assert
            await Assert.ThrowsAsync<ServiceException>(async () =>
            {
                await apiClient.PostAsync<string>("test/path", null, "en");
            });
        }

        protected class ResponseObject
        {
            public string Name { get; set; }

            public string Detail { get; set; }
        }

        protected class MockApiClient : BaseApiClient
        {
            public MockApiClient(IHttpContextAccessor httpContextAccessor, HttpClient client)
                : base(httpContextAccessor, client) { }

            protected override string ApiName => "TestApi";

            protected override Task PrepareHeaders(HttpRequestHeaders headers) =>
                // Mock implementation of PrepareHeaders method
                Task.CompletedTask;
        }
    }
}
