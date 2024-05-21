using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class NewCashOutWinLossRepositoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Mock<IContextService> _contextService;

        private readonly BalanceUp _balance = new BalanceUp
        {
            MoveBetsDay = new DateTime(2016, 8, 10),
            BalanceUpDay = new DateTime(2016, 8, 31)
        };

        private readonly Dictionary<string, object> _expectedCommonData = new()
        {
            { "Currency", 7 },
            { "reporttype", "20220101" },
            { "betcount", 5 },
            { "BasedTurnover", 90 },
            { "BasedWinLost", 80 },
            { "BasedLicenseeWL", 100 },
            { "BasedPlayerComm", 200 },
            { "BasedBAComm", 300 },
            { "BasedAmtDuetoBA", 200 },
            { "Turnover", 100 },
            { "WinLost", 200 },
            { "LicenseeWL", 200 },
            { "PlayerComm", 300 },
            { "BAComm", 200 },
            { "AmtDuetoBA", 100 }
        };

        public NewCashOutWinLossRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(b => b.GetBalanceUpAsync()).Returns(Task.FromResult(_balance));

            _contextService = new Mock<IContextService>();
            _contextService.Setup(c => c.CurrencyList).Returns(_fixture.CreateMany<KeyValuePair<int, string>>(10).ToDictionary(x => x.Key, x => x.Value));
        }

        public NewCashOutWinLossRepository CreateRepository()
        {
            return new NewCashOutWinLossRepository(_balanceUpService.Object, _contextService.Object);
        }

        [Fact]
        public async Task GetNewCashOutWinLossMainData_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.SortBy, "Turnover")
                .With(x => x.SortOrder, "asc")
                .Create();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetNewCashOutWinLossMainData(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetNewCashOutWinLossMainData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetNewCashOutWinLossMainData(metaModel);

            // Assert
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetNewCashOutWinLossMainData_ErrCode_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            reader.Setup(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("ErrCode");

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetNewCashOutWinLossMainData(metaModel);

            // Assert
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetNewCashOutWinLossMemberData_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var expectedMemberData = new Dictionary<string, object>
            {
                { "CustId", 199 },
                { "username", "Kelly" },
                { "Nickname", "Kelly12Bet" }
            };
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.PageSize, 20)
                .With(x => x.PageIndex, 1)
                .With(x => x.SortBy, "Turnover")
                .With(x => x.SortOrder, "asc")
                .Create();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedMemberData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetNewCashOutWinLossMemberData(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            AssertCommonData(data);
            Assert.Equal(199, data.CustId);
            Assert.Equal("Kelly", data.SystemId);
            Assert.Equal("Kelly12Bet", data.ExternalId);
        }

        [Fact]
        public async Task GetNewCashOutWinLossMemberData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetNewCashOutWinLossMemberData(metaModel);

            // Assert
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetNewCashOutWinLossMemberData_ErrCode_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            reader.Setup(r => r.FieldCount).Returns(1);
            reader.Setup(r => r.GetName(0)).Returns("ErrCode");

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetNewCashOutWinLossMemberData(metaModel);

            // Assert
            Assert.Equal(0, result.TotalRows);
        }

        public static void AssertCommonData(CashOutWinLoss data)
        {
            Assert.Equal(7, data.Currency);
            Assert.Equal(5, data.TotalTransaction);
            Assert.Equal(100, data.BasedLicenseeWL);
            Assert.Equal(80, data.BasedCustomerWL);
            Assert.Equal(90, data.BasedTurnover);
            Assert.Equal(200, data.BasedCustomerComm);
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
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);
        }
    }
}