using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class GodOfFortuneWinLossServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)ReportLevel.Main).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<GodOfFortuneWinLoss>>>();

            var repository = new Mock<IGodOfFortuneWinLossRepository>();
            repository.Setup(_ => _.GetSummaryWinloss(metaModel)).Returns(expectedData);

            var service = new GodOfFortuneWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnLicenseeData()
        {
            // Arrange
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)ReportLevel.Licensee).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<GodOfFortuneWinLoss>>>();

            var repository = new Mock<IGodOfFortuneWinLossRepository>();
            repository.Setup(_ => _.GetAgentWinlossDetail(metaModel)).Returns(expectedData);

            var service = new GodOfFortuneWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnCustomerData()
        {
            // Arrange
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)ReportLevel.Customer).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<GodOfFortuneWinLoss>>>();

            var repository = new Mock<IGodOfFortuneWinLossRepository>();
            repository.Setup(_ => _.GetCustomerWinlossDetail(metaModel)).Returns(expectedData);

            var service = new GodOfFortuneWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }
    }
}