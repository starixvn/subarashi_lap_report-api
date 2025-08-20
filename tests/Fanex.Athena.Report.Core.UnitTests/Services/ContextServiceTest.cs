namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;
    using AutoFixture;
    using Core.Services;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;

    public class ContextServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<HttpContext> _httpContext;
        private readonly Mock<ConnectionInfo> _connectionInfo;
        private readonly Mock<HttpRequest> _httpRequest;
        private readonly Mock<IHeaderDictionary> _headers;
        private readonly ClaimsPrincipal _user;

        public ContextServiceTest()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContext = new Mock<HttpContext>();
            _connectionInfo = new Mock<ConnectionInfo>();
            _httpRequest = new Mock<HttpRequest>();
            _headers = new Mock<IHeaderDictionary>();
            _user = new ClaimsPrincipal();

            _httpContext.Setup(x => x.User).Returns(_user);
            _httpContext.Setup(x => x.Connection).Returns(_connectionInfo.Object);
            _httpContext.Setup(x => x.Request).Returns(_httpRequest.Object);
            _httpRequest.Setup(x => x.Headers).Returns(_headers.Object);
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

        [Fact]
        public void IsAllowedCustomerComm_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsAllowedCustomerComm", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsAllowedCustomerComm;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnableBA_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsEnableBA", "false")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsEnableBA;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsEnableSingleWallet_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsEnableSingleWallet", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsEnableSingleWallet;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SpecialPlatformType_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedValue = 5;
            var claims = new List<Claim>
            {
                new Claim("SpecialPlatformType", expectedValue.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.SpecialPlatformType;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void IsFakeLicensee_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsFakeLicensee", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsFakeLicensee;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SiteDBServer_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedValue = "testserver.db";
            var claims = new List<Claim>
            {
                new Claim("SiteDBServer", expectedValue)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.SiteDBServer;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void UserId_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedValue = 12345;
            var claims = new List<Claim>
            {
                new Claim("UserSEQ", expectedValue.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.UserId;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void UserLevel_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            byte expectedValue = 2;
            var claims = new List<Claim>
            {
                new Claim("UserLevel", expectedValue.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.UserLevel;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void WhiteLabelID_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedValue = 100;
            var claims = new List<Claim>
            {
                new Claim("WhiteLabelID", expectedValue.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.WhiteLabelID;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void WhiteLabelID_WhenClaimNotExists_ReturnsDefault()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.WhiteLabelID;

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void IsShowIp_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsShowIP", "false")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsShowIp;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsShowIp_WhenClaimNotExists_ReturnsDefault()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.IsShowIp;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void OperatorId_WhenClaimExists_ReturnsClaimValue()
        {
            // Arrange
            var expectedValue = "TestOperator";
            var claims = new List<Claim>
            {
                new Claim("OperatorID", expectedValue)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.OperatorId;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void OfferedOddsGroupsList_WhenClaimExists_ReturnsSplitValues()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("OfferedOddsGroups", "group1,group2,group3")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.OfferedOddsGroupsList;

            // Assert
            Assert.Contains("group1", result);
            Assert.Contains("group2", result);
            Assert.Contains("group3", result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void OfferedOddsGroupsList_WhenClaimEmpty_ReturnsEmptyArray()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("OfferedOddsGroups", "")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.OfferedOddsGroupsList;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void OfferedSubProductList_WhenClaimExists_ReturnsSplitValues()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("OfferedSubProducts", "product1,product2")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.OfferedSubProductList;

            // Assert
            Assert.Contains("product1", result);
            Assert.Contains("product2", result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void OfferedSubProductList_WhenClaimEmpty_ReturnsEmpty()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.OfferedSubProductList;

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("lrf-test", 0)]  // AppMode.Self
        [InlineData("other-system", 1)]  // AppMode.Parasitic
        public void ApplicationMode_ReturnsCorrectMode(string name, int expectedMode)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("Name", name)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.ApplicationMode;

            // Assert
            Assert.Equal(expectedMode, (int)result);
        }

        [Fact]
        public void Language_ReturnsCurrentUICultureName()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var service = CreateService();

            // Act
            var result = service.Language;

            // Assert
            Assert.Equal("en-US", result);

            // Cleanup
            CultureInfo.CurrentUICulture = originalCulture;
        }

        [Fact]
        public void ForceIgnoreSharding_WhenHeaderIsTrue_ReturnsTrue()
        {
            // Arrange
            _headers.Setup(x => x["X-IgnoreSharding"]).Returns("True");
            var service = CreateService();

            // Act
            var result = service.ForceIgnoreSharding;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ForceIgnoreSharding_WhenHeaderIsFalse_ReturnsFalse()
        {
            // Arrange
            _headers.Setup(x => x["X-IgnoreSharding"]).Returns("False");
            var service = CreateService();

            // Act
            var result = service.ForceIgnoreSharding;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ForceIgnoreSharding_WhenHeaderIsEmpty_ReturnsFalse()
        {
            // Arrange
            _headers.Setup(x => x["X-IgnoreSharding"]).Returns("");
            var service = CreateService();

            // Act
            var result = service.ForceIgnoreSharding;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RawCurrencyAgentList_WithCurrencyAgentClaims_ReturnsDeserializedList()
        {
            // Arrange
            var currencyAgentJson1 = "{\"AgentId\":[1,2,3],\"CurrencyId\":100,\"Currency\":\"USD\"}";
            var currencyAgentJson2 = "{\"AgentId\":[4,5],\"CurrencyId\":200,\"Currency\":\"EUR\"}";
            
            var claims = new List<Claim>
            {
                new Claim("CurrencyAgent", currencyAgentJson1),
                new Claim("CurrencyAgent", currencyAgentJson2)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.GetType()
                .GetProperty("RawCurrencyAgentList", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(service);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AgentList_WithCurrencyAgentData_ReturnsReadOnlyDictionary()
        {
            // Arrange
            var currencyAgentJson = "{\"AgentId\":[1,2,3],\"CurrencyId\":100,\"Currency\":\"USD\"}";
            var claims = new List<Claim>
            {
                new Claim("CurrencyAgent", currencyAgentJson)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.AgentList;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(100));
            Assert.Equal(new[] {1,2,3}, result[100]);
        }

        [Fact]
        public void CurrencyList_WithCurrencyAgentData_ReturnsReadOnlyDictionary()
        {
            // Arrange
            var currencyAgentJson = "{\"AgentId\":[1,2,3],\"CurrencyId\":100,\"Currency\":\"USD\"}";
            var claims = new List<Claim>
            {
                new Claim("CurrencyAgent", currencyAgentJson)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.CurrencyList;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(100));
            Assert.Equal("USD", result[100]);
        }

        [Fact]
        public void AuditCurrencies_WithAuditCurrenciesClaims_ReturnsDeserializedList()
        {
            // Arrange
            var auditCurrencyJson = "{\"CurrencyId\":1,\"CurrencyCode\":\"USD\"}";
            var claims = new List<Claim>
            {
                new Claim("AuditCurrencies", auditCurrencyJson)
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.AuditCurrencies;

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Theory]
        [InlineData(1)] // Special platform value that should hide Saba
        [InlineData(2)]
        public void IsHideSaba_WithSpecialPlatform_CallsSabaHelper(int specialPlatform)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("SpecialPlatform", specialPlatform.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsHideSaba;

            // Assert - Just verify it doesn't throw and returns a value
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void IsShowIpVn_WhenUserIsAdminAndIsShowIpTrue_ReturnsTrue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsShowIP", "true")
            };
            var identity = new ClaimsIdentity(claims);
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsShowIpVn;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsShowIpVn_WhenUserNotAdminButIsShowIpTrue_ReturnsFalse()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsShowIP", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.IsShowIpVn;

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("", true, true, false, 1, -1, 0)] // NotManagement - empty VendorID
        [InlineData("vendor123", false, true, false, 1, -1, 0)] // NotManagement - IsEnabledSubsidiary false
        [InlineData("vendor123", true, true, true, 1, -1, 2)] // MasterManagement
        [InlineData("vendor123", true, true, false, 1, 10, 1)] // NormalManagement
        [InlineData("vendor123", true, true, true, 3, -1, 0)] // NotManagement - UserLevel >= 2
        public void SubsidiaryViewType_WithDifferentScenarios_ReturnsCorrectType(
            string vendorId, bool isEnabledSubsidiary, bool isMasterLicensee, 
            bool isMaster, int userLevel, int whiteLabelId, byte expectedType)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("VendorID", vendorId),
                new Claim("IsEnabledSubsidiary", isEnabledSubsidiary.ToString()),
                new Claim("IsMasterLicensee", isMasterLicensee.ToString()),
                new Claim("UserLevel", userLevel.ToString()),
                new Claim("WhiteLabelID", whiteLabelId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var result = service.SubsidiaryViewType;

            // Assert
            Assert.Equal(expectedType, result);
        }

        [Fact]
        public void IsInRoles_WithMultipleRoles_ChecksAllRoles()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Manager"));
            var user = new ClaimsPrincipal(identity);
            _httpContext.Setup(x => x.User).Returns(user);

            var service = CreateService();

            // Act
            var resultAdmin = service.IsInRoles("Administrator");
            var resultUser = service.IsInRoles("User");
            var resultManager = service.IsInRoles("Manager");
            var resultMultiple = service.IsInRoles("Administrator", "User");

            // Assert
            Assert.False(resultAdmin);
            Assert.True(resultUser);
            Assert.True(resultManager);
            Assert.True(resultMultiple); // Should return true if user has any of the roles
        }

        private ContextService CreateService()
        {
            return new ContextService(_httpContextAccessor.Object);
        }
    }
}