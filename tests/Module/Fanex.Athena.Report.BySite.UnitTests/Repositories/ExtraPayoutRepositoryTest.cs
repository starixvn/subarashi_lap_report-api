namespace Fanex.Athena.Report.BySite.UnitTests.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using BySite.Repositories;
using Common.Model.DTO;
using Common.Services.Interfaces;
using Core.Services.Interfaces;
using Data;
using MetaModels;
using Moq;
using Xunit;

public class ExtraPayoutRepositoryTest
{
    private readonly BalanceUp _balance = new()
    {
        MoveBetsDay = new DateTime(2016, 8, 10),
        BalanceUpDay = new DateTime(2016, 8, 31)
    };

    private readonly Mock<IBalanceUpService> _balanceUpService;
    private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;
    private readonly Mock<IContextService> _contextService;

    private readonly Dictionary<string, object> _expectedDetailData = new()
    {
        { "ProductCode", "SB" },
        { "PromotionType", 199 },
        { "WinLostDate", "01/01/2022" },
        { "RefNo", "1002" },
        { "BasedTurnover", 100 },
        { "BasedAmount", 200 },
        { "BasedSharingAmount", 300 },
        { "Turnover", 200 },
        { "Amount", 100 },
        { "SharingAmount", 200 }
    };

    private readonly Dictionary<string, object> _expectedMainData = new()
    {
        { "ProductCode", "SB" },
        { "reporttype", "20220101" },
        { "Currency", 7 },
        { "betcount", 10 },
        { "BasedTurnover", 100 },
        { "BasedAmount", 200 },
        { "BasedSharingAmount", 300 },
        { "Turnover", 200 },
        { "Amount", 100 },
        { "SharingAmount", 200 }
    };

    private readonly Dictionary<string, object> _expectedMemberData = new()
    {
        { "PromotionType", 199 },
        { "CustId", 99 },
        { "username", "Kelly" },
        { "Nickname", "Kelly12Bet" }
    };

    private readonly IFixture _fixture = new Fixture();

    public ExtraPayoutRepositoryTest()
    {
        DbSettingProviderManager
            .StartTestingSession()
            .Run();

        _balanceUpService = new Mock<IBalanceUpService>();
        _balanceUpService.Setup(b => b.GetBalanceUpAsync()).Returns(Task.FromResult(_balance));

        _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<string>(), It.IsAny<string>())).Returns(_fixture.Create<string>());
        _cacheRefAdapter.Setup(c => c.GetBettype(It.IsAny<int>(), false)).Returns(_fixture.Create<string>());

        _contextService = new Mock<IContextService>();
        _contextService.Setup(c => c.CurrencyList).Returns(_fixture.CreateMany<KeyValuePair<int, string>>(10).ToDictionary(x => x.Key, x => x.Value));
    }

    public ExtraPayoutRepository CreateRepository()
    {
        return new ExtraPayoutRepository(_balanceUpService.Object, _cacheRefAdapter.Object, _contextService.Object);
    }

    [Fact]
    public async Task GetExtraPayoutMainData_WhenCall_ReturnCorrectData()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);
        SetUpData(reader, _expectedMainData);

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutMainData(metaModel);
        var data = result.DataList.FirstOrDefault();

        // Assert
        Assert.Equal("SB", data.ProductCode);
        Assert.Equal(10, data.BetCount);
        Assert.Equal(7, data.Currency);
        Assert.Equal(100, data.BasedTurnover);
        Assert.Equal(200, data.BasedAmount);
        Assert.Equal(300, data.BasedSharingAmount);
    }

    [Fact]
    public async Task GetExtraPayoutMainData_NoData_ReturnEmptyResult()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutMainData(metaModel);

        // Assert
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public async Task GetExtraPayoutMainData_ErrCode_ReturnEmptyResult()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);
        reader.Setup(r => r.FieldCount).Returns(1);
        reader.Setup(r => r.GetName(0)).Returns("ErrCode");

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutMainData(metaModel);

        // Assert
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public async Task GetExtraPayoutMemberData_WhenCall_ReturnCorrectData()
    {
        // Arrange
        var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
            .With(x => x.PageIndex, 1)
            .With(x => x.PageSize, 20)
            .Create();
        var reader = new Mock<IDataReader>();
        reader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);
        SetUpData(reader, _expectedMainData);
        SetUpData(reader, _expectedMemberData);

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutMemberData(metaModel);
        var data = result.DataList.FirstOrDefault();

        // Assert
        Assert.Equal("SB", data.ProductCode);
        Assert.Equal(10, data.BetCount);
        Assert.Equal(7, data.Currency);
        Assert.Equal(100, data.BasedTurnover);
        Assert.Equal(200, data.BasedAmount);
        Assert.Equal(300, data.BasedSharingAmount);
        Assert.Equal(199, data.PromotionType);
        Assert.Equal(99, data.CustId);
        Assert.Equal("Kelly", data.Username);
    }

    [Fact]
    public async Task GetExtraPayoutMemberData_NoData_ReturnEmptyResult()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutMemberData(metaModel);

        // Assert
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public async Task GetExtraPayoutMemberData_ErrCode_ReturnEmptyResult()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);
        reader.Setup(r => r.FieldCount).Returns(1);
        reader.Setup(r => r.GetName(0)).Returns("ErrCode");

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutMemberData(metaModel);

        // Assert
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public async Task GetExtraPayoutDetailData_WhenCall_ReturnCorrectData()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false);
        SetUpData(reader, _expectedDetailData);

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutDetailData(metaModel);
        var data = result.DataList.FirstOrDefault();

        // Assert
        Assert.Equal("SB", data.ProductCode);
        Assert.Equal(199, data.PromotionType);
        Assert.Equal(100, data.BasedTurnover);
        Assert.Equal(new DateTime(2022, 01, 01), data.Date);
        Assert.Equal("1002", data.Refno);
        Assert.Equal(200, data.BasedAmount);
        Assert.Equal(300, data.BasedSharingAmount);
    }

    [Fact]
    public async Task GetExtraPayoutDetailData_NoData_ReturnEmptyResult()
    {
        // Arrange
        var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
        var reader = new Mock<IDataReader>();
        reader.Setup(r => r.Read()).Returns(false);

        CreateObjectDb(reader);

        var repository = CreateRepository();

        // Action
        var result = await repository.GetExtraPayoutDetailData(metaModel);

        // Assert
        Assert.Equal(0, result.TotalRows);
    }

    private static void SetUpData(Mock<IDataReader> reader, Dictionary<string, object> data)
    {
        foreach (var item in data)
        {
            reader.Setup(r => r[item.Key]).Returns(item.Value);
        }
    }

    private static void CreateObjectDb(Mock<IDataReader> mockDataReader)
    {
        var mockObjectDB = new Mock<IObjectDb>();
        mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockDataReader.Object));

        ObjectDbFactory.SwitchToTestingMode();
        ObjectDbFactory.Mock((x, y, z) => mockObjectDB.Object);
    }
}