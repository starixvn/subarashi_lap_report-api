namespace Fanex.Athena.Report.BySite.UnitTests.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BySite.Repositories;
using Common.Helpers;
using Common.Model.DataDomain;
using Common.Model.DTO;
using Common.Services.Interfaces;
using Core.Models.DTOs;
using Data;
using DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using MetaModels;
using Moq;
using Report.UnitTests.Common;
using Resources;
using Xunit;

public class LiveReportRepositoryTest
{
    protected const int CurrencyId = 123;

    protected const string CurrencyName = "aaaaaaaaa";

    private readonly BalanceUp _balanceUp;

    private readonly Mock<IBalanceUpService> _balanceUpService;

    private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

    private readonly Mock<IProductService> _productService;

    private readonly LiveReportMetaModel _metaModel;

    private readonly Mock<IObjectDb> _objectDb;

    private readonly Mock<IDataReader> _reader;

    private readonly IDictionary<int, string> _userCurrencyList = new Dictionary<int, string>
    {
        { CurrencyId, CurrencyName }, { 1, "USD" }
    };

    private readonly int[] _liveReportNoDetailLinkSports = { 1, 2, 3 };

    public LiveReportRepositoryTest()
    {
        DbSettingProviderManager.StartTestingSession().Run();
        _metaModel = FakeHelper.Create<LiveReportMetaModel>();
        _metaModel.BaseCurrency = "UUS";
        _metaModel.AgentListXml = "3";
        _metaModel.FromDate = DateTimeHelper.CreateDateTime(2017, 1, 1);
        _metaModel.ToDate = DateTimeHelper.CreateDateTime(2017, 2, 2);
        _metaModel.LiveIndicator = 1;

        _balanceUp = new BalanceUp
        {
            MoveBetsDay = DateTimeHelper.CreateDateTime(2017, 2, 2)
        };

        _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        _balanceUpService = new Mock<IBalanceUpService>();
        _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balanceUp);

