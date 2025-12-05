namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.MetaModels;
    using BySite.Repositories;
    using Common.Model.DTO;
    using Common.Services.Interfaces;
    using Data;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class SubsidiaryPerformanceRepositoryTest
    {
        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Fixture _fixture;
        private readonly Mock<IObjectDb> _objectDb;
        private readonly Mock<IDataReader> _reader;

        public SubsidiaryPerformanceRepositoryTest()
        {
            _fixture = new Fixture();
            _balanceUpService = new Mock<IBalanceUpService>();
            _objectDb = new Mock<IObjectDb>();
            _reader = new Mock<IDataReader>();
            DbSettingProviderManager
                .StartTestingSession()
                .Run();
        }

        [Fact]
        public async Task GetData_WithValidParameters_CallsBalanceUpService()
        {
            var metaModel = FakeHelper.Create<SubsidiaryPerformanceMetaModel>();
            var currencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDb();

            await repository.GetData(metaModel, currencyList);

            _balanceUpService.Verify(x => x.GetBalanceUpAsync(), Times.Once);
        }

        [Fact]
        public async Task GetData_WithConvertAll_MapsEffectiveBettingValueFromBase()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                FromDate = DateTime.Now.AddDays(-7),
                ToDate = DateTime.Now,
                ConvertAll = true,
                BaseCurrency = "USD"
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDbWithData(800m, 750m);

            var result = await repository.GetData(metaModel, currencyList);

            // Update the line to use the fully qualified name if necessary
            var resultList = new List<BySite.DTOs.SubsidiaryPerformance.SubsidiaryPerformance>(result);
            Assert.Single(resultList);
            Assert.Equal(800m, resultList[0].EffectiveBettingValue);
            Assert.Equal(800m, resultList[0].BaseCurrencyData.EffectiveBettingValue);
        }

        [Fact]
        public async Task GetData_WithoutConvertAll_MapsEffectiveBettingValueFromNonBase()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                FromDate = DateTime.Now.AddDays(-7),
                ToDate = DateTime.Now,
                ConvertAll = false,
                BaseCurrency = "USD"
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDbWithData(800m, 750m);

            var result = await repository.GetData(metaModel, currencyList);

            var resultList = new List<BySite.DTOs.SubsidiaryPerformance.SubsidiaryPerformance>(result);
            Assert.Single(resultList);
            Assert.Equal(750m, resultList[0].EffectiveBettingValue);
            Assert.Equal(800m, resultList[0].BaseCurrencyData.EffectiveBettingValue);
        }

        [Fact]
        public async Task GetDetailData_WithValidParameters_CallsBalanceUpService()
        {
            var metaModel = FakeHelper.Create<SubsidiaryPerformanceMetaModel>();
            var currencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDb();

            await repository.GetDetailData(metaModel, currencyList);

            _balanceUpService.Verify(x => x.GetBalanceUpAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDetailData_WithConvertAll_IncludesEffectiveBettingValueInTotal()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                FromDate = DateTime.Now.AddDays(-7),
                ToDate = DateTime.Now,
                ConvertAll = true,
                BaseCurrency = "USD"
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDbWithDetailData(500m, 450m);

            var result = await repository.GetDetailData(metaModel, currencyList);

            Assert.NotNull(result.TotalData);
            Assert.Equal(500m, result.TotalData.EffectiveBettingValue);
        }

        [Fact]
        public async Task GetDetailData_WithoutConvertAll_IncludesEffectiveBettingValueFromBase()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                FromDate = DateTime.Now.AddDays(-7),
                ToDate = DateTime.Now,
                ConvertAll = false,
                BaseCurrency = "USD"
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDbWithDetailData(500m, 450m);

            var result = await repository.GetDetailData(metaModel, currencyList);

            Assert.NotNull(result.TotalData);
            Assert.Equal(500m, result.TotalData.EffectiveBettingValue);
        }

        [Fact]
        public void Constructor_WithBalanceUpService_SetsService()
        {
            var repository = CreateRepository();

            Assert.NotNull(repository);
        }

        private SubsidiaryPerformanceRepository CreateRepository()
        {
            return new SubsidiaryPerformanceRepository(_balanceUpService.Object);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
        }

        private void SetupObjectDbWithData(decimal betEffectiveBase, decimal betEffective)
        {
            var readCount = 0;
            _reader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
            _reader.Setup(r => r["Currency"]).Returns(1);
            _reader.Setup(r => r["ProductCode"]).Returns("SB");
            _reader.Setup(r => r["recommend"]).Returns(1);
            _reader.Setup(r => r["BetCount"]).Returns(100);
            _reader.Setup(r => r["TurnoverBase"]).Returns(1000m);
            _reader.Setup(r => r["Turnover"]).Returns(900m);
            _reader.Setup(r => r["WinlostBase"]).Returns(50m);
            _reader.Setup(r => r["Winlost"]).Returns(45m);
            _reader.Setup(r => r["BetEffectiveBase"]).Returns(betEffectiveBase);
            _reader.Setup(r => r["BetEffective"]).Returns(betEffective);
            _reader.Setup(r => r["LicenseeWLBase"]).Returns(-50m);
            _reader.Setup(r => r["LicenseeWL"]).Returns(-45m);

            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
        }

        private void SetupObjectDbWithDetailData(decimal betEffectiveBase, decimal betEffective)
        {
            var readCount = 0;
            _reader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
            _reader.Setup(r => r["Currency"]).Returns(1);
            _reader.Setup(r => r["username"]).Returns("user1");
            _reader.Setup(r => r["nickname"]).Returns("nick1");
            _reader.Setup(r => r["BetCount"]).Returns(100);
            _reader.Setup(r => r["TurnoverBase"]).Returns(1000m);
            _reader.Setup(r => r["Turnover"]).Returns(900m);
            _reader.Setup(r => r["WinlostBase"]).Returns(50m);
            _reader.Setup(r => r["Winlost"]).Returns(45m);
            _reader.Setup(r => r["BetEffectiveBase"]).Returns(betEffectiveBase);
            _reader.Setup(r => r["BetEffective"]).Returns(betEffective);
            _reader.Setup(r => r["LicenseeWLBase"]).Returns(-50m);
            _reader.Setup(r => r["LicenseeWL"]).Returns(-45m);

            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
        }
    }
}