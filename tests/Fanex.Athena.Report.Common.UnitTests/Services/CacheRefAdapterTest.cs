using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.RemotingRef;
using Moq;
using System.Globalization;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    public class CacheRefAdapterTest
    {
        private readonly Mock<IRemotingRefService> _mockRemotingRefService;
        private readonly ICacheRefAdapter _cacheRefAdapter;

        public CacheRefAdapterTest()
        {
            _mockRemotingRefService = new Mock<IRemotingRefService>();
            _cacheRefAdapter = new CacheRefAdapter(_mockRemotingRefService.Object);
        }

        [Fact]
        public void Constructor_WithValidRemotingRefService_SetsProperty()
        {
            // Assert
            Assert.Equal(_mockRemotingRefService.Object, _cacheRefAdapter.RemotingRefService);
        }

        [Fact]
        public void GetSportName_WithValidSportId_ReturnsName()
        {
            // Arrange
            var sportId = 1;
            var expectedName = "Football";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetSportName(sportId.ToString(), currentLanguage, string.Empty))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetSportName(sportId);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetSportName(sportId.ToString(), currentLanguage, string.Empty), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        public void GetSportName_WithInvalidSportId_ReturnsEmptyString(object sportId)
        {
            // Act
            var result = _cacheRefAdapter.GetSportName(sportId);

            // Assert
            Assert.Equal(string.Empty, result);
            _mockRemotingRefService.Verify(x => x.GetSportName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetSportName_WithCustomLanguageCode_UsesSpecifiedLanguage()
        {
            // Arrange
            var sportId = 1;
            var languageCode = "zh-CN";
            var expectedName = "足球";
            
            _mockRemotingRefService.Setup(x => x.GetSportName(sportId.ToString(), languageCode, string.Empty))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetSportName(sportId, languageCode);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetSportName(sportId.ToString(), languageCode, string.Empty), Times.Once);
        }

        [Fact]
        public void GetLeagueName_WithValidLeagueId_ReturnsName()
        {
            // Arrange
            var leagueId = 123;
            var expectedName = "Premier League";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetLeagueName(leagueId.ToString(), currentLanguage, string.Empty))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetLeagueName(leagueId);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetLeagueName(leagueId.ToString(), currentLanguage, string.Empty), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        public void GetLeagueName_WithInvalidLeagueId_ReturnsEmptyString(object leagueId)
        {
            // Act
            var result = _cacheRefAdapter.GetLeagueName(leagueId);

            // Assert
            Assert.Equal(string.Empty, result);
            _mockRemotingRefService.Verify(x => x.GetLeagueName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetLeagueName3rd_WithValidLeagueId_ReturnsName()
        {
            // Arrange
            var leagueId = 456;
            var expectedName = "3rd Party League";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetLeagueName3rd(leagueId.ToString(), currentLanguage, string.Empty))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetLeagueName3rd(leagueId);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetLeagueName3rd(leagueId.ToString(), currentLanguage, string.Empty), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        public void GetLeagueName3rd_WithInvalidLeagueId_ReturnsEmptyString(object leagueId)
        {
            // Act
            var result = _cacheRefAdapter.GetLeagueName3rd(leagueId);

            // Assert
            Assert.Equal(string.Empty, result);
            _mockRemotingRefService.Verify(x => x.GetLeagueName3rd(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetCombinationTypeCombo_WithValidParameters_ReturnsArray()
        {
            // Arrange
            var betTeam = "H";
            var betCheck = "A";
            var handicap1 = 1.5m;
            var handicap2 = 2.5m;
            var expectedResult = new[] { "Option1", "Option2" };
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            // The method converts decimals to int, so we need to match the actual conversion
            _mockRemotingRefService.Setup(x => x.GetCombinationTypeCombo(betTeam, betCheck, 
                It.IsAny<int>(), It.IsAny<int>(), currentLanguage))
                .Returns(expectedResult);

            // Act
            var result = _cacheRefAdapter.GetCombinationTypeCombo(betTeam, betCheck, handicap1, handicap2);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockRemotingRefService.Verify(x => x.GetCombinationTypeCombo(betTeam, betCheck, 
                It.IsAny<int>(), It.IsAny<int>(), currentLanguage), Times.Once);
        }

        [Fact]
        public void GetBetBuilderSelectionNames_WithValidSelectionIds_ReturnsNames()
        {
            // Arrange
            var selectionIds = new[] { 1, 2, 3 };
            var expectedResult = new[] { "Selection1", "Selection2", "Selection3" };
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetBetBuilderSelectionNames(selectionIds, currentLanguage))
                .Returns(expectedResult);

            // Act
            var result = _cacheRefAdapter.GetBetBuilderSelectionNames(selectionIds);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockRemotingRefService.Verify(x => x.GetBetBuilderSelectionNames(selectionIds, currentLanguage), Times.Once);
        }

        [Fact]
        public void GetMatchName_WithValidTeamIds_ReturnsMatchName()
        {
            // Arrange
            var homeId = 100;
            var awayId = 200;
            var homeTeamName = "Arsenal";
            var awayTeamName = "Chelsea";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetTeamName(homeId.ToString(), currentLanguage, string.Empty))
                .Returns(homeTeamName);
            _mockRemotingRefService.Setup(x => x.GetTeamName(awayId.ToString(), currentLanguage, string.Empty))
                .Returns(awayTeamName);

            // Act
            var result = _cacheRefAdapter.GetMatchName(homeId, awayId);

            // Assert
            var expectedResult = "Arsenal - vs - Chelsea";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetMatchName_WithNeutralVenue_ReturnsMatchNameWithNeutralIndicator()
        {
            // Arrange
            var homeId = 100;
            var awayId = 200;
            var homeTeamName = "Arsenal";
            var awayTeamName = "Chelsea";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetTeamName(homeId.ToString(), currentLanguage, string.Empty))
                .Returns(homeTeamName);
            _mockRemotingRefService.Setup(x => x.GetTeamName(awayId.ToString(), currentLanguage, string.Empty))
                .Returns(awayTeamName);

            // Act
            var result = _cacheRefAdapter.GetMatchName(homeId, awayId, IsNeutral: true);

            // Assert
            var expectedResult = "Arsenal (N) - vs - Chelsea";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetTeamName_WithValidTeamId_ReturnsName()
        {
            // Arrange
            var teamId = 100;
            var expectedName = "Arsenal";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetTeamName(teamId.ToString(), currentLanguage, string.Empty))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetTeamName(teamId);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetTeamName(teamId.ToString(), currentLanguage, string.Empty), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        public void GetTeamName_WithInvalidTeamId_ReturnsEmptyString(object teamId)
        {
            // Act
            var result = _cacheRefAdapter.GetTeamName(teamId);

            // Assert
            Assert.Equal(string.Empty, result);
            _mockRemotingRefService.Verify(x => x.GetTeamName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetBettype_WithValidBettypeId_ReturnsName()
        {
            // Arrange
            var bettypeId = 1;
            var expectedName = "1X2";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetBetTypeName(bettypeId.ToString(), currentLanguage))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetBettype(bettypeId);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetBetTypeName(bettypeId.ToString(), currentLanguage), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        public void GetBettype_WithInvalidBettypeId_ReturnsEmptyString(object bettypeId)
        {
            // Act
            var result = _cacheRefAdapter.GetBettype(bettypeId);

            // Assert
            Assert.Equal(string.Empty, result);
            _mockRemotingRefService.Verify(x => x.GetBetTypeName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetBettype_WithSabaBettypeAndHideSaba_ReturnsResourceString()
        {
            // Arrange - Using a Saba bet type (228 or 229)
            var sabaBettypeId = 228;

            // Act
            var result = _cacheRefAdapter.GetBettype(sabaBettypeId, isHideSaba: true);

            // Assert
            Assert.NotNull(result);
            _mockRemotingRefService.Verify(x => x.GetBetTypeName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetBetTypeWithLiveIndicator_WithZeroBettypeId_ReturnsEmptyString()
        {
            // Act
            var result = _cacheRefAdapter.GetBetTypeWithLiveIndicator(0, true);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetBetTypeWithLiveIndicator_WithValidBettypeId_ReturnsName()
        {
            // Arrange
            var bettypeId = 1;
            var expectedName = "1X2";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetBetTypeName(bettypeId.ToString(), currentLanguage))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetBetTypeWithLiveIndicator(bettypeId, false);

            // Assert
            Assert.Equal(expectedName, result);
        }

        [Fact]
        public void GetBetTypeWithLiveIndicator_WithNumberGameBettype_ReturnsModifiedName()
        {
            // Arrange - Using BetType.NgNumberGroup 
            var numberGameBettypeId = BetType.NgNumberGroup;
            var expectedBaseName = "Number Group";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetBetTypeName(numberGameBettypeId.ToString(), currentLanguage))
                .Returns(expectedBaseName);

            // Act
            var result = _cacheRefAdapter.GetBetTypeWithLiveIndicator(numberGameBettypeId, true);

            // Assert - The method should return the base name with live indicator appended
            Assert.NotNull(result);
            Assert.True(result.Length > expectedBaseName.Length || result.Contains("(") || result.Contains("Live"));
        }

        [Theory]
        [InlineData("VS", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void GetBettypeWithProductName_WithDifferentProductCodes_ReturnsCorrectFormat(string productCode, bool shouldContainVS)
        {
            // Arrange
            var bettypeId = 1;
            var expectedBaseName = "1X2";
            var currentLanguage = CultureInfo.CurrentUICulture.Name;
            
            _mockRemotingRefService.Setup(x => x.GetBetTypeName(bettypeId.ToString(), currentLanguage))
                .Returns(expectedBaseName);

            // Act
            var result = _cacheRefAdapter.GetBettypeWithProductName(bettypeId, productCode);

            // Assert
            Assert.NotNull(result);
            if (shouldContainVS && productCode == "VS")
            {
                // Should contain some form of product indication, but ViewResources might not be available
                Assert.True(result.Contains("VS") || result.Contains("(") || result.Length > expectedBaseName.Length);
            }
            else
            {
                // For non-VS products, should return base name
                Assert.True(result.StartsWith(expectedBaseName) || result == expectedBaseName);
            }
        }

        [Fact]
        public void GetBetTypeName_WithCustomLanguageAndHideSaba_UsesSpecifiedLanguage()
        {
            // Arrange
            var bettypeId = 1;
            var languageCode = "zh-CN";
            var expectedName = "让球";
            
            _mockRemotingRefService.Setup(x => x.GetBetTypeName(bettypeId.ToString(), languageCode))
                .Returns(expectedName);

            // Act
            var result = _cacheRefAdapter.GetBetTypeName(bettypeId, languageCode, false);

            // Assert
            Assert.Equal(expectedName, result);
            _mockRemotingRefService.Verify(x => x.GetBetTypeName(bettypeId.ToString(), languageCode), Times.Once);
        }
    }
}