using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class NewCashOutWinLossServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>().With(_ => _.DrillDownLevel, 0).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<CashOutWinLoss>>>();

            var repository = new Mock<INewCashOutWinLossRepository>();
            repository.Setup(_ => _.GetNewCashOutWinLossMainData(metaModel)).Returns(expectedData);

            var service = new NewCashOutWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMemberData()
        {
            // Arrange
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>().With(_ => _.DrillDownLevel, 1).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<CashOutWinLoss>>>();

            var repository = new Mock<INewCashOutWinLossRepository>();
            repository.Setup(_ => _.GetNewCashOutWinLossMemberData(metaModel)).Returns(expectedData);

            var service = new NewCashOutWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }
    }
}