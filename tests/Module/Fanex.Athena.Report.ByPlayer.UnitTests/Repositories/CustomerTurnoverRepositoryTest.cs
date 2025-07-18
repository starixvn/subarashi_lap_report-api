namespace Fanex.Athena.Report.ByPlayer.UnitTests.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Common.Helpers;
using Common.Model.DTO;
using Common.Services.Interfaces;
using Data;
using Fanex.Athena.Report.Core.Services.Interfaces;
using MetaModels;
using Moq;
using Repository;
using Xunit;

public class CustomerTurnoverRepositoryTest
{
    private const string LanguageCode = "en";
    private readonly BalanceUp _balanceUp;

    private readonly Mock<IBalanceUpService> _balanceUpService;

    private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

    private readonly Mock<IProductService> _productService;

    private readonly Dictionary<int, string> _currencyList = new() { { 1, "UUS" } };

    private readonly CustomerTurnoverMetaModel _metaModel = new()
    {
        Username = "UsernameTest"
    };

    private readonly Mock<IObjectDb> _objectDb;

    private readonly Mock<IDataReader> _reader;

    public CustomerTurnoverRepositoryTest()
    {
        _balanceUpService = new Mock<IBalanceUpService>();
        _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        _productService = new Mock<IProductService>();
        _reader = new Mock<IDataReader>();
        _objectDb = new Mock<IObjectDb>();

        _balanceUp = new BalanceUp
        {
            MoveBetsDay = DateTimeHelper.CreateDateTime(2017, 2, 2)
        };
        _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balanceUp);
    }

    [Fact]
    public async Task GetDataList_NoData_ReturnEmptyResult()
    {
        // Arrange
        _reader.Setup(x => x.Read()).Returns(false);
        _reader.Setup(x => x.NextResult()).Returns(false);

        // Action
        var result = await CreateRepository().GetDataList(_metaModel, _currencyList, LanguageCode);

        // Assert
        Assert.Empty(result.DataList);
        Assert.Equal(0, result.TotalRows);
    }

    private CustomerTurnoverRepository CreateRepository()
    {
        SetupObjectDb();
        return new CustomerTurnoverRepository(_balanceUpService.Object, _cacheRefAdapter.Object, _productService.Object);
    }

    private void SetupObjectDb()
    {
        _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(_reader.Object));

        DbSettingProviderManager.StartTestingSession().Run();
        ObjectDbFactory.SwitchToTestingMode();
        ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
    }
}