namespace Fanex.Athena.Report.Common.UnitTests.Helpers
{
    using Common.Helpers;
    using Xunit;

    public class BetTypeHelperTest
    {
        [Theory]
        [InlineData(81, true)]   // Min range
        [InlineData(119, true)]  // Max range
        [InlineData(100, true)]  // Middle range
        [InlineData(80, false)]  // Below min
        [InlineData(120, false)] // Above max
        [InlineData(0, false)]   // Zero
        [InlineData(-1, false)]  // Negative
        public void IsNumberGame_WithVariousBetTypes_ReturnsExpectedResult(int bettype, bool expected)
        {
            // Act
            var result = BetTypeHelper.IsNumberGame(bettype);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(8101, true)]  // NGHappy5 Min range
        [InlineData(8120, true)]  // NGHappy5 Max range
        [InlineData(8110, true)]  // NGHappy5 Middle range
        [InlineData(3601, true)]  // Happy5LinkedGames Min range
        [InlineData(3610, true)]  // Happy5LinkedGames Max range
        [InlineData(3605, true)]  // Happy5LinkedGames Middle range
        [InlineData(8100, false)] // Below NGHappy5 min
        [InlineData(8121, false)] // Above NGHappy5 max
        [InlineData(3600, false)] // Below Happy5LinkedGames min
        [InlineData(3611, false)] // Above Happy5LinkedGames max
        [InlineData(100, false)]  // Regular number game range
        [InlineData(0, false)]    // Zero
        [InlineData(-1, false)]   // Negative
        public void IsNumberGameHappy5_WithVariousBetTypes_ReturnsExpectedResult(int bettype, bool expected)
        {
            // Act
            var result = BetTypeHelper.IsNumberGameHappy5(bettype);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1201, true)]  // Min range
        [InlineData(1299, true)]  // Max range
        [InlineData(1250, true)]  // Middle range
        [InlineData(1200, false)] // Below min
        [InlineData(1300, false)] // Above max
        [InlineData(100, false)]  // Different range
        [InlineData(0, false)]    // Zero
        [InlineData(-1, false)]   // Negative
        public void IsVirtualSports_WithVariousBetTypes_ReturnsExpectedResult(int bettype, bool expected)
        {
            // Act
            var result = BetTypeHelper.IsVirtualSports(bettype);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2700, true)]  // Min range
        [InlineData(2899, true)]  // Max range
        [InlineData(2800, true)]  // Middle range
        [InlineData(2699, false)] // Below min
        [InlineData(2900, false)] // Above max
        [InlineData(1250, false)] // VS1 range
        [InlineData(100, false)]  // Different range
        [InlineData(0, false)]    // Zero
        [InlineData(-1, false)]   // Negative
        public void IsVirtualSports2_WithVariousBetTypes_ReturnsExpectedResult(int bettype, bool expected)
        {
            // Act
            var result = BetTypeHelper.IsVirtualSports2(bettype);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void BetTypeRanges_DoNotOverlap()
        {
            // Test that different bet type ranges don't overlap
            var numberGameBetType = 100;
            var happy5BetType = 8110;
            var virtualSportsBetType = 1250;
            var virtualSports2BetType = 2800;

            // Assert each bet type is only true for its specific category
            Assert.True(BetTypeHelper.IsNumberGame(numberGameBetType));
            Assert.False(BetTypeHelper.IsNumberGameHappy5(numberGameBetType));
            Assert.False(BetTypeHelper.IsVirtualSports(numberGameBetType));
            Assert.False(BetTypeHelper.IsVirtualSports2(numberGameBetType));

            Assert.False(BetTypeHelper.IsNumberGame(happy5BetType));
            Assert.True(BetTypeHelper.IsNumberGameHappy5(happy5BetType));
            Assert.False(BetTypeHelper.IsVirtualSports(happy5BetType));
            Assert.False(BetTypeHelper.IsVirtualSports2(happy5BetType));

            Assert.False(BetTypeHelper.IsNumberGame(virtualSportsBetType));
            Assert.False(BetTypeHelper.IsNumberGameHappy5(virtualSportsBetType));
            Assert.True(BetTypeHelper.IsVirtualSports(virtualSportsBetType));
            Assert.False(BetTypeHelper.IsVirtualSports2(virtualSportsBetType));

            Assert.False(BetTypeHelper.IsNumberGame(virtualSports2BetType));
            Assert.False(BetTypeHelper.IsNumberGameHappy5(virtualSports2BetType));
            Assert.False(BetTypeHelper.IsVirtualSports(virtualSports2BetType));
            Assert.True(BetTypeHelper.IsVirtualSports2(virtualSports2BetType));
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void AllMethods_WithExtremeValues_ReturnFalse(int bettype)
        {
            // Act & Assert
            Assert.False(BetTypeHelper.IsNumberGame(bettype));
            Assert.False(BetTypeHelper.IsNumberGameHappy5(bettype));
            Assert.False(BetTypeHelper.IsVirtualSports(bettype));
            Assert.False(BetTypeHelper.IsVirtualSports2(bettype));
        }
    }
}
