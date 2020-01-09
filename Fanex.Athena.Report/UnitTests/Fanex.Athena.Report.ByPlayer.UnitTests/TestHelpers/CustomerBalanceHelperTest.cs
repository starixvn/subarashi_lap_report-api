using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Data;
using Moq;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers
{
    public static class CustomerBalanceHelperTest
    {
        public static bool IsEqualCommonField(CustomerBalance expected, CustomerBalance actual)
        {
            return expected.OpeningBalance == actual.OpeningBalance
                && expected.Deposit == actual.Deposit
                && expected.Withdrawal == actual.Withdrawal
                && expected.Bonus == actual.Bonus
                && expected.Adjustment == actual.Adjustment
                && expected.Promotion == actual.Promotion
                && expected.Turnover == actual.Turnover
                && expected.DrawGame == actual.DrawGame
                && (expected.Turnover - expected.DrawGame) == (actual.Turnover - actual.DrawGame)
                && expected.PendingFunds == actual.PendingFunds
                && expected.WinLoss == actual.WinLoss
                && expected.PlayerComm == actual.PlayerComm
                && (expected.WinLoss + expected.PlayerComm) == (actual.WinLoss + actual.PlayerComm)
                && expected.AffiliateTransfer == actual.AffiliateTransfer
                && expected.ClosingBalance == actual.ClosingBalance
                && expected.TurnoverBA == actual.TurnoverBA
                && expected.CommBA == actual.CommBA
                && expected.AmountDueToBA == actual.AmountDueToBA;
        }

        public static Mock<IObjectDb> SetUpObjectDb(Mock<IDataReader> mockIDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockIDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }

        public static IList<ProductOption> GetProductOptions()
        {
            return new List<ProductOption>
            {
                new ProductOption { Code = "SB", Status = true },
                new ProductOption { Code = "NG", Status = false }
            };
        }

        public static void ConfigureReaderForResult(CustomerBalance expected, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["nickname"]).Returns(expected.ExternalId);
            reader.Setup(r => r["username"]).Returns(expected.SystemId);
            MapReader(expected, string.Empty, reader);
            reader.Setup(r => r["TurnOverBA"]).Returns(expected.TurnoverBA);
            reader.Setup(r => r["CommBA"]).Returns(expected.CommBA);
            reader.Setup(r => r["AmountDueToBA"]).Returns(expected.AmountDueToBA);
        }

        public static void ConfigureReaderForTotalData(CustomerBalance expected, int expectedTotalRow, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["TotalRows"]).Returns(expectedTotalRow);
            MapReader(expected, "sum", reader);
            reader.Setup(r => r["sumturnoverba"]).Returns(expected.TurnoverBA);
            reader.Setup(r => r["sumcommba"]).Returns(expected.CommBA);
            reader.Setup(r => r["sumamountduetoba"]).Returns(expected.AmountDueToBA);
        }

        public static void MapReader(CustomerBalance expected, string prefix, Mock<IDataReader> reader)
        {
            var openingBalance = prefix + "openingbalance";
            var deposit = prefix + "deposit";
            var withdrawal = prefix + "withdrawal";
            var bonus = prefix + "bonus";
            var adjustment = prefix + "adjustment";
            var turnover = prefix + "turnover";
            var drawGame = prefix + "drawgame";
            var outstanding = prefix + "outstanding";
            var winloss = prefix + "winloss";
            var playerComm = prefix + "playercomm";
            var closingBalance = prefix + "closingbalance";
            var affiliateTransfer = prefix + "affiliatetransfer";
            var promotion = prefix + "promotion";

            reader.Setup(r => r[openingBalance]).Returns(expected.OpeningBalance);
            reader.Setup(r => r[deposit]).Returns(expected.Deposit);
            reader.Setup(r => r[withdrawal]).Returns(expected.Withdrawal);
            reader.Setup(r => r[bonus]).Returns(expected.Bonus);
            reader.Setup(r => r[adjustment]).Returns(expected.Adjustment);
            reader.Setup(r => r[turnover]).Returns(expected.Turnover);
            reader.Setup(r => r[drawGame]).Returns(expected.DrawGame);
            reader.Setup(r => r[outstanding]).Returns(expected.PendingFunds);
            reader.Setup(r => r[winloss]).Returns(expected.WinLoss);
            reader.Setup(r => r[playerComm]).Returns(expected.PlayerComm);
            reader.Setup(r => r[closingBalance]).Returns(expected.ClosingBalance);
            reader.Setup(r => r[affiliateTransfer]).Returns(expected.AffiliateTransfer);
            reader.Setup(r => r[promotion]).Returns(expected.Promotion);
        }
    }
}