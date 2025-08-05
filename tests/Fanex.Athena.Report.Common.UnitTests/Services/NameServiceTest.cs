using AutoFixture;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    public class NameServiceTest
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<ICacheRefAdapter> _mockCacheRefAdapter;

        public NameServiceTest()
        {
            _mockCacheRefAdapter = new Mock<ICacheRefAdapter>();
        }

        [Fact]
        public void GetBetTypeName_WithValidBetTypeId_ReturnsNameWithSuffix()
        {
            // Arrange
            const int betTypeId = 123;
            const string expectedBaseName = "Base Bet Type";
            const string expectedSuffix = "";
            const string expected = expectedBaseName + expectedSuffix;
            
            _mockCacheRefAdapter.Setup(c => c.GetBettype(betTypeId, false)).Returns(expectedBaseName);
            
            var service = CreateService();

            // Act
            var result = service.GetBetTypeName(betTypeId);

            // Assert
            Assert.Equal(expected, result);
            _mockCacheRefAdapter.Verify(c => c.GetBettype(betTypeId, false), Times.Once);
        }

        [Fact]
        public void GetBetTypeName_WithHideSabaFlag_CallsCacheWithCorrectFlag()
        {
            // Arrange
            const int betTypeId = 123;
            const string expectedName = "Hidden Saba Bet Type";
            
            _mockCacheRefAdapter.Setup(c => c.GetBettype(betTypeId, true)).Returns(expectedName);
            
            var service = CreateService();

            // Act
            var result = service.GetBetTypeName(betTypeId, true);

            // Assert
            Assert.Equal(expectedName, result);
            _mockCacheRefAdapter.Verify(c => c.GetBettype(betTypeId, true), Times.Once);
        }

        [Fact]
        public void GetSportName_WithPositiveSportTypeId_ReturnsCachedSportName()
        {
            // Arrange
            const int sportTypeId = 1;
            const string lang = "EN";
            const string expected = "Football";
            
            _mockCacheRefAdapter.Setup(c => c.GetSportName(sportTypeId, lang)).Returns(expected);
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId, false, lang);

            // Assert
            Assert.Equal(expected, result);
            _mockCacheRefAdapter.Verify(c => c.GetSportName(sportTypeId, lang), Times.Once);
        }

        [Fact]
        public void GetSportName_WithParlaySportType_ReturnsParlayText()
        {
            // Arrange
            const int sportTypeId = HardCodeSportType.Parlay;
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId);

            // Assert
            Assert.Equal("Parlay", result);
        }

        [Fact]
        public void GetSportName_WithSabaSoccer_ReturnsSabaPrefixedName()
        {
            // Arrange
            const int sportTypeId = HardCodeSportType.SabaSoccer;
            const string lang = "EN";
            const string baseSportName = "Soccer";
            const string expected = "Saba Soccer";
            
            _mockCacheRefAdapter.Setup(c => c.GetSportName(SportType.Soccer, lang)).Returns(baseSportName);
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId, false, lang);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetSportName_WithSabaSoccerAndHideSaba_ReturnsVirtualPrefixedName()
        {
            // Arrange
            const int sportTypeId = HardCodeSportType.SabaSoccer;
            const string lang = "EN";
            const string baseSportName = "Soccer";
            const string expected = "Virtual Soccer";
            
            _mockCacheRefAdapter.Setup(c => c.GetSportName(SportType.Soccer, lang)).Returns(baseSportName);
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId, true, lang);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetSportName_WithSabaBasketball_ReturnsSabaPrefixedName()
        {
            // Arrange
            const int sportTypeId = HardCodeSportType.SabaBasketball;
            const string lang = "EN";
            const string baseSportName = "Basketball";
            const string expected = "Saba Basketball";
            
            _mockCacheRefAdapter.Setup(c => c.GetSportName(SportType.BasketBall, lang)).Returns(baseSportName);
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId, false, lang);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetSportName_WithUnknownSabaSportType_ReturnsStringRepresentation()
        {
            // Arrange
            const int sportTypeId = -999; // Unknown Saba sport type
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId);

            // Assert
            Assert.Equal(sportTypeId.ToString(), result);
        }

        [Fact]
        public void GetBetTypeName_WithNumberGameHappy5OddEven_ReturnsSuffixedName()
        {
            // Arrange
            const int betTypeId = BetType.NumberGameHappy5OddEven;
            const string baseName = "Odd/Even";
            const string sportName = "Happy5";
            const string expected = baseName + " (" + sportName + ")";
            
            _mockCacheRefAdapter.Setup(c => c.GetBettype(betTypeId, false)).Returns(baseName);
            _mockCacheRefAdapter.Setup(c => c.GetSportName(SportType.NumberGameHappy5, It.IsAny<string>())).Returns(sportName);
            
            var service = CreateService();

            // Act
            var result = service.GetBetTypeName(betTypeId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetBetTypeName_WithNegativeBetTypeId_ReturnsResultFromCache()
        {
            // Arrange
            const int betTypeId = -1;
            const string expectedName = "Invalid Bet Type";
            
            _mockCacheRefAdapter.Setup(c => c.GetBettype(betTypeId, false)).Returns(expectedName);
            
            var service = CreateService();

            // Act
            var result = service.GetBetTypeName(betTypeId);

            // Assert
            Assert.Equal(expectedName, result);
        }

        [Fact]
        public void GetSportName_WithNullLanguage_UsesDefaultLanguage()
        {
            // Arrange
            const int sportTypeId = 1;
            const string expected = "Football";
            
            _mockCacheRefAdapter.Setup(c => c.GetSportName(sportTypeId, It.IsAny<string>())).Returns(expected);
            
            var service = CreateService();

            // Act
            var result = service.GetSportName(sportTypeId, false, null);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetBetTypeName_WhenCacheReturnsNull_ReturnsEmptyString()
        {
            // Arrange
            const int betTypeId = 123;
            
            _mockCacheRefAdapter.Setup(c => c.GetBettype(betTypeId, false)).Returns((string)null);
            
            var service = CreateService();

            // Act
            var result = service.GetBetTypeName(betTypeId);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        private NameService CreateService()
        {
            return new NameService(_mockCacheRefAdapter.Object);
        }
    }
}