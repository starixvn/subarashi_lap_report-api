namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.DTOs;
    using BySite.MetaModels;
    using BySite.Repositories.Interfaces;
    using BySite.Services;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using Moq;
    using Xunit;

    public partial class PlayerSabaCoinsServiceTest
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IPlayerSabaCoinsRepository> _repository;
        private readonly PlayerSabaCoinsService _service;

        private PlayerSabaCoinsAccount _account1;
        private PlayerSabaCoinsAccount _account2;

        private PlayerSabaCoinsMain _main1;
        private PlayerSabaCoinsMain _main2;

        private PlayerSabaCoinsExtraInfo _extraInfo1;
        private PlayerSabaCoinsExtraInfo _extraInfo2;

        public PlayerSabaCoinsServiceTest()
        {
            _repository = new Mock<IPlayerSabaCoinsRepository>();
            _service = new(_repository.Object);
            SetUpData();
        }

        #region SetUpData

        private void SetUpData()
        {
            _main1 = new PlayerSabaCoinsMain
            {
                CustId = 1,
                SabaId = 22,
                Username = "test 1",
                ExternalId = "12BTEST",
                OpeningBalance = 900,
                Turnover = 300,
                Outstanding = 600,
                WinLoss = 300,
                ClosingBalance = 900
            };
            _main2 = new PlayerSabaCoinsMain
            {
                CustId = 2,
                SabaId = 22,
                Username = "test 2",
                ExternalId = "12BTEST2",
                OpeningBalance = 900,
                Turnover = 300,
                Outstanding = 600,
                WinLoss = 300,
                ClosingBalance = 900
            };

            _extraInfo1 = new PlayerSabaCoinsExtraInfo
            {
                CustId = 1,
                SabaId = 22,
                ExchangeAmt = 300,
                RewardPromotionAmt = 900
            };
            _extraInfo2 = new PlayerSabaCoinsExtraInfo
            {
                CustId = 2,
                SabaId = 22,
                ExchangeAmt = 300,
                RewardPromotionAmt = 900
            };

            _account1 = new PlayerSabaCoinsAccount
            {
                CustId = 1,
                Username = "test 1",
                ExternalId = "12BTEST"
            };
            _account2 = new PlayerSabaCoinsAccount
            {
                CustId = 2,
                Username = "test 2",
                ExternalId = "12BTEST2"
            };
        }

        #endregion SetUpData

        [Fact]
        public async Task GetDataAsync_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>()
                .With(_ => _.SortBy, "").With(_ => _.SortBy, "asc")
                .With(_ => _.PageSize, 20).With(_ => _.PageIndex, 1).Create();

            _repository.Setup(_ => _.GetMainData(metaModel)).ReturnsAsync(SetupMainData());
            _repository.Setup(_ => _.GetExtraInfoData(metaModel)).ReturnsAsync(SetupExtraInfoData());
            var data = SetupResultData();

            // Action
            var result = await _service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(JsonSerializer.Serialize(data), JsonSerializer.Serialize(result));
        }

        [Fact]
        public async Task GetDataAsync_WhenCall_ReturnCorrectType()
        {
            // Arrange
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>()
                .With(_ => _.SortBy, "").With(_ => _.SortBy, "asc")
                .With(_ => _.PageSize, 20).With(_ => _.PageIndex, 1).Create();

            _repository.Setup(_ => _.GetMainData(metaModel)).ReturnsAsync(SetupMainData());
            _repository.Setup(_ => _.GetExtraInfoData(metaModel)).ReturnsAsync(SetupExtraInfoData());

            // Action
            var result = await _service.GetDataAsync(metaModel);

            // Assert
            Assert.IsType<SortableReportResultModel<PlayerSabaCoins>>(result);
        }

        [Fact]
        public async Task GetDataAsync_WhenCall_CallRepositoryToRetrieveData()
        {
            // Arrange
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>()
                .With(_ => _.SortBy, "").With(_ => _.SortBy, "asc")
                .With(_ => _.PageSize, 20).With(_ => _.PageIndex, 1).Create();

            _repository.Setup(_ => _.GetMainData(metaModel)).ReturnsAsync(SetupMainData());
            _repository.Setup(_ => _.GetExtraInfoData(metaModel)).ReturnsAsync(SetupExtraInfoData());

            // Action
            _ = await _service.GetDataAsync(metaModel);

            // Assert
            _repository.Verify(repo => repo.GetMainData(metaModel), Times.Once);
            _repository.Verify(repo => repo.GetExtraInfoData(metaModel), Times.Once);
        }

        private IEnumerable<PlayerSabaCoinsMain> SetupMainData()
        {
            return new List<PlayerSabaCoinsMain>
            {
                _main1, _main2
            };
        }

        private IEnumerable<PlayerSabaCoinsExtraInfo> SetupExtraInfoData()
        {
            return new List<PlayerSabaCoinsExtraInfo>
            {
                _extraInfo1, _extraInfo2
            };
        }

        private SortableReportResultModel<PlayerSabaCoins> SetupResultData()
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
                    ExchangeAmt = 300,
                    RewardPromotionAmt = 900,
                    Accounts = accounts
                }
            };

            var totalData = new PlayerSabaCoins
            {
                OpeningBalance = 900,
                Turnover = 300,
                Outstanding = 600,
                WinLoss = 300,
                ClosingBalance = 900,
                ExchangeAmt = 300,
                RewardPromotionAmt = 900
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