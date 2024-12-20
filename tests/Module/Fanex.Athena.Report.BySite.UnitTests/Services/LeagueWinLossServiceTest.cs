using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class LeagueWinLossServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        public LeagueWinLossServiceTest()
        {
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)LeagueWinLossReportLevel.League).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<LeagueWinLoss>>>();

            var repository = new Mock<ILeagueWinLossRepository>();
            repository.Setup(_ => _.GetLeagueWinlossDetail(metaModel)).Returns(expectedData);

            var service = new LeagueWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMatchData()
        {
            // Arrange
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)LeagueWinLossReportLevel.Match).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<LeagueWinLoss>>>();

            var repository = new Mock<ILeagueWinLossRepository>();
            repository.Setup(_ => _.GetMatchWinlossDetail(metaModel)).Returns(expectedData);

            var service = new LeagueWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnBettypeData()
        {
            // Arrange
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)LeagueWinLossReportLevel.Bettype).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<LeagueWinLoss>>>();

            var repository = new Mock<ILeagueWinLossRepository>();
            repository.Setup(_ => _.GetBettypeWinlossDetail(metaModel)).Returns(expectedData);

            var service = new LeagueWinLossService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetCurrencyDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)LeagueWinLossReportLevel.League).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<LeagueWinLoss>>>();

            var repository = new Mock<ILeagueWinLossRepository>();
            repository.Setup(_ => _.GetLeagueWinlossCurrencyDetail(metaModel)).Returns(expectedData);

            var service = new LeagueWinLossService(repository.Object);

            // Action
            var result = service.GetCurrencyDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetCurrencyDataAsync_WhenCall_ReturnMatchData()
        {
            // Arrange
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)LeagueWinLossReportLevel.Match).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<LeagueWinLoss>>>();

            var repository = new Mock<ILeagueWinLossRepository>();
            repository.Setup(_ => _.GetMatchWinlossCurrencyDetail(metaModel)).Returns(expectedData);

            var service = new LeagueWinLossService(repository.Object);

            // Action
            var result = service.GetCurrencyDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetCurrencyDataAsync_WhenCall_ReturnBettypeData()
        {
            // Arrange
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().With(_ => _.DrillDownLevel, (int)LeagueWinLossReportLevel.Bettype).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<LeagueWinLoss>>>();

            var repository = new Mock<ILeagueWinLossRepository>();
            repository.Setup(_ => _.GetBettypeWinlossCurrencyDetail(metaModel)).Returns(expectedData);

            var service = new LeagueWinLossService(repository.Object);

            // Action
            var result = service.GetCurrencyDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }
    }
}