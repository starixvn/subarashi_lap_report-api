namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using BySite.DTOs;
    using BySite.MetaModels;
    using BySite.Repositories.Interfaces;
    using BySite.Services;
    using Common.Services.Interfaces;
    using Moq;
    using Resources.Constants;
    using Xunit;

    public class MatchWinLossServiceTest
    {
        private readonly Mock<IMatchWinLossRepository> _dataAccess;

        private MatchWinLossService _service => new(_dataAccess.Object, new Mock<ICacheRefAdapter>().Object);

        public MatchWinLossServiceTest()
        {
            _dataAccess = new Mock<IMatchWinLossRepository>();
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnDataFromDataAccess()
        {
            // Arrange
            var metaModel = new MatchWinLossMetaModel();
            var dataList = new List<MatchWinLoss>
            {
                new() { ProductName = Product.SB, IsNeutral = true, },
                new() { ProductName = Product.VS, SportType = SportType.VirtualCycling, },
                new() { ProductName = Product.NG, }
            };

            _dataAccess.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            // Action
            var result = await _service.GetDataList(metaModel);

            // Assert
            _dataAccess.Verify(x => x.GetDataList(metaModel));
            Assert.Equal(result.Count, dataList.Count);
        }

        [Fact]
        public async Task GetDetailDataSet_WhenCall_ReturnDataFromDataAccess()
        {
            // Arrange
            var metaModel = new MatchWinLossMetaModel();
            var dataSet = new DataSet();

            _dataAccess.Setup(X => X.GetDetailDataSet(metaModel)).ReturnsAsync(dataSet);

            // Action
            var result = await _service.GetDetailDataSet(metaModel);

            // Assert
            _dataAccess.Verify(x => x.GetDetailDataSet(metaModel));
            Assert.Equal(result, dataSet);
        }

        [Fact]
        public async Task GetDetailDataList_WhenCall_ReturnDataFromDataAccess()
        {
            // Arrange
            var metaModel = new MatchWinLossMetaModel();
            var dataList = new List<MatchWinLoss>();
            dataList.Add(new MatchWinLoss());

            _dataAccess.Setup(x => x.GetDetailDataList(metaModel)).ReturnsAsync(dataList);

            // Action
            var result = await _service.GetDetailDataList(metaModel);

            // Assert
            _dataAccess.Verify(x => x.GetDetailDataList(metaModel));
            Assert.Equal(dataList, result);
        }

        [Fact]
        public async Task GetOutrightParlayList_WhenCall_ReturnDataFromDataAccess()
        {
            // Arrange
            var metaModel = new MatchWinLossMetaModel();
            var dataList = new List<MatchWinLoss>();
            dataList.Add(new MatchWinLoss());

            _dataAccess.Setup(x => x.GetOutrightParlayList(metaModel)).ReturnsAsync(dataList);

            // Action
            var result = await _service.GetOutrightParlayList(metaModel);

            // Assert
            _dataAccess.Verify(x => x.GetOutrightParlayList(metaModel));
            Assert.Equal(result, dataList);
        }
    }
}
