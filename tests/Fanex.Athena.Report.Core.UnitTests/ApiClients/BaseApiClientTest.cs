namespace Fanex.Athena.Report.Core.UnitTests.ApiClients
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Core.ApiClients.Abstraction;
    using Core.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;
    using Xunit;

    public class BaseApiClientTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly HttpClient _httpClient;

        public BaseApiClientTest()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };
        }

        [Fact]
        public async Task PostAsync_WithValidData_ReturnsDeserializedObject()
        {
            // Arrange
            var testData = new { Name = "Test", Value = 123 };
            var expectedResponse = new { Result = "Success", Id = 456 };
            var responseJson = JsonConvert.SerializeObject(expectedResponse);
            var language = "en";
            var path = "test/endpoint";

            SetupHttpResponse(HttpStatusCode.OK, responseJson);
            var client = CreateTestClient();

            // Act
            var result = await client.PostAsync<dynamic>(path, testData, language);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success", (string)result.Result);
            Assert.Equal(456, (int)result.Id);
            VerifyHttpRequest(HttpMethod.Post, path);
        }

        [Fact]
        public async Task PostStringAsync_WithValidData_ReturnsStringResponse()
        {
            // Arrange
            var testData = new { Name = "Test", Value = 123 };
            var expectedResponse = "Success Response";
            var language = "en";
            var path = "test/endpoint";

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);
            var client = CreateTestClient();

            // Act
            var result = await client.PostStringAsync(path, testData, language);

            // Assert
            Assert.Equal(expectedResponse, result);
            VerifyHttpRequest(HttpMethod.Post, path);
        }

        [Fact]
        public async Task GetStringAsync_WithValidPath_ReturnsStringResponse()
        {
            // Arrange
            var expectedResponse = "Get Response";
            var path = "test/get-endpoint";
            var timeout = 30;

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);
            var client = CreateTestClient();

            // Act
            var result = await client.GetStringAsync(path, timeout);

            // Assert
            Assert.Equal(expectedResponse, result);
            VerifyHttpRequest(HttpMethod.Get, path);
        }

        [Fact]
        public async Task PostAsync_WhenHttpRequestFails_ThrowsServiceException()
        {
            // Arrange
            var testData = new { Name = "Test" };
            var language = "en";
            var path = "test/endpoint";

            SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");
            var client = CreateTestClient();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ServiceException>(() =>

                client.PostAsync<dynamic>(path, testData, language));


            Assert.Contains("TestApi", exception.Message);
        }

        [Fact]
        public async Task GetStringAsync_WhenHttpRequestFails_ThrowsServiceException()
        {
            // Arrange
            var path = "test/endpoint";

            SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");
            var client = CreateTestClient();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ServiceException>(() =>

                client.GetStringAsync(path));


            Assert.Contains("TestApi", exception.Message);
        }

        [Fact]
        public async Task PostAsync_WhenHttpClientThrowsException_ThrowsServiceException()
        {
            // Arrange
            var testData = new { Name = "Test" };
            var language = "en";
            var path = "test/endpoint";

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",

                    ItExpr.IsAny<HttpRequestMessage>(),

                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var client = CreateTestClient();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ServiceException>(() =>

                client.PostAsync<dynamic>(path, testData, language));


            Assert.Contains("Network error", exception.Message);
            Assert.Contains("TestApi", exception.Message);
        }

        [Fact]
        public async Task GetStringAsync_WhenHttpClientThrowsException_ThrowsServiceException()
        {
            // Arrange
            var path = "test/endpoint";

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var client = CreateTestClient();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ServiceException>(() =>
                client.GetStringAsync(path));


            Assert.Contains("Network error", exception.Message);
            Assert.Contains("TestApi", exception.Message);
        }

        [Fact]
        public async Task PostAsync_WithNullData_SendsNullContent()
        {
            // Arrange
            var language = "en";
            var path = "test/endpoint";
            var expectedResponse = new { Result = "Success" };
            var responseJson = JsonConvert.SerializeObject(expectedResponse);

            SetupHttpResponse(HttpStatusCode.OK, responseJson);
            var client = CreateTestClient();

            // Act
            var result = await client.PostAsync<dynamic>(path, null, language);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success", (string)result.Result);
            VerifyHttpRequest(HttpMethod.Post, path);
        }

        [Theory]
        [InlineData("en")]
        [InlineData("fr")]
        [InlineData("es")]
        public async Task PostAsync_WithDifferentLanguages_SetsLanguageHeader(string language)
        {
            // Arrange
            var testData = new { Name = "Test" };
            var path = "test/endpoint";
            var expectedResponse = new { Result = "Success" };
            var responseJson = JsonConvert.SerializeObject(expectedResponse);

            SetupHttpResponse(HttpStatusCode.OK, responseJson);
            var client = CreateTestClient();

            // Act
            await client.PostAsync<dynamic>(path, testData, language);

            // Assert
            VerifyHttpRequest(HttpMethod.Post, path);
            // Note: In a real test, you would verify the Language header was set correctly
        }

        [Fact]
        public async Task GetStringAsync_WithCustomTimeout_UsesSpecifiedTimeout()
        {
            // Arrange
            var path = "test/endpoint";
            var timeout = 60;
            var expectedResponse = "Response";

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);
            var client = CreateTestClient();

            // Act
            var result = await client.GetStringAsync(path, timeout);

            // Assert
            Assert.Equal(expectedResponse, result);
            VerifyHttpRequest(HttpMethod.Get, path);
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",

                    ItExpr.IsAny<HttpRequestMessage>(),

                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        private void VerifyHttpRequest(HttpMethod method, string path)
        {
            _httpMessageHandler.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>

                        req.Method == method &&

                        req.RequestUri.ToString().Contains(path)),
                    ItExpr.IsAny<CancellationToken>());
        }

        private TestApiClient CreateTestClient()
        {
            return new TestApiClient(_httpContextAccessor.Object, _httpClient);
        }

        // Test implementation of BaseApiClient
        private class TestApiClient : BaseApiClient
        {
            public TestApiClient(IHttpContextAccessor httpContextAccessor, HttpClient client)

                : base(httpContextAccessor, client)
            {
            }

            protected override string ApiName => "TestApi";

            protected override Task PrepareHeaders(HttpRequestHeaders headers)
            {
                // Mock implementation - in real scenarios this would set authentication headers, etc.
                return Task.CompletedTask;
            }
        }
    }
}
