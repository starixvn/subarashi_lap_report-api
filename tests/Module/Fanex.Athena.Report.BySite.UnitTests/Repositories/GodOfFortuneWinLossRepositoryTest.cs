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
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class GodOfFortuneWinLossRepositoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        public GodOfFortuneWinLossRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();
        }

        [Fact]
        public async Task GetSummaryWinloss_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>()
                .With(x => x.ReportBy, ReportByType.Monthly)
                .Create();

            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, false, true, false, true, false }).Dequeue);

            var expectedDataItem = _fixture.Create<GodOfFortuneWinLoss>();
            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "discount", expectedDataItem.Discount },
                { "winlost", expectedDataItem.CustomerWinLoss },
                { "LicenseeWL", expectedDataItem.LicenseeWinLoss },
                { "betcount", expectedDataItem.TotalTransaction },
                { "turnover", expectedDataItem.TurnOver },
                { "currency", 1 },
                { "username", expectedDataItem.SystemId },
                { "nickname", expectedDataItem.ExternalId },
                { "CustId", expectedDataItem.CustomerId },
                { "reporttype", "20170101" }
            };
            SetUpData(reader, readerDataMapping);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetSummaryWinloss(metaModel);
            var firstOriginalItem = result.DataList.First();

            // Assert
            Assert.Single(result.DataList);
            Assert.Equal(expectedDataItem.LicenseeWinLoss, firstOriginalItem.LicenseeWinLoss);
            Assert.Equal(expectedDataItem.CustomerWinLoss, firstOriginalItem.CustomerWinLoss);
            Assert.Equal(expectedDataItem.TurnOver, firstOriginalItem.TurnOver);
            Assert.Equal(expectedDataItem.Discount, firstOriginalItem.Discount);
        }

        [Fact]
        public async Task GetAgentWinlossDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>()
                .With(x => x.ReportBy, ReportByType.Monthly)
                .Create();

            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, false, true, false, true, false }).Dequeue);

            var expectedDataItem = _fixture.Create<GodOfFortuneWinLoss>();
            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "discount", expectedDataItem.Discount },
                { "winlost", expectedDataItem.CustomerWinLoss },
                { "LicenseeWL", expectedDataItem.LicenseeWinLoss },
                { "betcount", expectedDataItem.TotalTransaction },
                { "turnover", expectedDataItem.TurnOver },
                { "currency", 1 },
                { "username", expectedDataItem.SystemId },
                { "nickname", expectedDataItem.ExternalId },
                { "CustId", expectedDataItem.CustomerId }
            };
            SetUpData(reader, readerDataMapping);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetAgentWinlossDetail(metaModel);
            var firstOriginalItem = result.DataList.First();

            // Assert
            Assert.Single(result.DataList);
            Assert.Equal(expectedDataItem.LicenseeWinLoss, firstOriginalItem.LicenseeWinLoss);
            Assert.Equal(expectedDataItem.CustomerWinLoss, firstOriginalItem.CustomerWinLoss);
            Assert.Equal(expectedDataItem.TurnOver, firstOriginalItem.TurnOver);
            Assert.Equal(expectedDataItem.Discount, firstOriginalItem.Discount);
            Assert.Equal(metaModel.DrillDownFromDate, firstOriginalItem.Date);
        }

        [Fact]
        public async Task GetCustomerWinlossDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>()
                .With(x => x.ReportBy, ReportByType.Monthly)
                .Create();

            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, false, true, false, true, false }).Dequeue);

            var expectedDataItem = _fixture.Create<GodOfFortuneWinLoss>();
            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "discount", expectedDataItem.Discount },
                { "winlost", expectedDataItem.CustomerWinLoss },
                { "LicenseeWL", expectedDataItem.LicenseeWinLoss },
                { "betcount", expectedDataItem.TotalTransaction },
                { "turnover", expectedDataItem.TurnOver },
                { "currency", 1 },
                { "username", expectedDataItem.SystemId },
                { "nickname", expectedDataItem.ExternalId },
                { "CustId", expectedDataItem.CustomerId }
            };
            SetUpData(reader, readerDataMapping);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetCustomerWinlossDetail(metaModel);
            var firstOriginalItem = result.DataList.First();

            // Assert
            Assert.Single(result.DataList);
            Assert.Equal(expectedDataItem.LicenseeWinLoss, firstOriginalItem.LicenseeWinLoss);
            Assert.Equal(expectedDataItem.CustomerWinLoss, firstOriginalItem.CustomerWinLoss);
            Assert.Equal(expectedDataItem.TurnOver, firstOriginalItem.TurnOver);
            Assert.Equal(expectedDataItem.Discount, firstOriginalItem.Discount);
            Assert.Equal(metaModel.DrillDownFromDate, firstOriginalItem.Date);
        }

        public static GodOfFortuneWinLossRepository CreateRepository()
        {
            var balance = new BalanceUp
            {
                MoveBetsDay = new DateTime(2016, 8, 10),
                BalanceUpDay = new DateTime(2016, 8, 31)
            };

            var balanceUpService = new Mock<IBalanceUpService>();
            var contextService = new Mock<IContextService>();
            contextService.Setup(_ => _.CurrencyList).Returns(new Dictionary<int, string>());

            balanceUpService.Setup(b => b.GetBalanceUpAsync()).Returns(Task.FromResult(balance));

            return new GodOfFortuneWinLossRepository(balanceUpService.Object, contextService.Object);
        }

        private static void SetUpData(Mock<IDataReader> reader, IDictionary<string, object> expectedData)
        {
            reader.Setup(r => r[It.IsAny<string>()]).Returns<string>(x => expectedData.ContainsKey(x) ? expectedData[x] : string.Empty);
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