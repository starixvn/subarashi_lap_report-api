namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BySite.MetaModels;
    using BySite.Repositories;
    using Common.Model.DTO;
    using Data;
    using Moq;
    using Xunit;

    public class DailyBetDetailsRepositoryTests
    {
        private readonly Mock<IObjectDb> _mockObjectDb;
        private readonly Mock<IDataReader> _mockDataReader;
        private readonly DailyBetDetailsRepository _repository;

        public DailyBetDetailsRepositoryTests()
        {
            _mockObjectDb = new Mock<IObjectDb>();
            _mockDataReader = new Mock<IDataReader>();
            _repository = new DailyBetDetailsRepository();

            SetupObjectDbFactory();
        }

        private void SetupObjectDbFactory()
        {
            DbSettingProviderManager.StartTestingSession().Run();
            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((name, connectionString, command) => _mockObjectDb.Object);
        }

        [Fact]
        public async Task GetDataAsync_WithValidData_ReturnsCorrectResult()
        {
            var metaModel = new DailyBetDetailsMetaModel
            {
                SiteName = "TestSite",
                Date = DateTime.Today,
                CurrencyIdList = "1,2",
                CurrencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } },
                PageIndex = 0,
                PageSize = 10
            };

            var auditCurrencyList = new List<AuditCurrency>
            {
                new AuditCurrency { Name = "USD", HardCodeName = "US Dollar" }
            };

            SetupDataReader();
            _mockObjectDb.Setup(db => db.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockDataReader.Object);
            _mockObjectDb.Setup(db => db.GetParameterValue("TotalRow")).Returns(1);

            var result = await _repository.GetDataAsync(metaModel, auditCurrencyList);

            Assert.NotNull(result);
            Assert.Single(result.DataList);
            Assert.Equal(1, result.TotalRows);

            var item = result.DataList.First();
            Assert.Equal("US Dollar", item.CurrencyName);
            Assert.Equal("REF123", item.RefNo);
            Assert.Equal("TestBrand", item.BrandUrl);
            Assert.Equal("Sport", item.GameType);
            Assert.Equal(100m, item.DebitAmount);
            Assert.Equal(50m, item.BetAmount);
            Assert.Equal(-25m, item.NetAmount);
            Assert.Equal("1", item.RIdx);
        }

        [Fact]
        public async Task GetDataAsync_WithNoAuditCurrencyMatch_UsesCurrencyName()
        {
            var metaModel = new DailyBetDetailsMetaModel
            {
                SiteName = "TestSite",
                Date = DateTime.Today,
                CurrencyIdList = "1",
                CurrencyList = new Dictionary<int, string> { { 1, "CNY" } },
                PageIndex = 0,
                PageSize = 10
            };

            var auditCurrencyList = new List<AuditCurrency>
            {
                new AuditCurrency { Name = "USD", HardCodeName = "US Dollar" }
            };

            SetupDataReader("CNY");
            _mockObjectDb.Setup(db => db.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockDataReader.Object);
            _mockObjectDb.Setup(db => db.GetParameterValue("TotalRow")).Returns(1);

            var result = await _repository.GetDataAsync(metaModel, auditCurrencyList);

            Assert.NotNull(result);
            Assert.Single(result.DataList);
            var item = result.DataList.First();
            Assert.Equal("CNY", item.CurrencyName);
        }

        [Fact]
        public async Task GetDataAsync_WithEmptyData_ReturnsEmptyResult()
        {
            var metaModel = new DailyBetDetailsMetaModel
            {
                SiteName = "TestSite",
                Date = DateTime.Today,
                CurrencyIdList = "1",
                CurrencyList = new Dictionary<int, string> { { 1, "USD" } },
                PageIndex = 0,
                PageSize = 10
            };

            var auditCurrencyList = new List<AuditCurrency>();

            _mockDataReader.Setup(r => r.Read()).Returns(false);
            _mockObjectDb.Setup(db => db.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockDataReader.Object);
            _mockObjectDb.Setup(db => db.GetParameterValue("TotalRow")).Returns(0);

            var result = await _repository.GetDataAsync(metaModel, auditCurrencyList);

            Assert.NotNull(result);
            Assert.Empty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataAsync_WithIvoryBetAndRefNo_SetsSearchRefNoParameter()
        {
            var metaModel = new DailyBetDetailsMetaModel
            {
                SiteName = "IvoryBet",
                Date = DateTime.Today,
                RefNo = 12345,
                CurrencyIdList = "1",
                CurrencyList = new Dictionary<int, string> { { 1, "USD" } },
                PageIndex = 0,
                PageSize = 10
            };

            var auditCurrencyList = new List<AuditCurrency>();

            _mockDataReader.Setup(r => r.Read()).Returns(false);
            _mockObjectDb.Setup(db => db.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockDataReader.Object);
            _mockObjectDb.Setup(db => db.GetParameterValue("TotalRow")).Returns(0);

            await _repository.GetDataAsync(metaModel, auditCurrencyList);

            _mockObjectDb.Verify(db => db.SetParameterValue("SiteName", "IvoryBet"), Times.Once);
            _mockObjectDb.Verify(db => db.SetParameterValue("SearchRefNo", 12345L), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WithNonIvoryBetSite_DoesNotSetSearchRefNoParameter()
        {
            var metaModel = new DailyBetDetailsMetaModel
            {
                SiteName = "OtherSite",
                Date = DateTime.Today,
                RefNo = 12345,
                CurrencyIdList = "1",
                CurrencyList = new Dictionary<int, string> { { 1, "USD" } },
                PageIndex = 0,
                PageSize = 10
            };

            var auditCurrencyList = new List<AuditCurrency>();

            _mockDataReader.Setup(r => r.Read()).Returns(false);
            _mockObjectDb.Setup(db => db.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockDataReader.Object);
            _mockObjectDb.Setup(db => db.GetParameterValue("TotalRow")).Returns(0);

            await _repository.GetDataAsync(metaModel, auditCurrencyList);

            _mockObjectDb.Verify(db => db.SetParameterValue("SiteName", "OtherSite"), Times.Once);
            _mockObjectDb.Verify(db => db.SetParameterValue("SearchRefNo", It.IsAny<long>()), Times.Never);
        }

        private void SetupDataReader(string currencyCode = "USD")
        {
            var sequence = new MockSequence();
            _mockDataReader.InSequence(sequence).Setup(r => r.Read()).Returns(true);
            _mockDataReader.InSequence(sequence).Setup(r => r.Read()).Returns(false);

            _mockDataReader.Setup(r => r["CurrencyCode"]).Returns(currencyCode);
            _mockDataReader.Setup(r => r["RefNo"]).Returns("REF123");
            _mockDataReader.Setup(r => r["SettlementDate"]).Returns(DateTime.Today);
            _mockDataReader.Setup(r => r["BrandUrl"]).Returns("TestBrand");
            _mockDataReader.Setup(r => r["GameType"]).Returns("Sport");
            _mockDataReader.Setup(r => r["DebitAmount"]).Returns(100m);
            _mockDataReader.Setup(r => r["Turnover"]).Returns(50m);
            _mockDataReader.Setup(r => r["WinLoss"]).Returns(25m);
            _mockDataReader.Setup(r => r["rIdx"]).Returns("1");
        }
    }
}