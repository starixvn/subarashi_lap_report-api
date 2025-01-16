using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Core.Utils.Repository;
using Moq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
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
                    OpeningBalance = 1500,
                    Turnover = 500,
                    Outstanding = 900,
                    WinLoss = 400,
                    ClosingBalance = 1400,
                    Accounts = accounts
                }
            };

            var totalData = new PlayerSabaCoins
            {
                OpeningBalance = 1500,
                Turnover = 500,
                Outstanding = 900,
                WinLoss = 400,
                ClosingBalance = 1400
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