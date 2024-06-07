using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.UnitTests.Common;
using Fanex.Athena.Report.UnitTests.Common.ServiceMoqExtensions;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class TurnoverRepositoryTest
    {
        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        private ITurnoverRepository _repository;

        public TurnoverRepositoryTest()
        {
            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();

            CreateRepository();
        }

        [Fact]
        public async Task GetDataList_WhenCall_ParametersPassedCorrectly()
        {
            // Arrange
            var metaModel = new TurnoverMetaModel
            {
                Username = "abc",
                AgentListXml = "xml",
                ProductOptions = new List<ProductOption>
                {
                    new() { Code = "SB", Status = true, },
                    new() { Code = "NG", Status = false, }
                }
            };

            // Action
            await _repository.GetDataList(metaModel);

            // Assert
            _objectDb.Verify(db => db.SetParameterValue("username", metaModel.Username), Times.Once());
            _objectDb.Verify(db => db.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once());
            _objectDb.Verify(db => db.SetParameterValue("fromdate", metaModel.FromDate), Times.Once());
            _objectDb.Verify(db => db.SetParameterValue("todate", metaModel.ToDate), Times.Once());
            _objectDb.Verify(db => db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency), Times.Once());
            _objectDb.Verify(db => db.SetParameterValue("ProductList", "SB"), Times.Once());
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<TurnoverMetaModel>(x => x.BaseCurrency, "USD");
            var expected = new
            {
                winlostdate = FakeHelper.Create<DateTime>(),
                OpeningBalance = FakeHelper.Create<decimal>(),
                Deposit = FakeHelper.Create<decimal>(),
                Withdrawal = FakeHelper.Create<decimal>(),
                AffiliateTransfer = FakeHelper.Create<decimal>(),
                Bonus = FakeHelper.Create<decimal>(),
                Adjustment = FakeHelper.Create<decimal>(),
                Promotion = FakeHelper.Create<decimal>(),
                Turnover = FakeHelper.Create<decimal>(),
                PendingFund = FakeHelper.Create<decimal>(),
                WinLoss = FakeHelper.Create<decimal>(),
                ClosingBalance = FakeHelper.Create<decimal>(),
                Transfer3rd = FakeHelper.Create<decimal>(),
                DrawGame = FakeHelper.Create<decimal>(),
                TurnoverBA = FakeHelper.Create<decimal>(),
                CommBA = FakeHelper.Create<decimal>(),
                AmountDueToBA = FakeHelper.Create<decimal>(),
                PlayerComm = FakeHelper.Create<decimal>()
            };
            var expectedNetTurnover = expected.Turnover - expected.DrawGame;

            _reader.SetupToggleRead(new List<object> { expected, expected, });

            // Action
            var result = await _repository.GetDataList(metaModel);
            var netTurnover = result[0].Turnover - result[0].DrawGame;

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expected.OpeningBalance, result[0].OpeningBalance);
            Assert.Equal(expected.Deposit, result[0].Deposit);
            Assert.Equal(expected.Withdrawal, result[0].Withdrawal);
            Assert.Equal(expected.AffiliateTransfer, result[0].AffiliateTransfer);
            Assert.Equal(expected.Bonus, result[0].Bonus);
            Assert.Equal(expected.Adjustment, result[0].Adjustment);
            Assert.Equal(expected.Promotion, result[0].Promotion);
            Assert.Equal(expected.Turnover, result[0].Turnover);
            Assert.Equal(expected.PendingFund, result[0].PendingFunds);
            Assert.Equal(expected.WinLoss, result[0].WinLoss);
            Assert.Equal(expected.ClosingBalance, result[0].ClosingBalance);
            Assert.Equal(expected.Transfer3rd, result[0].Transfer3rd);
            Assert.Equal(expected.DrawGame, result[0].DrawGame);
            Assert.Equal(expected.TurnoverBA, result[0].TurnoverBA);
            Assert.Equal(expected.CommBA, result[0].CommBA);
            Assert.Equal(expected.AmountDueToBA, result[0].AmountDueToBA);
            Assert.Equal(expected.PlayerComm, result[0].PlayerComm);
            Assert.Equal(expectedNetTurnover, netTurnover);
        }

        private void CreateRepository()
        {
            SetupObjectDb();
            _repository = new TurnoverRepository();
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            DbSettingProviderManager.StartTestingSession().Run();
            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);
        }
    }
}
