using Fanex.Athena.Report.Common.Helpers;
using System;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Helpers
{
    public class DateTimeHelperTest
    {
        [Fact]
        public void Now_WhenCalled_ReturnsCurrentDateTime()
        {
            // Arrange
            var expectedTime = DateTimeOffset.Now.DateTime;

            // Act
            var result = DateTimeHelper.Now;

            // Assert
            // Allow for small time difference due to execution time
            Assert.True(Math.Abs((result - expectedTime).TotalSeconds) < 1);
        }

        [Fact]
        public void Now_WithCustomProvider_ReturnsCustomDateTime()
        {
            // Arrange
            var customDateTime = new DateTime(2023, 5, 15, 10, 30, 45);
            var originalProvider = DateTimeHelper.DateTimeOffsetProvider;
            DateTimeHelper.DateTimeOffsetProvider = () => new DateTimeOffset(customDateTime);

            try
            {
                // Act
                var result = DateTimeHelper.Now;

                // Assert
                Assert.Equal(customDateTime, result);
            }
            finally
            {
                // Cleanup
                DateTimeHelper.DateTimeOffsetProvider = originalProvider;
            }
        }

        [Fact]
        public void MinValue_WhenCalled_ReturnsMinDateTime()
        {
            // Act
            var result = DateTimeHelper.MinValue;

            // Assert
            Assert.Equal(DateTimeOffset.MinValue.DateTime, result);
        }

        [Theory]
        [InlineData("2023-05-15", "2023-05-20", "2023-05-20")]
        [InlineData("2023-12-31", "2023-01-01", "2023-12-31")]
        [InlineData("2023-05-15", "2023-05-15", "2023-05-15")]
        public void GetMaxDate_WithTwoDates_ReturnsLaterDate(string date1Str, string date2Str, string expectedStr)
        {
            // Arrange
            var date1 = DateTime.Parse(date1Str);
            var date2 = DateTime.Parse(date2Str);
            var expected = DateTime.Parse(expectedStr);

            // Act
            var result = DateTimeHelper.GetMaxDate(date1, date2);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2023-05-15", "2023-05-20", "2023-05-15")]
        [InlineData("2023-12-31", "2023-01-01", "2023-01-01")]
        [InlineData("2023-05-15", "2023-05-15", "2023-05-15")]
        public void GetMinDate_WithTwoDates_ReturnsEarlierDate(string date1Str, string date2Str, string expectedStr)
        {
            // Arrange
            var date1 = DateTime.Parse(date1Str);
            var date2 = DateTime.Parse(date2Str);
            var expected = DateTime.Parse(expectedStr);

            // Act
            var result = DateTimeHelper.GetMinDate(date1, date2);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2023, 5, 15, 0, 0, 0)]
        [InlineData(2023, 12, 31, 23, 59, 59)]
        [InlineData(2000, 1, 1, 12, 30, 45)]
        public void CreateDateTime_WithValidParameters_ReturnsCorrectDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            // Arrange
            var expected = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero).DateTime;

            // Act
            var result = DateTimeHelper.CreateDateTime(year, month, day, hour, minute, second);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2023, 5, 15)]
        [InlineData(2023, 12, 31)]
        [InlineData(2000, 1, 1)]
        public void CreateDateTime_WithDateOnly_ReturnsDateTimeAtMidnight(int year, int month, int day)
        {
            // Arrange
            var expected = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero).DateTime;

            // Act
            var result = DateTimeHelper.CreateDateTime(year, month, day);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2023-05-15", DayOfWeek.Monday, "2023-05-15")] // Monday itself
        [InlineData("2023-05-16", DayOfWeek.Monday, "2023-05-15")] // Tuesday -> previous Monday
        [InlineData("2023-05-17", DayOfWeek.Monday, "2023-05-15")] // Wednesday -> previous Monday
        [InlineData("2023-05-18", DayOfWeek.Monday, "2023-05-15")] // Thursday -> previous Monday
        [InlineData("2023-05-19", DayOfWeek.Monday, "2023-05-15")] // Friday -> previous Monday
        [InlineData("2023-05-20", DayOfWeek.Monday, "2023-05-15")] // Saturday -> previous Monday
        [InlineData("2023-05-21", DayOfWeek.Monday, "2023-05-15")] // Sunday -> previous Monday
        public void GetFirstDateOfWeek_WithMondayStart_ReturnsCorrectMonday(string inputDateStr, DayOfWeek startOfWeek, string expectedDateStr)
        {
            // Arrange
            var inputDate = DateTime.Parse(inputDateStr);
            var expectedDate = DateTime.Parse(expectedDateStr).Date;

            // Act
            var result = DateTimeHelper.GetFirstDateOfWeek(inputDate, startOfWeek);

            // Assert
            Assert.Equal(expectedDate, result);
        }

        [Theory]
        [InlineData("2023-05-14", DayOfWeek.Sunday, "2023-05-14")] // Sunday itself
        [InlineData("2023-05-15", DayOfWeek.Sunday, "2023-05-14")] // Monday -> previous Sunday
        [InlineData("2023-05-16", DayOfWeek.Sunday, "2023-05-14")] // Tuesday -> previous Sunday
        [InlineData("2023-05-17", DayOfWeek.Sunday, "2023-05-14")] // Wednesday -> previous Sunday
        [InlineData("2023-05-18", DayOfWeek.Sunday, "2023-05-14")] // Thursday -> previous Sunday
        [InlineData("2023-05-19", DayOfWeek.Sunday, "2023-05-14")] // Friday -> previous Sunday
        [InlineData("2023-05-20", DayOfWeek.Sunday, "2023-05-14")] // Saturday -> previous Sunday
        public void GetFirstDateOfWeek_WithSundayStart_ReturnsCorrectSunday(string inputDateStr, DayOfWeek startOfWeek, string expectedDateStr)
        {
            // Arrange
            var inputDate = DateTime.Parse(inputDateStr);
            var expectedDate = DateTime.Parse(expectedDateStr).Date;

            // Act
            var result = DateTimeHelper.GetFirstDateOfWeek(inputDate, startOfWeek);

            // Assert
            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void GetFirstDateOfWeek_DefaultStartOfWeek_UsesMonday()
        {
            // Arrange
            var inputDate = new DateTime(2023, 5, 17); // Wednesday
            var expectedDate = new DateTime(2023, 5, 15).Date; // Previous Monday

            // Act
            var result = DateTimeHelper.GetFirstDateOfWeek(inputDate);

            // Assert
            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void GetFirstDateOfWeek_ReturnsDateOnly()
        {
            // Arrange
            var inputDate = new DateTime(2023, 5, 17, 15, 30, 45); // Wednesday with time

            // Act
            var result = DateTimeHelper.GetFirstDateOfWeek(inputDate);

            // Assert
            Assert.Equal(TimeSpan.Zero, result.TimeOfDay);
        }
    }
}
