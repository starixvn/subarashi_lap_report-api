using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services;
using Fanex.Data;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    public class CurrencyServiceTest
    {
        protected readonly IFixture fixture = new Fixture();

        private Mock<IMemoryCache> _cache;

        private Mock<ICacheEntry> _cacheEntry;

        public CurrencyServiceTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();
        }

        [Fact]
        public async Task GetCurrencyListAsync_DataNotInCache_CallDataAccess()
        {
            // Arrange
            var expectedResult = fixture.CreateMany<Currency>();
            var objectDb = CreateObjectDb();
            objectDb.Setup(db => db.QueryAsync<Currency>(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            var service = CreateService();

            // Action
            var result = await service.GetCurrencyListAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            objectDb.Verify(obj => obj.QueryAsync<Currency>(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCurrencyListAsync_DataInCache_DoNotCallDataAccess()
        {
            // Arrange
            var expectedResult = fixture.CreateMany<Currency>();
            var objectDb = CreateObjectDb();
            var service = CreateService(expectedResult);

            // Action
            var result = await service.GetCurrencyListAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            objectDb.Verify(obj => obj.QueryAsync<Currency>(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private CurrencyService CreateService(object currencyList = null)
        {
            _cache = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            _cache.Setup(c => c.TryGetValue(It.IsAny<object>(), out currencyList)).Returns(true);

            _cacheEntry = new Mock<ICacheEntry>();
            _cache.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(_cacheEntry.Object);

            return new CurrencyService(_cache.Object);
        }

        private static Mock<IObjectDb> CreateObjectDb()
        {
            var mockObjectDB = new Mock<IObjectDb>();
            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }
    }
}