using Fanex.Athena.Report.Core.ApiClients;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.ApiClients
{
    public class CommonApiClientTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly ICommonApiClient _commonApiClient;

        public CommonApiClientTest()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _contextService = new Mock<IContextService>();
            _httpMessageHandler = new Mock<HttpMessageHandler>();
            
            _httpClient = new HttpClient(_httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };

            _commonApiClient = new CommonApiClient(_httpContextAccessor.Object, _httpClient, _contextService.Object);
        }

        [Fact]
        public async Task GetAsync_WithValidPath_ReturnsDeserializedResult()
        {
            // Arrange
            var path = "test/endpoint";
            var expectedResult = new TestModel { Id = 1, Name = "Test" };
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                ErrorMessage = null,
                Result = expectedResult
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _commonApiClient.GetAsync<TestModel>(path);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Id, result.Id);
            Assert.Equal(expectedResult.Name, result.Name);
        }

        [Fact]
        public async Task GetAsync_WithQueryParameters_BuildsCorrectUrl()
        {
            // Arrange
            var path = "test/endpoint";
            var queryParams = new { userId = 123, status = "active" };
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            HttpRequestMessage capturedRequest = null;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
                .ReturnsAsync(httpResponse);

            // Act
            await _commonApiClient.GetAsync<TestModel>(path, queryParams);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("userId=123", capturedRequest.RequestUri.ToString());
            Assert.Contains("status=active", capturedRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetAsync_WithErrorResponse_ReturnsDefault()
        {
            // Arrange
            var path = "test/endpoint";
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 1,
                ErrorMessage = "Error occurred",
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _commonApiClient.GetAsync<TestModel>(path);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WithCustomTimeout_UsesSpecifiedTimeout()
        {
            // Arrange
            var path = "test/endpoint";
            var timeout = 60;
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _commonApiClient.GetAsync<TestModel>(path, null, timeout);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAsync_WithNullQueryObject_DoesNotAddQueryString()
        {
            // Arrange
            var path = "test/endpoint";
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            HttpRequestMessage capturedRequest = null;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
                .ReturnsAsync(httpResponse);

            // Act
            await _commonApiClient.GetAsync<TestModel>(path, null);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal($"https://api.example.com/{path}", capturedRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetAsync_SetsLanguageHeader()
        {
            // Arrange
            var path = "test/endpoint";
            var language = "zh-CN";
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns(language);

            HttpRequestMessage capturedRequest = null;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
                .ReturnsAsync(httpResponse);

            // Act
            await _commonApiClient.GetAsync<TestModel>(path);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.Headers.Contains("Language"));
            Assert.Contains(language, capturedRequest.Headers.GetValues("Language"));
        }

        [Theory]
        [InlineData("users")]
        [InlineData("reports/daily")]
        [InlineData("api/v1/data")]
        public async Task GetAsync_WithVariousPaths_ProcessesCorrectly(string path)
        {
            // Arrange
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _commonApiClient.GetAsync<TestModel>(path);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAsync_WithComplexQueryObject_BuildsCorrectQueryString()
        {
            // Arrange
            var path = "test/endpoint";
            var queryParams = new 
            { 
                userId = 123, 
                status = "active",
                date = DateTime.Parse("2023-01-01"),
                isEnabled = true
            };
            var apiResponse = new ApiResponse<TestModel>
            {
                ErrorCode = 0,
                Result = new TestModel { Id = 1, Name = "Test" }
            };
            var responseJson = JsonConvert.SerializeObject(apiResponse);

            _contextService.Setup(x => x.Language).Returns("en");

            HttpRequestMessage capturedRequest = null;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
                .ReturnsAsync(httpResponse);

            // Act
            await _commonApiClient.GetAsync<TestModel>(path, queryParams);

            // Assert
            Assert.NotNull(capturedRequest);
            var query = capturedRequest.RequestUri.Query;
            Assert.Contains("userId=123", query);
            Assert.Contains("status=active", query);
            Assert.Contains("isEnabled=True", query);
        }

        public class TestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}