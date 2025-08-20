namespace Fanex.Athena.Report.Common.UnitTests.Helpers
{
    using Common.Helpers;
    using Resources.Constants;
    using Xunit;

    public class SportHelperTest
    {
        [Theory]
        [InlineData(SportType.VirtualHorseRacing, true)]
        [InlineData(SportType.VirtualDogRacing, true)]
        [InlineData(SportType.VirtualSteepleChase, true)]
        [InlineData(SportType.VirtualHarnessRacing, true)]
        [InlineData(SportType.VirtualMotorRacing, true)]
        [InlineData(SportType.VirtualGreyhound, false)]
        [InlineData(SportType.VirtualMotoRacing, false)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void IsVSNewRacingSportTypes_WithVariousSportTypes_ReturnsExpectedResult(int sportType, bool expected)
        {
            // Act
            var result = SportHelper.IsVSNewRacingSportTypes(sportType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SportType.BitCoin, true)]
        [InlineData(SportType.VirtualHorseRacing, true)]
        [InlineData(SportType.VirtualDogRacing, true)]
        [InlineData(SportType.BetbyBasketball, true)]
        [InlineData(SportType.VirtuallySports, true)]
        [InlineData(SportType.VirtualSteepleChase, true)]
        [InlineData(SportType.VirtualHarnessRacing, true)]
        [InlineData(SportType.VirtualMotorRacing, true)]
        [InlineData(SportType.VirtualGreyhound, false)]
        [InlineData(SportType.VirtualMotoRacing, false)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void IsLeague3rdSports_WithVariousSportTypes_ReturnsExpectedResult(int sportType, bool expected)
        {
            // Act
            var result = SportHelper.IsLeague3rdSports(sportType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SportType.BetbyBasketball, true)]
        [InlineData(SportType.VirtuallySports, true)]
        [InlineData(SportType.VirtualSteepleChase, true)]
        [InlineData(SportType.VirtualHarnessRacing, true)]
        [InlineData(SportType.VirtualMotorRacing, true)]
        [InlineData(SportType.VirtualHorseRacing, false)]
        [InlineData(SportType.VirtualDogRacing, false)]
        [InlineData(SportType.BitCoin, false)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void IsMatch3rdSports_WithVariousSportTypes_ReturnsExpectedResult(int sportType, bool expected)
        {
            // Act
            var result = SportHelper.IsMatch3rdSports(sportType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SportType.VirtualGreyhound, true)]
        [InlineData(SportType.VirtualMotoRacing, true)]
        [InlineData(SportType.VirtualSpeedway, true)]
        [InlineData(SportType.VirtualCycling, true)]
        [InlineData(SportType.VirtualHorseRacing, false)]
        [InlineData(SportType.VirtualDogRacing, false)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void IsVSSportTypes_WithVariousSportTypes_ReturnsExpectedResult(int sportType, bool expected)
        {
            // Act
            var result = SportHelper.IsVSSportTypes(sportType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SportTypeCategories_HaveCorrectOverlaps()
        {
            // Test overlapping sport types between categories

            // VirtualSteepleChase, VirtualHarnessRacing, VirtualMotorRacing should be in both VSNewRacing and Match3rd
            Assert.True(SportHelper.IsVSNewRacingSportTypes(SportType.VirtualSteepleChase));
            Assert.True(SportHelper.IsMatch3rdSports(SportType.VirtualSteepleChase));

            Assert.True(SportHelper.IsVSNewRacingSportTypes(SportType.VirtualHarnessRacing));
            Assert.True(SportHelper.IsMatch3rdSports(SportType.VirtualHarnessRacing));

            Assert.True(SportHelper.IsVSNewRacingSportTypes(SportType.VirtualMotorRacing));
            Assert.True(SportHelper.IsMatch3rdSports(SportType.VirtualMotorRacing));

            // VirtualHorseRacing, VirtualDogRacing should be in both VSNewRacing and League3rd
            Assert.True(SportHelper.IsVSNewRacingSportTypes(SportType.VirtualHorseRacing));
            Assert.True(SportHelper.IsLeague3rdSports(SportType.VirtualHorseRacing));

            Assert.True(SportHelper.IsVSNewRacingSportTypes(SportType.VirtualDogRacing));
            Assert.True(SportHelper.IsLeague3rdSports(SportType.VirtualDogRacing));

            // BetbyBasketball, VirtuallySports should be in both Match3rd and League3rd
            Assert.True(SportHelper.IsMatch3rdSports(SportType.BetbyBasketball));
            Assert.True(SportHelper.IsLeague3rdSports(SportType.BetbyBasketball));

            Assert.True(SportHelper.IsMatch3rdSports(SportType.VirtuallySports));
            Assert.True(SportHelper.IsLeague3rdSports(SportType.VirtuallySports));
        }

        [Fact]
        public void SportTypeCategories_HaveUniqueTypes()
        {
            // Test sport types that should only belong to one category

            // BitCoin should only be in League3rd
            Assert.False(SportHelper.IsVSNewRacingSportTypes(SportType.BitCoin));
            Assert.False(SportHelper.IsMatch3rdSports(SportType.BitCoin));
            Assert.False(SportHelper.IsVSSportTypes(SportType.BitCoin));
            Assert.True(SportHelper.IsLeague3rdSports(SportType.BitCoin));

            // VirtualGreyhound should only be in VSSportTypes
            Assert.False(SportHelper.IsVSNewRacingSportTypes(SportType.VirtualGreyhound));
            Assert.False(SportHelper.IsMatch3rdSports(SportType.VirtualGreyhound));
            Assert.False(SportHelper.IsLeague3rdSports(SportType.VirtualGreyhound));
            Assert.True(SportHelper.IsVSSportTypes(SportType.VirtualGreyhound));
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(99999)]
        public void AllMethods_WithInvalidSportTypes_ReturnFalse(int sportType)
        {
            // Act & Assert
            Assert.False(SportHelper.IsVSNewRacingSportTypes(sportType));
            Assert.False(SportHelper.IsLeague3rdSports(sportType));
            Assert.False(SportHelper.IsMatch3rdSports(sportType));
            Assert.False(SportHelper.IsVSSportTypes(sportType));
        }
    }
}