        _reader = new Mock<IDataReader>();
        _objectDb = new Mock<IObjectDb>();
        _productService = new Mock<IProductService>();
    }

    [Fact]
    public async Task GetLiveReportSummary_WhenCall_ParametersPassedCorrectly()
    {
        // Arrange
        _metaModel.ProductCodeList = "SB,NG";

        var repository = CreateRepository();

        // Action
        await repository.GetLiveReportSummary(_metaModel);

        // Assert
        AssertCommonSetParameterValues();
    }

    [Fact]
    public async Task GetLiveReportSummary_WhenCall_ReturnCorrectResult()
    {
        // Arrange
        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);
        _reader.Setup(x => x["ProductCode"]).Returns("SB");
        _reader.Setup(x => x["Turnover"]).Returns(100);
        _reader.Setup(x => x["Winlost"]).Returns(200);
        _reader.Setup(x => x["PlayerComm"]).Returns(300);
        _reader.Setup(x => x["Comm"]).Returns(400);
        _reader.Setup(x => x["LicenseeWL"]).Returns(500);
        _reader.Setup(x => x["AmtDueto3rd"]).Returns(600);
        _reader.Setup(x => x["Betcount"]).Returns(700);
        _productService.Setup(_productService => _productService.GetProductName("SB")).Returns(ViewResources.Sportsbook);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetLiveReportSummary(_metaModel);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(ViewResources.Sportsbook, result[0].ProductName);
        Assert.Equal("SB", result[0].ProductCode);
        Assert.Equal(100, result[0].TurnOver);
        Assert.Equal(200, result[0].CustomerWinLoss);
        Assert.Equal(300, result[0].CustomerComm);
        Assert.Equal(400, result[0].LicenseeComm);
        Assert.Equal(500, result[0].LicenseeWinLoss);
        Assert.Equal(600, result[0].AmountDue);
        Assert.Equal(700, result[0].BetCount);
    }

    [Fact]
    public async Task GetDetailBySport_WhenCall_ParametersPassedCorrectly()
    {
        // Arrange
        _metaModel.ProductCodeList = "SB,NG";

        var repository = CreateRepository();

        // Action
        await repository.GetDetailBySport(_metaModel, _liveReportNoDetailLinkSports);

        // Assert
        AssertCommonSetParameterValues();
    }

    [Fact]
    public async Task GetDetailBySport_WhenCall_ReturnCorrectResult()
    {
        // Arrange
        AssignCommonReaderValues(FakeHelper.Create<LiveReport>());

        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);
        _reader.Setup(x => x["SportType"]).Returns("1");
        _reader.Setup(x => x["Turnover"]).Returns(100);
        _reader.Setup(x => x["Winlost"]).Returns(200);
        _reader.Setup(x => x["ProductCode"]).Returns("SB");

        var repository = CreateRepository();

        // Action
        var result = await repository.GetDetailBySport(_metaModel, _liveReportNoDetailLinkSports);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal("1", result[0].ProductCode);
        Assert.Equal(100, result[0].TurnOver);
        Assert.Equal(200, result[0].CustomerWinLoss);
    }

    [Theory]
    [InlineData("102", BetType.CombinationMixParlay)]
    [InlineData("103", BetType.Outright)]
    public async Task GetDetailBySport_WhenCall_CallCorrectCacheRefMethod(string sportType, int bettype)
    {
        // Arrange
        AssignCommonReaderValues(FakeHelper.Create<LiveReport>());

        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);

        _reader.Setup(x => x["SportType"]).Returns(sportType);
        _reader.Setup(x => x["Turnover"]).Returns(100);
        _reader.Setup(x => x["Winlost"]).Returns(200);
        _reader.Setup(x => x["ProductCode"]).Returns("SB");

        var repository = CreateRepository();

        // Action
        await repository.GetDetailBySport(_metaModel, _liveReportNoDetailLinkSports);

        // Assert
        _cacheRefAdapter.Verify(x => x.GetBettype(bettype), Times.Once);
    }

    [Fact]
    public async Task GetDetailBySport_VirtualSports2MixParlay_CallCorrectCacheRefMethod()
    {
        // Arrange
        AssignCommonReaderValues(FakeHelper.Create<LiveReport>());

        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);
        _reader.Setup(x => x["SportType"]).Returns("104");
        _reader.Setup(x => x["Turnover"]).Returns(100);
        _reader.Setup(x => x["Winlost"]).Returns(200);
        _reader.Setup(x => x["ProductCode"]).Returns("SB");

        var repository = CreateRepository();

        // Action
        await repository.GetDetailBySport(_metaModel, _liveReportNoDetailLinkSports);

        // Assert
        _cacheRefAdapter.Verify(x => x.GetBettype(BetType.MixParlay), Times.Once);
    }

    [Fact]
    public async Task GetDetailByCurrencyAndBettype_WhenCall_ParametersPassedCorrectly()
    {
        // Arrange
        _metaModel.ProductCodeList = "SB,NG";

        var repository = CreateRepository();

        // Action
        await repository.GetDetailByCurrencyAndBettype(_metaModel, _userCurrencyList, new List<ProductInfo>());

        // Assert
        AssertCommonSetParameterValues();
    }

    [Fact]
    public async Task GetDetailByCurrencyAndBettype_WhenCall_ReturnCorrectResult()
    {
        // Arrange
        var productList = new List<ProductInfo>
        {
            new() { ProductCode = "SB", ProductName = "SuperBoy" }
        };

        AssignCommonReaderValues(FakeHelper.Create<LiveReport>());

        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false).Returns(true).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);
        _reader.Setup(x => x["Currency"]).Returns("1");
        _reader.Setup(x => x["Turnover"]).Returns(100);
        _reader.Setup(x => x["Winlost"]).Returns(200);
        _reader.Setup(x => x["bettype"]).Returns("1");
        _reader.Setup(x => x["ProductCode"]).Returns("SB");

        var repository = CreateRepository();

        // Action
        var result = await repository.GetDetailByCurrencyAndBettype(_metaModel, _userCurrencyList, productList);

        // Assert
        Assert.Equal(1, result.Item1.Count);
        Assert.Equal(100, result.Item1[0].TurnOver);
        Assert.Equal(200, result.Item1[0].CustomerWinLoss);

        Assert.Equal(1, result.Item2.Count);
        Assert.Equal(1, result.Item2[0].LiveReports.Count);
        Assert.Equal("SuperBoy", result.Item2[0].ProductName);
    }

    [Theory]
    [InlineData("2803")]
    [InlineData("2804")]
    [InlineData("2805")]
    [InlineData("2806")]
    public async Task GetDetailByCurrencyAndBettype_BettypeOfVirtualBasketball_ReturnCorrectBetTypeName(string betType)
    {
        // Arrange
        var productName = $"{betType} (Virtual Basketball)";
        var productList = new List<ProductInfo>
        {
            new() { ProductCode = "VS2", ProductName = "VS2" }
        };

        AssignCommonReaderValues(FakeHelper.Create<LiveReport>());

        _cacheRefAdapter.Setup(x => x.GetBettype(It.IsAny<int>())).Returns(betType);
        _cacheRefAdapter.Setup(x => x.GetSportName(It.IsAny<int>(), null)).Returns("Virtual Basketball");

        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false).Returns(true).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);
        _reader.Setup(x => x["Currency"]).Returns("20");
        _reader.Setup(x => x["Turnover"]).Returns(200);
        _reader.Setup(x => x["Winlost"]).Returns(400);
        _reader.Setup(x => x["bettype"]).Returns(betType);
        _reader.Setup(x => x["ProductCode"]).Returns("VS2");

        var repository = CreateRepository();

        // Action
        var result = await repository.GetDetailByCurrencyAndBettype(_metaModel, _userCurrencyList, productList);

        // Assert
        Assert.Equal(productName, result.Item2[0].LiveReports[0].ProductName);
    }

    [Fact]
    public async Task GetDetailByCurrencyAndBettype_NoData_ReturnEmptyResult()
    {
        // Arrange
        var productList = new List<ProductInfo>
        {
            new() { ProductCode = "SB", ProductName = "SuperBoy" }
        };

        _reader.SetupSequence(x => x.Read()).Returns(false);
        _reader.SetupSequence(x => x.NextResult()).Returns(true);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetDetailByCurrencyAndBettype(_metaModel, _userCurrencyList, productList);

        // Assert
        Assert.Empty(result.Item1);
        Assert.Empty(result.Item2);
    }

    [Fact]
    public async Task GetLiveReportWinlossByMatch_WhenCall_ParametersPassedCorrectly()
    {
        // Arrange
        var metaModel = new LiveReportMatchListMetalModel
        {
            ProductCodeList = "SB,NG",
            AgentListXml = "3",
            FromDate = DateTimeHelper.CreateDateTime(2017, 1, 1),
            ToDate = DateTimeHelper.CreateDateTime(2017, 2, 2),
            LiveIndicator = 1,
            SportId = 3
        };

        var repository = CreateRepository();

        // Action
        await repository.GetLiveReportWinlossByMatch(metaModel);

        // Assert
        _objectDb.Verify(x => x.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("fromdate", metaModel.FromDate.Date), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("todate", metaModel.ToDate.Date), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("liveindicator", metaModel.LiveIndicator), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("movebetsday", _balanceUp.MoveBetsDay), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("sportid", metaModel.SportId), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("BaseCurrency", metaModel.BaseCurrency), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("ProductList", "SB,NG"), Times.Once);
    }

    [Fact]
    public async Task GetLiveReportWinlossByMatch_WhenCall_ReturnCorrectResult()
    {
        // Arrange
        var expected = FakeHelper.Create<LiveReportWinLossByMatch>();

        _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
        _reader.Setup(x => x["matchid"]).Returns(expected.MatchId);
        _reader.Setup(x => x["homeid"]).Returns(expected.HomeId);
        _reader.Setup(x => x["awayid"]).Returns(expected.AwayId);
        _reader.Setup(x => x["Betcount"]).Returns(expected.BetCount);
        _reader.Setup(x => x["Turnover"]).Returns(expected.TurnOver);
        _reader.Setup(x => x["Winlost"]).Returns(expected.CustomerWinLoss);
        _reader.Setup(x => x["PlayerComm"]).Returns(expected.CustomerComm);
        _reader.Setup(x => x["Comm"]).Returns(expected.LicenseeComm);
        _reader.Setup(x => x["LicenseeWL"]).Returns(expected.LicenseeWinLoss);
        _reader.Setup(x => x["EventType"]).Returns(expected.EventType);
        _reader.Setup(x => x["VendorMatchId"]).Returns(expected.VendorMatchId);
        _reader.Setup(x => x["Distance"]).Returns(expected.Distance);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetLiveReportWinlossByMatch(new LiveReportMatchListMetalModel { ProductCodeList = string.Empty });

        // Assert
        Assert.Single(result);
        Assert.Equal(expected.MatchId, result[0].MatchId);
        Assert.Equal(expected.HomeId, result[0].HomeId);
        Assert.Equal(expected.AwayId, result[0].AwayId);
        Assert.Equal(expected.TurnOver, result[0].TurnOver);
        Assert.Equal(expected.CustomerWinLoss, result[0].CustomerWinLoss);
        Assert.Equal(expected.CustomerComm, result[0].CustomerComm);
        Assert.Equal(expected.LicenseeComm, result[0].LicenseeComm);
        Assert.Equal(expected.LicenseeWinLoss, result[0].LicenseeWinLoss);
        Assert.Equal(expected.EventType, result[0].EventType);
        Assert.Equal(expected.VendorMatchId, result[0].VendorMatchId);
        Assert.Equal(expected.Distance, result[0].Distance);
    }

    private void AssignCommonReaderValues(LiveReport item)
    {
        _reader.Setup(x => x["Turnover"]).Returns(item.TurnOver);
        _reader.Setup(x => x["Winlost"]).Returns(item.CustomerWinLoss);
        _reader.Setup(x => x["PlayerComm"]).Returns(item.CustomerComm);
        _reader.Setup(x => x["Comm"]).Returns(item.LicenseeComm);
        _reader.Setup(x => x["LicenseeWL"]).Returns(item.LicenseeWinLoss);
        _reader.Setup(x => x["AmtDueto3rd"]).Returns(item.AmountDue);
        _reader.Setup(x => x["Betcount"]).Returns(item.BetCount);
    }

    private void AssertCommonSetParameterValues()
    {
        _objectDb.Verify(x => x.SetParameterValue("AgentList", _metaModel.AgentListXml), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("fromdate", _metaModel.FromDate.Date), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("todate", _metaModel.ToDate.Date), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("liveindicator", _metaModel.LiveIndicator), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("movebetsday", _balanceUp.MoveBetsDay), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("ProductCodes", _metaModel.ProductCodeList), Times.Once);
        _objectDb.Verify(x => x.SetParameterValue("BaseCurrency", _metaModel.BaseCurrency), Times.Once);
    }

    private LiveReportRepository CreateRepository()
    {
        SetupObjectDb();
        return new LiveReportRepository(_cacheRefAdapter.Object, _balanceUpService.Object, _productService.Object);
    }

    private void SetupObjectDb()
    {
        _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(_reader.Object));

        ObjectDbFactory.SwitchToTestingMode();
        ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
    }
}