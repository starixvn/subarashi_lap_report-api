using AutoFixture;
using Fanex.Athena.Report.Core.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class ContextServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<HttpContext> _httpContext;
        private readonly Mock<ConnectionInfo> _connectionInfo;
        private readonly ClaimsPrincipal _user;

        public ContextServiceTest()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContext = new Mock<HttpContext>();
            _connectionInfo = new Mock<ConnectionInfo>();
            _user = new ClaimsPrincipal();

            _httpContext.Setup(x => x.User).Returns(_user);
            _httpContext.Setup(x => x.Connection).Returns(_connectionInfo.Object);
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext.Object);
        }

        [Fact]
        public void ClientIP_ReturnsRemoteIpAddress()
        {
            // Arrange
            var expectedIp = IPAddress.Parse("192.168.1.1");
            _connectionInfo.Setup(x => x.RemoteIpAddress).Returns(expectedIp);

            var service = CreateService();

            // Act
            var result = service.ClientIP;

            // Assert
            Assert.Equal(expectedIp.ToString(), result);
        }

        [Fact]
        public void LicenseeId_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedLicenseeId = _fixture.Create<int>();
            var claims = new List<Claim>
            {
                new Claim("LicenseeId", expectedLicenseeId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.LicenseeId;

            // Assert
            Assert.Equal(expectedLicenseeId, result);
        }

        [Fact]
        public void LicenseeId_WhenClaimNotExists_ReturnsDefault()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.LicenseeId;

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void SiteName_ReturnsClaimValue()
        {
            // Arrange
            var expectedSiteName = _fixture.Create<string>();
            var claims = new List<Claim>
            {
                new Claim("SiteName", expectedSiteName)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.SiteName;

            // Assert
            Assert.Equal(expectedSiteName, result);
        }

        [Fact]
        public void IsSuperAdmin_WhenAdministratorClaimIsTrue_ReturnsTrue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("Administrator", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsSuperAdmin;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserName_ReturnsUserIDClaim()
        {
            // Arrange
            var expectedUserName = _fixture.Create<string>();
            var claims = new List<Claim>
            {
                new Claim("UserID", expectedUserName)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.UserName;

            // Assert
            Assert.Equal(expectedUserName, result);
        }

        [Fact]
        public void IsInRoles_WhenUserInRole_ReturnsTrue()
        {
            // Arrange
            var role = "Administrator";
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsInRoles(role);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInRoles_WhenUserNotInRole_ReturnsFalse()
        {
            // Arrange
            var role = "Administrator";
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsInRoles(role);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SpecialPlatform_ReturnsClaimValue()
        {
            // Arrange
            var expectedPlatform = _fixture.Create<int>();
            var claims = new List<Claim>
            {
                new Claim("SpecialPlatform", expectedPlatform.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.SpecialPlatform;

            // Assert
            Assert.Equal(expectedPlatform, result);
        }

        [Fact]
        public void DefaultBaseCurrency_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedCurrency = "USD";
            var claims = new List<Claim>
            {
                new Claim("BaseCurrency", expectedCurrency)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.DefaultBaseCurrency;

            // Assert
            Assert.Equal(expectedCurrency, result);
        }

        [Fact]
        public void DefaultBaseCurrency_WhenClaimNotExists_ReturnsDefault()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.DefaultBaseCurrency;

            // Assert
            Assert.Equal("EUR", result);
        }

        private ContextService CreateService()
        {
            return new ContextService(_httpContextAccessor.Object);
        }
    }
}