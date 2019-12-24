using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Fanex.RemotingRef;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class PromotionPrizeRepositoryTest
    {
        private readonly BalanceUp _balance = new BalanceUp
        {
            MoveBetsDay = new DateTime(2016, 8, 10),
            BalanceUpDay = new DateTime(2016, 8, 31)
        };

        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Mock<ICurrencyService> _currencyService;
        private readonly Mock<IRemotingRefService> _remotingRefService;

        private const string BetTypeName = "BetTypeName";
        private const string ProductCode = "SB";

        public PromotionPrizeRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(b => b.GetBalanceUpAsync()).Returns(Task.FromResult(_balance));

            _currencyService = new Mock<ICurrencyService>();
            _currencyService.Setup(c => c.GetCurrencyListAsync()).Returns(Task.FromResult(_fixture.CreateMany<Currency>()));

            _remotingRefService = new Mock<IRemotingRefService>();
            _remotingRefService.Setup(x => x.GetBetTypeName(BetTypeName, Thread.CurrentThread.CurrentUICulture.Name)).Returns(BetTypeName);
        }

        [Fact]
        public async Task GetData_WhenCall_PassParameterCorrectly()
        {
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            var objectDb = CreateObjectDb(reader);

            var repository = new PromotionPrizeRepository(_balanceUpService.Object, _currencyService.Object, _remotingRefService.Object);

            // Action
            await repository.GetData(metaModel);

            // Assert
            objectDb.Verify(x => x.SetParameterValue("AgentId", metaModel.AgentId), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("UserName", metaModel.UserName), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("FromDate", metaModel.FromDate), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("ToDate", metaModel.ToDate), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("MoveBetsDay", _balance.MoveBetsDay), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("DefaultBaseCurrency", metaModel.DefaultBaseCurrency), Times.Once);
        }

        [Fact]
        public async Task GetData_WhenCall_ReturnCorrectData()
        {
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, true, false, true, false }).Dequeue);
            reader.Setup(r => r.NextResult()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);

            var expectedSummaryDataItem = _fixture.Create<PromotionPrize>();
            var expectedPromitionPrizeData = _fixture.Create<PromotionPrize>();
            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "ProductCode", ProductCode },
                { "BetType", BetTypeName },
                { "SumAmount", expectedSummaryDataItem.Amount },
                { "CurrencyId", 1 },
                { "UserName", expectedPromitionPrizeData.SystemId },
                { "NickName", expectedPromitionPrizeData.ExternalId },
                { "Amount", expectedPromitionPrizeData.Amount },
                { "Date", expectedPromitionPrizeData.Date }
            };

            SetUpData(reader, readerDataMapping);

            CreateObjectDb(reader);

            var repository = new PromotionPrizeRepository(_balanceUpService.Object, _currencyService.Object, _remotingRefService.Object);

            // Action
            var result = await repository.GetData(metaModel);
            var resultData = result[ProductCode];

            // Assert
            Assert.Equal(2, resultData.SummaryTotal.Count);
            Assert.Equal(BetTypeName, resultData.SummaryTotal[0].PromotionType);
            Assert.Equal(expectedSummaryDataItem.Amount, resultData.SummaryTotal[0].Amount);

            Assert.Equal(expectedSummaryDataItem.Amount * 2, resultData.GrandTotal);

            Assert.Equal(expectedPromitionPrizeData.SystemId, resultData.Data[0].SystemId);
            Assert.Equal(expectedPromitionPrizeData.ExternalId, resultData.Data[0].ExternalId);
            Assert.Equal(expectedPromitionPrizeData.Amount, resultData.Data[0].Amount);
            Assert.Equal(expectedPromitionPrizeData.Date, resultData.Data[0].Date);
            Assert.Equal(BetTypeName, resultData.Data[0].PromotionType);
        }

        private static void SetUpData(Mock<IDataReader> reader, IDictionary<string, object> expectedData)
        {
            reader.Setup(r => r[It.IsAny<string>()]).Returns<string>(x => expectedData.ContainsKey(x) ? expectedData[x] : string.Empty);
        }

        private static Mock<IObjectDb> CreateObjectDb(Mock<IDataReader> mockDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReader(It.IsAny<object>(), It.IsAny<DbTransaction>()))
                .Returns(mockDataReader.Object);

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }
    }
}