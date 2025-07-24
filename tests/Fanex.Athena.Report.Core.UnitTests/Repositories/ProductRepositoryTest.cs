using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories;
using Fanex.Data;
using Moq;
using System.Data;
using System.Data.Common;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Repositories
{
    using Resources.Constants;

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

            _objectDb = new Mock<IObjectDb>();
            _reader = new Mock<IDataReader>();
        }

        [Fact]
        public void GetProductAccessList_NoData_ReturnDefaultData()
        {
            // Arrange
            _reader.Setup(r => r.Read()).Returns(false);
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
            _reader.Setup(r => r.GetOrdinal("ProductID")).Returns(ProductIdIndex);
            _reader.Setup(r => r.GetInt32(ProductIdIndex)).Returns(expectedProductAccess.ProductID);

            const int ProductPrefixIndex = 1;
            _reader.Setup(r => r.GetOrdinal("productprefix")).Returns(ProductPrefixIndex);
            _reader.Setup(r => r.GetString(ProductPrefixIndex)).Returns("NG");

            const int CommIndex = 2;
            _reader.Setup(r => r.GetOrdinal("comm")).Returns(CommIndex);
            _reader.Setup(r => r.GetBoolean(CommIndex)).Returns(expectedProductAccess.AllowCommission);

            const int WinLossIndex = 3;
            _reader.Setup(r => r.GetOrdinal("winloss")).Returns(WinLossIndex);
            _reader.Setup(r => r.GetBoolean(WinLossIndex)).Returns(expectedProductAccess.AllowWinloss);

            const int IsOfferedSingleWalletIndex = 4;
            _reader.Setup(r => r.GetOrdinal("IsOfferedSingleWallet"))
                   .Returns(IsOfferedSingleWalletIndex);
            _reader.Setup(r => r.GetBoolean(IsOfferedSingleWalletIndex))
                   .Returns(expectedProductAccess.IsOfferedSingleWallet);

            _reader.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            SetupObjectDb();

            var repository = new ProductRepository();

            // Action
            var result = repository.GetProductAccessList(LicenseeId);

            // Assert
            Assert.Equal(0, result[0].ProductID);
            Assert.Equal("SB", result[0].ProductCode);
            Assert.Equal(nameof(Product.SB), result[0].ProductName);

            Assert.Equal(expectedProductAccess.ProductID, result[1].ProductID);
            Assert.Equal(expectedProductAccess.ProductCode, result[1].ProductCode);
            Assert.Equal(expectedProductAccess.ProductName, result[1].ProductName);
            Assert.Equal(expectedProductAccess.AllowCommission, result[1].AllowCommission);
            Assert.Equal(expectedProductAccess.AllowWinloss, result[1].AllowWinloss);
        }

        private static ProductInfo GetFakeProductAccess() =>
            new()
            {
                ProductID = 1,
                ProductCode = "NG",
                AllowCommission = true,
                AllowWinloss = true
            };

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReader(It.IsAny<object>(), It.IsAny<DbTransaction>()))
                     .Returns(_reader.Object);

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
        }
    }
}