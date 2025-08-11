namespace Fanex.Athena.Report.Core.UnitTests.Helpers
{
    using Core.Helpers;
    using Xunit;

    public class SqlHelperTest
    {
        [Fact]
        public void ToSqlTextSearch_WithNormalText_ReturnsSameText()
        {
            // Arrange
            const string input = "normal text";
            const string expected = "normal text";

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToSqlTextSearch_WithUnderscores_EscapesUnderscores()
        {
            // Arrange
            const string input = "text_with_underscores";
            const string expected = "text[_]with[_]underscores";

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToSqlTextSearch_WithLeadingAndTrailingSpaces_TrimsSpaces()
        {
            // Arrange
            const string input = "  text with spaces  ";
            const string expected = "text with spaces";

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToSqlTextSearch_WithUnderscoresAndSpaces_TrimsAndEscapes()
        {
            // Arrange
            const string input = "  text_with_underscores  ";
            const string expected = "text[_]with[_]underscores";

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToSqlTextSearch_WithNullInput_ReturnsNull()
        {
            // Arrange
            string input = null;

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToSqlTextSearch_WithEmptyString_ReturnsEmptyString()
        {
            // Arrange
            const string input = "";
            const string expected = "";

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToSqlTextSearch_WithOnlySpaces_ReturnsEmptyString()
        {
            // Arrange
            const string input = "   ";
            const string expected = "";

            // Act
            var result = SqlHelper.ToSqlTextSearch(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}