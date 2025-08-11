namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    using Core.Models.DTOs;
    using Xunit;

    public class GroupSubsidiaryTest
    {
        [Fact]
        public void WhiteLabelID_GetSet_WorksCorrectly()
        {
            // Arrange
            var groupSubsidiary = new GroupSubsidiary();
            const int expectedId = 123;

            // Act
            groupSubsidiary.WhiteLabelID = expectedId;

            // Assert
            Assert.Equal(expectedId, groupSubsidiary.WhiteLabelID);
        }

        [Fact]
        public void WhiteLabelID_CanBeNull()
        {
            // Arrange
            var groupSubsidiary = new GroupSubsidiary();

            // Act
            groupSubsidiary.WhiteLabelID = null;

            // Assert
            Assert.Null(groupSubsidiary.WhiteLabelID);
        }

        [Fact]
        public void WhiteLabelName_GetSet_WorksCorrectly()
        {
            // Arrange
            var groupSubsidiary = new GroupSubsidiary();
            const string expectedName = "Test White Label";

            // Act
            groupSubsidiary.WhiteLabelName = expectedName;

            // Assert
            Assert.Equal(expectedName, groupSubsidiary.WhiteLabelName);
        }

        [Fact]
        public void OperatorId_GetSet_WorksCorrectly()
        {
            // Arrange
            var groupSubsidiary = new GroupSubsidiary();
            const string expectedOperatorId = "OP001";

            // Act
            groupSubsidiary.OperatorId = expectedOperatorId;

            // Assert
            Assert.Equal(expectedOperatorId, groupSubsidiary.OperatorId);
        }

        [Fact]
        public void DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var groupSubsidiary = new GroupSubsidiary();

            // Assert
            Assert.Null(groupSubsidiary.WhiteLabelID);
            Assert.Null(groupSubsidiary.WhiteLabelName);
            Assert.Null(groupSubsidiary.OperatorId);
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var groupSubsidiary = new GroupSubsidiary();
            const int expectedId = 456;
            const string expectedName = "Test Name";
            const string expectedOperatorId = "OP002";

            // Act
            groupSubsidiary.WhiteLabelID = expectedId;
            groupSubsidiary.WhiteLabelName = expectedName;
            groupSubsidiary.OperatorId = expectedOperatorId;

            // Assert
            Assert.Equal(expectedId, groupSubsidiary.WhiteLabelID);
            Assert.Equal(expectedName, groupSubsidiary.WhiteLabelName);
            Assert.Equal(expectedOperatorId, groupSubsidiary.OperatorId);
        }

        [Fact]
        public void Properties_CanHandleEmptyStrings()
        {
            // Arrange
            var groupSubsidiary = new GroupSubsidiary();

            // Act
            groupSubsidiary.WhiteLabelName = "";
            groupSubsidiary.OperatorId = "";

            // Assert
            Assert.Equal("", groupSubsidiary.WhiteLabelName);
            Assert.Equal("", groupSubsidiary.OperatorId);
        }
    }
}