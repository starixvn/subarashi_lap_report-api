using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class SettledMatchCountServiceTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Fixture _fixture = new();

        private readonly Mock<ISettledMatchCountRepository> _settledMatchCountRepository;

        public SettledMatchCountServiceTest()
        {
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _settledMatchCountRepository = new Mock<ISettledMatchCountRepository>();
        }

        [Fact]
        public async Task GetDataList_SportIsNotAllowed_ReturnEmptyData()
        {
            // Arrange
            var metadata = _fixture.Create<SettledMatchCountMetaModel>();
            IList<SettledMatchCount> dataList = new List<SettledMatchCount>
            {
                new()
                {
                    SportType = SportType.NumberGame
                }
            };
            _settledMatchCountRepository.Setup(x => x.GetDataList(It.IsAny<bool>(), It.IsAny<int>()))
                                        .Returns(Task.FromResult(dataList));

            var service = new SettledMatchCountService(_settledMatchCountRepository.Object, _cacheRefAdapter.Object);

            // Action
            var result = await service.GetDataList(metadata);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            products[0].ProductCode = Product.NG;
            products[1].ProductCode = Product.VS;
            var metaModel = _fixture.Build<SettledMatchCountMetaModel>()
                                    .With(x => x.ProductAccess, new ProductAccess(products)).Create();

            IList<SettledMatchCount> dataList = new List<SettledMatchCount>
            {
                new() { SportType = SportType.NumberGame, IsToday = 0, Entered = 1, },
                new() { SportType = SportType.NumberGame, IsToday = 0, NonEntered = 1, },
                new() { SportType = SportType.VirtualSport, IsToday = 0, NonEntered = 1, },
                new() { SportType = SportType.VirtualSport, IsToday = 1, NonEntered = 1, }
            };

            _settledMatchCountRepository.Setup(x => x.GetDataList(It.IsAny<bool>(), It.IsAny<int>()))
                                        .Returns(Task.FromResult(dataList));

            const int expectedToDayRecords = 3;
            var service = new SettledMatchCountService(_settledMatchCountRepository.Object, _cacheRefAdapter.Object);

            // Action
            var result = await service.GetDataList(metaModel);

            // Assert
            Assert.True(result.Count >= expectedToDayRecords);
        }
    }
}
