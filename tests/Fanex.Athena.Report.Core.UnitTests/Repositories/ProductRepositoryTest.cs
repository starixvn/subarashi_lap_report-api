using System.Data;
using System.Data.Common;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories;
using Fanex.Athena.Report.UnitTests.Common.ServiceMoqExtensions;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Repositories
{
    public class ProductRepositoryTest
    {
        private const int LicenseeId = 1;

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        public ProductRepositoryTest()
        {
            DbSettingProviderManager
               .StartTestingSession()
               .Run();

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public void GetProductAccessList_NoData_ReturnDefaultData()
        {
            // Arrange
            _reader.Setup(x => x.Read()).Returns(false);
            SetupObjectDb();

            var repository = new ProductRepository();

            // Action
            var result = repository.GetProductAccessList(It.IsAny<int>());

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetProductAccessList_WhenCall_DataReturnedCorrectly()
        {
            // Arrange
            var expectedProductAccess = GetFakeProductAccess();

            const int ProductIdIndex = 0;
            _reader.Setup(x => x.GetOrdinal(It.Is<string>(_ => _.Equals("ProductID")))).Returns(ProductIdIndex);
            _reader.Setup(x => x.GetInt32(ProductIdIndex)).Returns(expectedProductAccess.ProductID);
            const int ProductPrefixIndex = 1;
            _reader.Setup(x => x.GetOrdinal(It.Is<string>(_ => _.Equals("productprefix")))).Returns(ProductPrefixIndex);
            _reader.Setup(x => x.GetString(ProductPrefixIndex)).Returns("NG");
            const int CommIndex = 2;
            _reader.Setup(x => x.GetOrdinal(It.Is<string>(_ => _.Equals("comm")))).Returns(CommIndex);
            _reader.Setup(x => x.GetBoolean(CommIndex)).Returns(expectedProductAccess.AllowCommission);
            const int WinLossIndex = 3;
            _reader.Setup(x => x.GetOrdinal(It.Is<string>(_ => _.Equals("winloss")))).Returns(WinLossIndex);
            _reader.Setup(x => x.GetBoolean(WinLossIndex)).Returns(expectedProductAccess.AllowWinloss);
            const int IsOfferedSingleWalletIndex = 4;
            _reader.Setup(x => x.GetOrdinal(It.Is<string>(_ => _.Equals("IsOfferedSingleWallet"))))
                   .Returns(IsOfferedSingleWalletIndex);
            _reader.Setup(x => x.GetBoolean(IsOfferedSingleWalletIndex))
                   .Returns(expectedProductAccess.IsOfferedSingleWallet);

            _reader.SetupToggleRead();
            SetupObjectDb();

            var repository = new ProductRepository();

            // Action
            var result = repository.GetProductAccessList(LicenseeId);

            // Assert
            Assert.Equal(0, result[0].ProductID);
            Assert.Equal("SB", result[0].ProductCode);

            Assert.Equal(expectedProductAccess.ProductID, result[1].ProductID);
            Assert.Equal(expectedProductAccess.ProductCode, result[1].ProductCode);
            Assert.Equal(expectedProductAccess.ProductName, result[1].ProductName);
            Assert.Equal(expectedProductAccess.AllowCommission, result[1].AllowCommission);
            Assert.Equal(expectedProductAccess.AllowWinloss, result[1].AllowWinloss);
        }

        private static ProductInfo GetFakeProductAccess() =>
            new ProductInfo
            {
                ProductID = 1,
                ProductName = "Number Game",
                ProductCode = "NG",
                AllowCommission = true,
                AllowWinloss = true
            };

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReader(It.IsAny<object>(), It.IsAny<DbTransaction>()))
                     .Returns(_reader.Object);

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);
        }
    }
}
