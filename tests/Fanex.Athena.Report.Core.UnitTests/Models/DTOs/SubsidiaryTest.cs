using Fanex.Athena.Report.Core.Models.DTOs;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    public class SubsidiaryTest
    {
        [Fact]
        public void AgentId_GetSet_WorksCorrectly()
        {
            // Arrange
            var subsidiary = new Subsidiary();
            const int expectedAgentId = 12345;

            // Act
            subsidiary.AgentId = expectedAgentId;

            // Assert
            Assert.Equal(expectedAgentId, subsidiary.AgentId);
        }

        [Fact]
        public void OperatorId_GetSet_WorksCorrectly()
        {
            // Arrange
            var subsidiary = new Subsidiary();
            const string expectedOperatorId = "OP123";

            // Act
            subsidiary.OperatorId = expectedOperatorId;

            // Assert
            Assert.Equal(expectedOperatorId, subsidiary.OperatorId);
        }

        [Fact]
        public void CurrencyId_GetSet_WorksCorrectly()
        {
            // Arrange
            var subsidiary = new Subsidiary();
            const int expectedCurrencyId = 1;

            // Act
            subsidiary.CurrencyId = expectedCurrencyId;

            // Assert
            Assert.Equal(expectedCurrencyId, subsidiary.CurrencyId);
        }

        [Fact]
        public void DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var subsidiary = new Subsidiary();

            // Assert
            Assert.Equal(0, subsidiary.AgentId);
            Assert.Null(subsidiary.OperatorId);
            Assert.Equal(0, subsidiary.CurrencyId);
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var subsidiary = new Subsidiary();
            const int expectedAgentId = 98765;
            const string expectedOperatorId = "OP456";
            const int expectedCurrencyId = 2;

            // Act
            subsidiary.AgentId = expectedAgentId;
            subsidiary.OperatorId = expectedOperatorId;
            subsidiary.CurrencyId = expectedCurrencyId;

            // Assert
            Assert.Equal(expectedAgentId, subsidiary.AgentId);
            Assert.Equal(expectedOperatorId, subsidiary.OperatorId);
            Assert.Equal(expectedCurrencyId, subsidiary.CurrencyId);
        }

        [Fact]
        public void Properties_HandleNegativeValues()
        {
            // Arrange
            var subsidiary = new Subsidiary();
            const int negativeAgentId = -1;
            const int negativeCurrencyId = -5;

            // Act
            subsidiary.AgentId = negativeAgentId;
            subsidiary.CurrencyId = negativeCurrencyId;

            // Assert
            Assert.Equal(negativeAgentId, subsidiary.AgentId);
            Assert.Equal(negativeCurrencyId, subsidiary.CurrencyId);
        }

        [Fact]
        public void OperatorId_CanHandleEmptyString()
        {
            // Arrange
            var subsidiary = new Subsidiary();

            // Act
            subsidiary.OperatorId = "";

            // Assert
            Assert.Equal("", subsidiary.OperatorId);
        }
    }
}