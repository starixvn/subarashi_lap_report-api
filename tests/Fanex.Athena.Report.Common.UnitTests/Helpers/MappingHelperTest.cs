namespace Fanex.Athena.Report.Common.UnitTests.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers;
    using Resources.Constants;
    using Xunit;

    public class MappingHelperTest
    {
        [Theory]
        [InlineData(1204, SportType.Soccer)]
        [InlineData(1235, SportType.Tennis)]
        [InlineData(8102, SportType.NumberGameHappy5)]
        public void MapBettypeSportBySport_WithValidBetType_ReturnsCorrectSportType(int betType, int expectedSportType)
        {
            // Act
            var result = MappingHelper.MapBettypeSportBySport.TryGetValue(betType, out var sportType);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedSportType, sportType);
        }

        [Theory]
        [InlineData(9999)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1203)]
        public void MapBettypeSportBySport_WithInvalidBetType_ReturnsFalse(int betType)
        {
            // Act
            var result = MappingHelper.MapBettypeSportBySport.TryGetValue(betType, out var sportType);

            // Assert
            Assert.False(result);
            Assert.Equal(0, sportType);
        }

        [Theory]
        [InlineData(1235, SportType.Tennis)]
        [InlineData(8102, SportType.NumberGameHappy5)]
        [InlineData(2803, SportType.VirtualBasketball)]
        [InlineData(2804, SportType.VirtualBasketball)]
        [InlineData(2805, SportType.VirtualBasketball)]
        [InlineData(2806, SportType.VirtualBasketball)]
        public void MapBettypeSportByProduct_WithValidBetType_ReturnsCorrectSportType(int betType, int expectedSportType)
        {
            // Act
            var result = MappingHelper.MapBettypeSportByProduct.TryGetValue(betType, out var sportType);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedSportType, sportType);
        }

        [Theory]
        [InlineData(9999)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1204)] // This exists in MapBettypeSportBySport but not in MapBettypeSportByProduct
        public void MapBettypeSportByProduct_WithInvalidBetType_ReturnsFalse(int betType)
        {
            // Act
            var result = MappingHelper.MapBettypeSportByProduct.TryGetValue(betType, out var sportType);

            // Assert
            Assert.False(result);
            Assert.Equal(0, sportType);
        }

        [Fact]
        public void MapBettypeSportBySport_IsReadOnly()
        {
            // Act & Assert
            Assert.IsAssignableFrom<IReadOnlyDictionary<int, int>>(MappingHelper.MapBettypeSportBySport);
        }

        [Fact]
        public void MapBettypeSportByProduct_IsReadOnly()
        {
            // Act & Assert
            Assert.IsAssignableFrom<IReadOnlyDictionary<int, int>>(MappingHelper.MapBettypeSportByProduct);
        }

        [Fact]
        public void MapBettypeSportBySport_HasExpectedCount()
        {
            // Act
            var count = MappingHelper.MapBettypeSportBySport.Count;

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public void MapBettypeSportByProduct_HasExpectedCount()
        {
            // Act
            var count = MappingHelper.MapBettypeSportByProduct.Count;

            // Assert
            Assert.Equal(6, count);
        }

        [Fact]
        public void BothMappings_ShareCommonEntries()
        {
            // Arrange
            var commonBetTypes = new[] { 1235, 8102 };

            // Act & Assert
            foreach (var betType in commonBetTypes)
            {
                Assert.True(MappingHelper.MapBettypeSportBySport.ContainsKey(betType));
                Assert.True(MappingHelper.MapBettypeSportByProduct.ContainsKey(betType));
                
                // Verify they map to the same sport type
                var sportTypeBySport = MappingHelper.MapBettypeSportBySport[betType];
                var sportTypeByProduct = MappingHelper.MapBettypeSportByProduct[betType];
                Assert.Equal(sportTypeBySport, sportTypeByProduct);
            }
        }

        [Fact]
        public void MapBettypeSportBySport_ContainsUniqueKeys()
        {
            // Act
            var keys = MappingHelper.MapBettypeSportBySport.Keys.ToList();
            var distinctKeys = keys.Distinct().ToList();

            // Assert
            Assert.Equal(keys.Count, distinctKeys.Count);
        }

        [Fact]
        public void MapBettypeSportByProduct_ContainsUniqueKeys()
        {
            // Act
            var keys = MappingHelper.MapBettypeSportByProduct.Keys.ToList();
            var distinctKeys = keys.Distinct().ToList();

            // Assert
            Assert.Equal(keys.Count, distinctKeys.Count);
        }

        [Fact]
        public void VirtualBasketballBetTypes_AllMapToSameSportType()
        {
            // Arrange
            var virtualBasketballBetTypes = new[] { 2803, 2804, 2805, 2806 };

            // Act & Assert
            foreach (var betType in virtualBasketballBetTypes)
            {
                Assert.True(MappingHelper.MapBettypeSportByProduct.TryGetValue(betType, out var sportType));
                Assert.Equal(SportType.VirtualBasketball, sportType);
            }
        }
    }
}
