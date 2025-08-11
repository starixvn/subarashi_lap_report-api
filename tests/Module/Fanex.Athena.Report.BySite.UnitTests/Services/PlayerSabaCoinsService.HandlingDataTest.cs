namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.DTOs;
    using BySite.MetaModels;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using Moq;
    using Xunit;

    public partial class PlayerSabaCoinsServiceTest
    {
        [Fact]
        public async Task GetDataAsync_NoExtraInfo_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>()
                .With(_ => _.SortBy, "").With(_ => _.SortBy, "asc")
                .With(_ => _.PageSize, 20).With(_ => _.PageIndex, 1).Create();

            _repository.Setup(_ => _.GetMainData(metaModel)).ReturnsAsync(SetupMainData());
            _repository.Setup(_ => _.GetExtraInfoData(metaModel)).ReturnsAsync(new List<PlayerSabaCoinsExtraInfo>());
            var data = SetupResultNoExtraInfoData();

            // Action
            var result = await _service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(JsonSerializer.Serialize(data), JsonSerializer.Serialize(result));
        }

        [Fact]
        public async Task GetDataAsync_NoMainData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>()
                .With(_ => _.SortBy, "").With(_ => _.SortBy, "asc")
                .With(_ => _.PageSize, 20).With(_ => _.PageIndex, 1).Create();

            _repository.Setup(_ => _.GetMainData(metaModel)).ReturnsAsync(new List<PlayerSabaCoinsMain>());
            _repository.Setup(_ => _.GetExtraInfoData(metaModel)).ReturnsAsync(SetupExtraInfoData());
            var emptyData = new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = new SortableList<PlayerSabaCoins>(),
                TotalData = new PlayerSabaCoins()
            };

            // Action
            var result = await _service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(JsonSerializer.Serialize(emptyData), JsonSerializer.Serialize(result));
        }

        private SortableReportResultModel<PlayerSabaCoins> SetupResultNoExtraInfoData()
        {
            var accounts = new List<PlayerSabaCoinsAccount>
            {
                _account1, _account2
            };

            var dataList = new List<PlayerSabaCoins>
            {
                new PlayerSabaCoins
                {
                    SabaId = 22,
                    OpeningBalance = 900,
                    Turnover = 300,
                    Outstanding = 600,
                    WinLoss = 300,
                    ClosingBalance = 900,
                    Accounts = accounts
                }
            };

            var totalData = new PlayerSabaCoins
            {
                OpeningBalance = 900,
                Turnover = 300,
                Outstanding = 600,
                WinLoss = 300,
                ClosingBalance = 900
            };

            return new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = new SortableList<PlayerSabaCoins>(dataList),
                TotalData = totalData,
                TotalRows = 1,
            };
        }
    }
}