using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System.Data;
using System.Data.Common;
using System.Linq;
using Fanex.Athena.Report.Core.Repositories;
using Fanex.Athena.Report.UnitTests.Common;
using Fanex.Athena.Report.Resources.Constants;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.UnitTests.Repositories
{
    public class BetTypeRepositoryTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;
        public BetTypeRepositoryTest()
        {
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _objectDb = new Mock<IObjectDb>();
            _reader = new Mock<IDataReader>();

            DbSettingProviderManager
               .StartTestingSession()
               .Run();
        }

        [Fact]
        public async Task GetBetTypesByProduct_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var productCodes = $"{Product.SB}";
            var licenseeId = FakeHelper.Create<int>();
            var languageCode = FakeHelper.Create<string>();
            var isHideSaba = FakeHelper.Create<bool>();

            var expectedBetTypeIds = new List<int> { 1 };
            _objectDb.Setup(x => x.QueryAsync<int>(It.IsAny<object>(), null, default)).ReturnsAsync(expectedBetTypeIds);

            var repository = GetRepository();

            // Action
            var result = (await repository.GetBetTypesByProduct(licenseeId, productCodes, languageCode, isHideSaba)).ToList();

            // Assert
            Assert.Equal(result[0].ID, expectedBetTypeIds[0]);
        }

        [Fact]
        public async Task GetBetTypesByProduct_BetTypeSportResult_ReturnResultWithSuffixName()
        {
            // Arrange
            var productCodes = $"{Product.SB}";
            var licenseeId = FakeHelper.Create<int>();
            var languageCode = FakeHelper.Create<string>();
            var isHideSaba = FakeHelper.Create<bool>();

            var expectedBetTypeIds = new List<int> { 1235 };
            _objectDb.Setup(x => x.QueryAsync<int>(It.IsAny<object>(), null, default)).ReturnsAsync(expectedBetTypeIds);

            var suffix = "Any";
            var expectedSuffix = $" ({suffix})";
            _cacheRefAdapter.Setup(x => x.GetSportName(It.IsAny<object>(), languageCode)).Returns(suffix);

            var repository = GetRepository();

            // Action
            var result = (await repository.GetBetTypesByProduct(licenseeId, productCodes, languageCode, isHideSaba)).ToList();

            // Assert
            Assert.Equal(result[0].ID, expectedBetTypeIds[0]);
            Assert.Contains(expectedSuffix, result[0].Name);
        }

        [Fact]
        public async Task GetBetTypesByProduct_PresentProductCO_AssignNewCoBetType()
        {
            // Arrange
            var productCodes = $"{Product.SB},{Product.CO}";
            var licenseeId = FakeHelper.Create<int>();
            var languageCode = FakeHelper.Create<string>();
            var isHideSaba = FakeHelper.Create<bool>();

            var expectedBetTypeIds = new List<int>();
            _objectDb.Setup(x => x.QueryAsync<int>(It.IsAny<object>(), null, default)).ReturnsAsync(expectedBetTypeIds);

            var repository = GetRepository();

            // Action
            var result = (await repository.GetBetTypesByProduct(licenseeId, productCodes, languageCode, isHideSaba)).ToList();

            // Assert
            Assert.Equal(6, result.Count);
            _cacheRefAdapter.Verify(x => x.GetBettype(It.IsAny<int>()), Times.Exactly(6));
        }

        [Fact]
        public async Task GetBetTypesByProduct_EmptyProductCode_ReturnEmpty()
        {
            // Arrange
            var productCodes = FakeHelper.Create<string>();
            var licenseeId = FakeHelper.Create<int>();
            var languageCode = FakeHelper.Create<string>();
            var isHideSaba = FakeHelper.Create<bool>();

            var repository = GetRepository();

            // Action
            var result = (await repository.GetBetTypesByProduct(licenseeId, productCodes, languageCode, isHideSaba)).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBetTypeForProductsAsync_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var productCodes = FakeHelper.Create<string>();
            var licenseeId = FakeHelper.Create<int>();

            var expectedProductCodes = FakeHelper.Create<string>();
            var expectedLicenseeId = FakeHelper.Create<int>();

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x["ProductCode"]).Returns(expectedProductCodes);
            _reader.Setup(x => x["Bettype"]).Returns(expectedLicenseeId);
            var repository = GetRepository();

            // Action
            var result = (await repository.GetBetTypeForProductsAsync(licenseeId, productCodes)).ToList();

            // Assert
            _objectDb.Verify(x => x.SetParameterValue("licenseeid", licenseeId));
            _objectDb.Verify(x => x.SetParameterValue("productCodes", productCodes));

            Assert.Single(result);
            Assert.Equal(expectedLicenseeId, result[0].BetType);
            Assert.Equal(expectedProductCodes, result[0].ProductCode);
        }

        [Fact]
        public async Task GetBetTypeForProductsAsync_EmptyProductCode_ReturnEmpty()
        {
            // Arrange
            var productCodes = "";
            var licenseeId = FakeHelper.Create<int>();

            var repository = GetRepository();

            // Action
            var result = (await repository.GetBetTypeForProductsAsync(licenseeId, productCodes)).ToList();

            // Assert
            Assert.Empty(result);
        }


        private BetTypeRepository GetRepository()
        {
            SetupObjectDb();
            return new BetTypeRepository(_cacheRefAdapter.Object);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), default))
                     .ReturnsAsync(_reader.Object);

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);
        }
    }
}
