using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class SportServiceTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IMemoryCache> _memoryCache;

        private readonly Mock<ISportRepository> _sportRepository;

        public SportServiceTest()
        {
            _sportRepository = new Mock<ISportRepository>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _memoryCache = new Mock<IMemoryCache>();
        }

        [Fact]
        public async Task GetSportsAndProducts_WhenCall_ReturnCorrectly()
        {
            //Arrange
            const int LicenseeId = 1;
            const string Lang = "VN";
            const string ProductCode = "Sport";
            var service = CreateService();
            var expectedResult = FakeHelper.CreateMany<SportProductItem>().ToList();
            _sportRepository.Setup(x => x.GetSportsAndProducts(LicenseeId, Lang, ProductCode)).ReturnsAsync(expectedResult);

            //Action
            var result = await service.GetSportsAndProducts(LicenseeId, Lang, ProductCode);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetSportAndProductAndSabaSport_WhenCall_ReturnCorrectly()
        {
            //Arrange
            const int LicenseeId = 1;
            const string Lang = "VN";
            const string ProductCode = "Sport";
            const bool IsHideSaba = false;

            var service = CreateService();

            var sabaSports = service.GetSabaSports().ToList();
            var stringSabaSports = string.Join(",", sabaSports.Select(s => s.Id));

            var sportsAndProducts = FakeHelper.CreateMany<SportProductItem>().ToList();
            var stringSportsAndProducts = string.Join(",", sportsAndProducts.Select(s => s.Id));
            _sportRepository.Setup(x => x.GetSportsAndProducts(LicenseeId, Lang, ProductCode)).ReturnsAsync(sportsAndProducts);

            //Action
            var result = await service.GetSportAndProductAndSabaSport(LicenseeId, Lang, IsHideSaba, ProductCode);
            var stringResult = string.Join(",", result.Select(r => r.Id));

            //Assert
            Assert.Equal(sportsAndProducts.Count() + sabaSports.Count(), result.Count());
            Assert.Contains(stringSabaSports, stringResult);
            Assert.Contains(stringSportsAndProducts, stringResult);
        }

        private SportService CreateService() => new(_sportRepository.Object, _cacheRefAdapter.Object, _memoryCache.Object);
    }
}
