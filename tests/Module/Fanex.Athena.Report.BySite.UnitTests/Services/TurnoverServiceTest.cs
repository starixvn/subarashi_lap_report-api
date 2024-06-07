using System.Linq;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.UnitTests.Common;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class TurnoverServiceTest
    {
        private readonly Mock<ITurnoverRepository> _repository;

        private readonly TurnoverService _service;

        public TurnoverServiceTest()
        {
            _repository = new Mock<ITurnoverRepository>();
            _service = new TurnoverService(_repository.Object);
        }

        [Fact]
        public void SumTotalData_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var dataList = FakeHelper.CreateMany<TurnoverDto>().ToList();

            // Action
            var result = _service.SumTotalData(dataList);

            // Assert
            Assert.Equal(dataList.Sum(p => p.Deposit), result.Deposit);
            Assert.Equal(dataList.Sum(p => p.Withdrawal), result.Withdrawal);
            Assert.Equal(dataList.Sum(p => p.Bonus), result.Bonus);
            Assert.Equal(dataList.Sum(p => p.Adjustment), result.Adjustment);
            Assert.Equal(dataList.Sum(p => p.Promotion), result.Promotion);
            Assert.Equal(dataList.Sum(p => p.Turnover), result.Turnover);
            Assert.Equal(dataList.Sum(p => p.DrawGame), result.DrawGame);
            Assert.Equal(dataList.Sum(p => p.NetTurnover), result.NetTurnover);
            Assert.Equal(dataList.Sum(p => p.PendingFunds), result.PendingFunds);
            Assert.Equal(dataList.Sum(p => p.WinLoss), result.WinLoss);
            Assert.Equal(dataList.Sum(p => p.PlayerComm), result.PlayerComm);
            Assert.Equal(dataList.Sum(p => p.ClosingBalance), result.ClosingBalance);
            Assert.Equal(dataList.Sum(p => p.AffiliateTransfer), result.AffiliateTransfer);
            Assert.Equal(dataList.Sum(p => p.Transfer3rd), result.Transfer3rd);
            Assert.Equal(dataList.Sum(p => p.TurnoverBA), result.TurnoverBA);
            Assert.Equal(dataList.Sum(p => p.CommBA), result.CommBA);
            Assert.Equal(dataList.Sum(p => p.AmountDueToBA), result.AmountDueToBA);
        }
    }
}
