using Fanex.Athena.Report.Core.Models.DTOs;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    public class ShowingOptionTest
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Act
            var showingOption = new ShowingOption();

            // Assert
            Assert.True(showingOption.GrandTotalVisible);
            Assert.False(showingOption.NGVisible);
            Assert.False(showingOption.BAVisible);
            Assert.False(showingOption.KNVisible);
        }

        [Fact]
        public void Constructor_OtherPropertiesHaveDefaultFalseValue()
        {
            // Act
            var showingOption = new ShowingOption();

            // Assert
            Assert.False(showingOption.SBVisible);
            Assert.False(showingOption.VSVisible);
            Assert.False(showingOption.CustomerCommVisible);
        }

        [Fact]
        public void BAVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.BAVisible = true;

            // Assert
            Assert.True(showingOption.BAVisible);
        }

        [Fact]
        public void NGVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.NGVisible = true;

            // Assert
            Assert.True(showingOption.NGVisible);
        }

        [Fact]
        public void SBVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.SBVisible = true;

            // Assert
            Assert.True(showingOption.SBVisible);
        }

        [Fact]
        public void VSVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.VSVisible = true;

            // Assert
            Assert.True(showingOption.VSVisible);
        }

        [Fact]
        public void KNVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.KNVisible = true;

            // Assert
            Assert.True(showingOption.KNVisible);
        }

        [Fact]
        public void GrandTotalVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.GrandTotalVisible = false;

            // Assert
            Assert.False(showingOption.GrandTotalVisible);
        }

        [Fact]
        public void CustomerCommVisible_GetSet_WorksCorrectly()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.CustomerCommVisible = true;

            // Assert
            Assert.True(showingOption.CustomerCommVisible);
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var showingOption = new ShowingOption();

            // Act
            showingOption.BAVisible = true;
            showingOption.NGVisible = true;
            showingOption.SBVisible = true;
            showingOption.VSVisible = true;
            showingOption.KNVisible = true;
            showingOption.GrandTotalVisible = false;
            showingOption.CustomerCommVisible = true;

            // Assert
            Assert.True(showingOption.BAVisible);
            Assert.True(showingOption.NGVisible);
            Assert.True(showingOption.SBVisible);
            Assert.True(showingOption.VSVisible);
            Assert.True(showingOption.KNVisible);
            Assert.False(showingOption.GrandTotalVisible);
            Assert.True(showingOption.CustomerCommVisible);
        }

        [Fact]
        public void DefaultValues_MatchExpectedPattern()
        {
            // Arrange & Act
            var showingOption = new ShowingOption();

            // Assert - Testing the business logic of default visibility
            Assert.True(showingOption.GrandTotalVisible); // Grand total should be visible by default
            Assert.False(showingOption.NGVisible); // Number game hidden by default
            Assert.False(showingOption.BAVisible); // Bet advisor hidden by default
            Assert.False(showingOption.KNVisible); // Keno hidden by default
            Assert.False(showingOption.SBVisible); // Sportsbook visibility not set by default
            Assert.False(showingOption.VSVisible); // Virtual sports visibility not set by default
            Assert.False(showingOption.CustomerCommVisible); // Customer commission hidden by default
        }
    }
}