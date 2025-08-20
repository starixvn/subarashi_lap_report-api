namespace Fanex.Athena.Report.Common.UnitTests.Helpers
{
    using System;
    using Common.Helpers;
    using Xunit;

    public class ConvertHelperTest
    {
        [Theory]
        [InlineData("20230515", false, 2023, 5, 15)]
        [InlineData("20231231", false, 2023, 12, 31)]
        [InlineData("20000101", false, 2000, 1, 1)]
        [InlineData("20230228", false, 2023, 2, 28)]
        public void ToDateTime_WithDailyFormat_ReturnsCorrectDateTime(string date, bool isMonthly, int expectedYear, int expectedMonth, int expectedDay)
        {
            // Arrange
            var expected = new DateTime(expectedYear, expectedMonth, expectedDay);

            // Act
            var result = ConvertHelper.ToDateTime(date, isMonthly);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("202305", true, 2023, 5, 1)]
        [InlineData("202312", true, 2023, 12, 1)]
        [InlineData("200001", true, 2000, 1, 1)]
        [InlineData("202302", true, 2023, 2, 1)]
        public void ToDateTime_WithMonthlyFormat_ReturnsFirstDayOfMonth(string date, bool isMonthly, int expectedYear, int expectedMonth, int expectedDay)
        {
            // Arrange
            var expected = new DateTime(expectedYear, expectedMonth, expectedDay);

            // Act
            var result = ConvertHelper.ToDateTime(date, isMonthly);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("20230229", false)] // Invalid date - not a leap year
        [InlineData("20230431", false)] // Invalid date - April has 30 days
        [InlineData("20231301", false)] // Invalid month
        [InlineData("20230132", false)] // Invalid day
        public void ToDateTime_WithInvalidDate_ThrowsException(string date, bool isMonthly)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ConvertHelper.ToDateTime(date, isMonthly));
        }

        [Theory]
        [InlineData("20240229", false)] // Valid leap year date
        [InlineData("20200229", false)] // Valid leap year date
        public void ToDateTime_WithValidLeapYearDate_ReturnsCorrectDateTime(string date, bool isMonthly)
        {
            // Arrange
            var year = Convert.ToInt32(date.Substring(0, 4));
            var month = Convert.ToInt32(date.Substring(4, 2));
            var day = Convert.ToInt32(date.Substring(6, 2));
            var expected = new DateTime(year, month, day);

            // Act
            var result = ConvertHelper.ToDateTime(date, isMonthly);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void ToDateTime_WithNullOrEmptyString_ThrowsException(string date, bool isMonthly)
        {
            // Act & Assert
            Assert.ThrowsAny<Exception>(() => ConvertHelper.ToDateTime(date, isMonthly));
        }

        [Fact]
        public void ToDateTime_MonthlyWithExtraDigits_IgnoresExtraDigits()
        {
            // Arrange
            var date = "20230515"; // Has day digits but isMonthly = true
            var expected = new DateTime(2023, 5, 1);

            // Act
            var result = ConvertHelper.ToDateTime(date, true);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}