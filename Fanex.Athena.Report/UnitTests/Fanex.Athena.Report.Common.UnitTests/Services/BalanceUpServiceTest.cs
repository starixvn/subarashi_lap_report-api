using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services;
using Fanex.Data;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class BalanceUpServiceTest : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        private readonly IMemoryCache _memoryCache;
        public BalanceUpServiceTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        public void Dispose()
        {
            _memoryCache?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetBalanceUpAsync_WhenCall_ReturnCorrectResultAsync()
        {
            // Arrange
            var balanceupInfo = new BalanceUp
            {
                BalanceUpDay = DateTime.Now,
                MoveBetsDay = DateTime.Now
            };

            var mockIDataReader = new Mock<IDataReader>();

            _memoryCache.Remove("EXT_LAP_CACHE_BALANCEUP");

            mockIDataReader.Setup(m => m["balanceupday"]).Returns(balanceupInfo.BalanceUpDay);
            mockIDataReader.Setup(m => m["movebetsday"]).Returns(balanceupInfo.MoveBetsDay);
            mockIDataReader.SetupSequence(m => m.Read())
              .Returns(true)
              .Returns(false);

            var mockObjectDB = new Mock<IObjectDb>();

            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockIDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            var service = new BalanceUpService(_memoryCache);

            // Action
            var result = await service.GetBalanceUpAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(result.BalanceUpDay, balanceupInfo.BalanceUpDay);
            Assert.Equal(result.MoveBetsDay, balanceupInfo.MoveBetsDay);
        }

        [Fact]
        public async Task GetBalanceUpAsync_WhenCall_ReturnDataFromCache()
        {
            // Arrange
            var balanceupInfo = new BalanceUp
            {
                BalanceUpDay = DateTime.Now,
                MoveBetsDay = DateTime.Now
            };
            _memoryCache.Set("EXT_LAP_CACHE_BALANCEUP", balanceupInfo);

            var service = new BalanceUpService(_memoryCache);

            // Action
            var result = await service.GetBalanceUpAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(result, balanceupInfo);

        }

    }
}
