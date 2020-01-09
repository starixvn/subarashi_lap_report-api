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
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class PlatformWinLossRepositoryTest
    {
        private readonly BalanceUp _balance = new BalanceUp
        {
            MoveBetsDay = new DateTime(2016, 8, 10),
            BalanceUpDay = new DateTime(2016, 8, 31)
        };

        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Mock<ICurrencyService> _currencyService;

        public PlatformWinLossRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(b => b.GetBalanceUpAsync()).Returns(Task.FromResult(_balance));

            _currencyService = new Mock<ICurrencyService>();
            _currencyService.Setup(c => c.GetCurrencyListAsync()).Returns(Task.FromResult(_fixture.CreateMany<Currency>()));
        }

        [Fact]
        public async Task GetSummaryDataAsync_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, true, false }).Dequeue);

            var expectedDataItem = _fixture.Create<PlatformWinLossSummary>();
            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "PlatformType", PlatformType.Asia },
                { "Turnover", expectedDataItem.Turnover },
                { "productprefix", "SB" },
                { "Betcount", expectedDataItem.TotalTransactions },
                { "Winlost", expectedDataItem.CustomerWinLoss },
                { "PlayerComm", expectedDataItem.CustomerComm },
                { "LicenseeWL", expectedDataItem.LicenseeWL }
            };
            SetUpData(reader, readerDataMapping);

            CreateObjectDb(reader);

            var repository = new PlatformWinLossRepository(_balanceUpService.Object, _currencyService.Object);

            // Action
            var result = await repository.GetSummaryDataAsync(metaModel);
            var firstItem = result.DataList.First();
            var secondItem = result.DataList.ElementAt(1);

            // Assert
            Assert.Equal(3, result.DataList.Count());
            Assert.Equal(PlatformType.Asia, firstItem.PlatformType);
            Assert.Equal(ViewResources.Asia, firstItem.Name);
            Assert.Equal(expectedDataItem.Turnover * 2, firstItem.Turnover);

            Assert.Equal(PlatformType.Asia.ToString(), secondItem.Parent);
            Assert.Equal(ViewResources.ResourceManager.GetString("SB"), secondItem.Name);
            Assert.Equal(expectedDataItem.Turnover, secondItem.Turnover);

            Assert.Equal(100, result.TotalData.TurnoverPercent);
        }

        [Fact]
        public async Task GetMemberData_WhenCall_ParametersPassedCorrectly()
        {
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            var objectDb = CreateObjectDb(reader);

            var repository = new PlatformWinLossRepository(_balanceUpService.Object, _currencyService.Object);

            // Action
            await repository.GetSummaryDataAsync(metaModel);

            // Assert
            objectDb.Verify(x => x.SetParameterValue("PlatformType", metaModel.PlatformType), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("movebetsday", _balance.MoveBetsDay), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("DrawFilter", metaModel.DrawFilter), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("FromDate", metaModel.FromDate), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("ToDate", metaModel.ToDate), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("BaseCurrency", metaModel.BaseCurrency), Times.Once);
        }

        [Fact]
        public async Task GetMemberData_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);

            var expectedItemData = _fixture.Create<PlatformWinLossMember>();

            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "Betcount", expectedItemData.TotalTransactions },
                { "Turnover", expectedItemData.Turnover },
                { "Winlost", expectedItemData.CustomerWinLoss },
                { "PlayerComm", expectedItemData.CustomerComm },
                { "LicenseeWL", expectedItemData.LicenseeWL },
                { "CommBA", expectedItemData.BACommission },
                { "AmtDueToBA", expectedItemData.AmtDueToBA },
                { "Currency", expectedItemData.CurrencyId },
                { "username", expectedItemData.SystemId },
                { "custid", expectedItemData.CustomerId },
                { "nickname", expectedItemData.ExternalId },
            };
            SetUpData(reader, readerDataMapping);

            CreateObjectDb(reader);

            var repository = new PlatformWinLossRepository(_balanceUpService.Object, _currencyService.Object);

            // Action
            var result = await repository.GetMemberDataAsync(metaModel).ConfigureAwait(false);
            var resultItemData = result.DataList.ElementAt(0);

            // Assert
            Assert.Single(result.DataList);

            Assert.Equal(1, resultItemData.Index);
            Assert.Equal(expectedItemData.TotalTransactions, resultItemData.TotalTransactions);
            Assert.Equal(expectedItemData.Turnover, resultItemData.Turnover);
            Assert.Equal(expectedItemData.CustomerWinLoss, resultItemData.CustomerWinLoss);
            Assert.Equal(expectedItemData.CustomerComm, resultItemData.CustomerComm);
            Assert.Equal(expectedItemData.LicenseeWL, resultItemData.LicenseeWL);
            Assert.Equal(expectedItemData.BACommission, resultItemData.BACommission);
            Assert.Equal(expectedItemData.AmtDueToBA, resultItemData.AmtDueToBA);
            Assert.Equal(expectedItemData.CurrencyId, resultItemData.CurrencyId);
            Assert.Equal(expectedItemData.SystemId, resultItemData.SystemId);
            Assert.Equal(expectedItemData.CustomerId, resultItemData.CustomerId);
            Assert.Equal(expectedItemData.ExternalId, resultItemData.ExternalId);

            Assert.Equal(expectedItemData.Turnover, result.TotalData.Turnover);
        }

        private static void SetUpData(Mock<IDataReader> reader, IDictionary<string, object> expectedData)
        {
            reader.Setup(r => r[It.IsAny<string>()]).Returns<string>(x => expectedData.ContainsKey(x) ? expectedData[x] : string.Empty);
        }

        private static Mock<IObjectDb> CreateObjectDb(Mock<IDataReader> mockDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }
    }
}