namespace Fanex.Athena.Report.BySite.UnitTests.Models
{
    using System.Collections.Generic;
    using AutoFixture;
    using BySite.DTOs;
    using BySite.Models;
    using Xunit;

    public class OddsBoostBonusDetailResultModelTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void OddsBoostBonusDetailResultModel_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedData = _fixture.Create<List<OddsBoostBonusDetail>>();
            var expectedTotalSummary = _fixture.Create<OddsBoostBonusDetail>();

            // Act
            var resultModel = new OddsBoostBonusDetailResultModel
            {
                Data = expectedData,
                TotalSummary = expectedTotalSummary
            };

            // Assert
            Assert.Equal(expectedData, resultModel.Data);
            Assert.Equal(expectedTotalSummary, resultModel.TotalSummary);
        }

        [Fact]
        public void OddsBoostBonusDetailResultModel_DefaultConstructor_InitializesWithNullValues()
        {
            // Act
            var resultModel = new OddsBoostBonusDetailResultModel();

            // Assert
            Assert.Null(resultModel.Data);
            Assert.Null(resultModel.TotalSummary);
        }

        [Fact]
        public void OddsBoostBonusDetailResultModel_Data_CanBeEmptyList()
        {
            // Arrange
            var emptyData = new List<OddsBoostBonusDetail>();

            // Act
            var resultModel = new OddsBoostBonusDetailResultModel { Data = emptyData };

            // Assert
            Assert.NotNull(resultModel.Data);
            Assert.Empty(resultModel.Data);
        }

        [Fact]
        public void OddsBoostBonusDetailResultModel_Data_CanContainMultipleItems()
        {
            // Arrange
            var data = new List<OddsBoostBonusDetail>
            {
                _fixture.Create<OddsBoostBonusDetail>(),
                _fixture.Create<OddsBoostBonusDetail>(),
                _fixture.Create<OddsBoostBonusDetail>()
            };

            // Act
            var resultModel = new OddsBoostBonusDetailResultModel { Data = data };

            // Assert
            Assert.Equal(3, resultModel.Data.Count);
            Assert.Equal(data[0], resultModel.Data[0]);
            Assert.Equal(data[1], resultModel.Data[1]);
            Assert.Equal(data[2], resultModel.Data[2]);
        }

        [Fact]
        public void OddsBoostBonusDetailResultModel_TotalSummary_CanBeSetToSpecificValues()
        {
            // Arrange
            var totalSummary = new OddsBoostBonusDetail
            {
                CustomerAmount = 100.50m,
                ExternalId = "TotalSummary",
                SystemId = "Summary"
            };

            // Act
            var resultModel = new OddsBoostBonusDetailResultModel { TotalSummary = totalSummary };

            // Assert
            Assert.Equal(100.50m, resultModel.TotalSummary.CustomerAmount);
            Assert.Equal("TotalSummary", resultModel.TotalSummary.ExternalId);
            Assert.Equal("Summary", resultModel.TotalSummary.SystemId);
        }

        [Fact]
        public void OddsBoostBonusDetailResultModel_CanBeInitializedWithBothProperties()
        {
            // Arrange
            var data = new List<OddsBoostBonusDetail>
            {
                new() { CustomerAmount = 50.25m, ExternalId = "ext1" },
                new() { CustomerAmount = 75.75m, ExternalId = "ext2" }
            };
            var totalSummary = new OddsBoostBonusDetail { CustomerAmount = 126.00m };

            // Act
            var resultModel = new OddsBoostBonusDetailResultModel
            {
                Data = data,
                TotalSummary = totalSummary
            };

            // Assert
            Assert.Equal(2, resultModel.Data.Count);
            Assert.Equal(50.25m, resultModel.Data[0].CustomerAmount);
            Assert.Equal(75.75m, resultModel.Data[1].CustomerAmount);
            Assert.Equal(126.00m, resultModel.TotalSummary.CustomerAmount);
        }
    }
}
