namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Models.DTOs;
    using Xunit;

    public class RiskLevelTest
    {
        [Fact]
        public void RiskLevel_Properties_GetSetCorrectly()
        {
            // Arrange
            var riskLevel = new RiskLevel();
            const string expectedId = "1";
            const string expectedKey = "VIP";
            const string expectedName = "VIP Customer";
            var expectedCustomerClassList = new[] { "1", "2", "3" };

            // Act
            riskLevel.Id = expectedId;
            riskLevel.Key = expectedKey;
            riskLevel.Name = expectedName;
            riskLevel.CustomerClassList = expectedCustomerClassList;

            // Assert
            Assert.Equal(expectedId, riskLevel.Id);
            Assert.Equal(expectedKey, riskLevel.Key);
            Assert.Equal(expectedName, riskLevel.Name);
            Assert.Equal(expectedCustomerClassList, riskLevel.CustomerClassList);
        }

        [Fact]
        public void RiskLevel_DefaultCustomerClassList_IsEmpty()
        {
            // Arrange & Act
            var riskLevel = new RiskLevel();

            // Assert
            Assert.NotNull(riskLevel.CustomerClassList);
            Assert.Empty(riskLevel.CustomerClassList);
        }

        [Theory]
        [InlineData("VIP", 1)]
        [InlineData("Healthy", 2)]
        [InlineData("New", 3)]
        [InlineData("Moderate", 4)]
        [InlineData("Suspicious", 5)]
        [InlineData("Dangerous", 6)]
        [InlineData("Problems", 7)]
        [InlineData("Robot", 8)]
        [InlineData("Uncategorized", 999)]
        public void GetComparerValue_WithValidKeys_ReturnsCorrectValue(string key, int expected)
        {
            // Arrange
            var riskLevel = new RiskLevel { Key = key };

            // Act
            var result = RiskLevel.GetComparerValue(riskLevel);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("vip", 1)]
        [InlineData("HEALTHY", 2)]
        [InlineData("MoDeRaTe", 4)]
        public void GetComparerValue_WithDifferentCasing_ReturnsCorrectValue(string key, int expected)
        {
            // Arrange
            var riskLevel = new RiskLevel { Key = key };

            // Act
            var result = RiskLevel.GetComparerValue(riskLevel);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Unknown")]
        [InlineData("Invalid")]
        [InlineData("")]
        // [InlineData(null)]
        public void GetComparerValue_WithInvalidKeys_ReturnsDefaultValue(string key)
        {
            // Arrange
            var riskLevel = new RiskLevel { Key = key };
            const int expectedDefaultValue = 99;

            // Act
            var result = RiskLevel.GetComparerValue(riskLevel);

            // Assert
            Assert.Equal(expectedDefaultValue, result);
        }

        [Fact]
        public void UncategorizedConstant_HasCorrectValue()
        {
            // Act & Assert
            Assert.Equal("Uncategorized", RiskLevel.Uncategorized);
        }
    }

    public class RiskLevelComparerTest
    {
        private readonly RiskLevelComparer _comparer;

        public RiskLevelComparerTest()
        {
            _comparer = new RiskLevelComparer();
        }

        [Fact]
        public void Compare_WhenXLessThanY_ReturnsNegativeOne()
        {
            // Arrange
            var vip = new RiskLevel { Key = "VIP" };      // Value: 1
            var healthy = new RiskLevel { Key = "Healthy" }; // Value: 2

            // Act
            var result = _comparer.Compare(vip, healthy);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Compare_WhenXGreaterThanY_ReturnsOne()
        {
            // Arrange
            var dangerous = new RiskLevel { Key = "Dangerous" }; // Value: 6
            var moderate = new RiskLevel { Key = "Moderate" };   // Value: 4

            // Act
            var result = _comparer.Compare(dangerous, moderate);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_WhenXEqualsY_ReturnsZero()
        {
            // Arrange
            var vip1 = new RiskLevel { Key = "VIP" };
            var vip2 = new RiskLevel { Key = "vip" }; // Case insensitive

            // Act
            var result = _comparer.Compare(vip1, vip2);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void Compare_WithUnknownRiskLevels_UsesDefaultValue()
        {
            // Arrange
            var unknown1 = new RiskLevel { Key = "Unknown1" }; // Both should get default value 99
            var unknown2 = new RiskLevel { Key = "Unknown2" };

            // Act
            var result = _comparer.Compare(unknown1, unknown2);

            // Assert
            Assert.Equal(0, result); // Both get default value 99, so they're equal
        }

        [Fact]
        public void Compare_KnownVsUnknown_KnownIsLess()
        {
            // Arrange
            var vip = new RiskLevel { Key = "VIP" };      // Value: 1
            var unknown = new RiskLevel { Key = "Unknown" }; // Value: 99 (default)

            // Act
            var result = _comparer.Compare(vip, unknown);

            // Assert
            Assert.Equal(-1, result); // VIP (1) < Unknown (99)
        }

        [Fact]
        public void Compare_UncategorizedVsOthers_UncategorizedIsLast()
        {
            // Arrange
            var uncategorized = new RiskLevel { Key = "Uncategorized" }; // Value: 999
            var dangerous = new RiskLevel { Key = "Dangerous" };         // Value: 6

            // Act
            var result = _comparer.Compare(uncategorized, dangerous);

            // Assert
            Assert.Equal(1, result); // Uncategorized (999) > Dangerous (6)
        }

        [Fact]
        public void RiskLevelComparer_CanSortCollectionCorrectly()
        {
            // Arrange
            var riskLevels = new List<RiskLevel>
            {
                new RiskLevel { Key = "Uncategorized" },
                new RiskLevel { Key = "Dangerous" },
                new RiskLevel { Key = "VIP" },
                new RiskLevel { Key = "Healthy" },
                new RiskLevel { Key = "Unknown" },
                new RiskLevel { Key = "Moderate" }
            };

            // Act
            var sortedRiskLevels = riskLevels.OrderBy(r => r, _comparer).ToList();

            // Assert
            Assert.Equal("VIP", sortedRiskLevels[0].Key);        // 1
            Assert.Equal("Healthy", sortedRiskLevels[1].Key);    // 2
            Assert.Equal("Moderate", sortedRiskLevels[2].Key);   // 4
            Assert.Equal("Dangerous", sortedRiskLevels[3].Key);  // 6
            Assert.Equal("Unknown", sortedRiskLevels[4].Key);    // 99 (default)
            Assert.Equal("Uncategorized", sortedRiskLevels[5].Key); // 999
        }
    }
}