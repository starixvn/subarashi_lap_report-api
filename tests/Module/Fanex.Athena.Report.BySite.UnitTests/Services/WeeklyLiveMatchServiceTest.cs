using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class WeeklyLiveMatchServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IWeeklyLiveMatchRepository> _mockWeeklyLiveMatchRepository;

        public WeeklyLiveMatchServiceTest()
        {
            _mockWeeklyLiveMatchRepository = new Mock<IWeeklyLiveMatchRepository>();
        }

        [Fact]
        public async Task GetData_InValidMetaModel_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = new WeeklyLiveMatchMetaModel
            {
                FromDate = new DateTime(2017, 2, 1),
                ToDate = new DateTime(2017, 1, 1)
            };

            var service = new WeeklyLiveMatchService(_mockWeeklyLiveMatchRepository.Object);

            // Action
            var result = await service.GetData(metaModel);

            // Assert
            Assert.Empty(result.DataList);
            Assert.Empty(result.WeeksTotalData);
            _mockWeeklyLiveMatchRepository.Verify(x => x.GetDataList(It.IsAny<WeeklyLiveMatchMetaModel>()),
                Times.Never);
        }

        [Fact]
        public async Task GetData_WhenCall_ReturnData()
        {
            // Arrange
            var metaModel = new WeeklyLiveMatchMetaModel
            {
                FromDate = new DateTime(2017, 1, 1),
                ToDate = new DateTime(2017, 2, 1)
            };
            var expected = _fixture.Create<WeeklyLiveMatchResultModel>();
            _mockWeeklyLiveMatchRepository.Setup(x => x.GetDataList(It.IsAny<WeeklyLiveMatchMetaModel>()))
                                          .Returns(Task.FromResult(expected));

            var service = new WeeklyLiveMatchService(_mockWeeklyLiveMatchRepository.Object);

            // Action
            var result = await service.GetData(metaModel);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetWeekHeaders_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var fromDate = new DateTime(2018, 1, 9);
            var toDate = new DateTime(2018, 1, 26);
            const int numberOfWeeks = 3;

            var expected = new[]
            {
                "09 Jan - 14 Jan 2018",
                "15 Jan - 21 Jan 2018",
                "22 Jan - 26 Jan 2018"
            };

            var service = new WeeklyLiveMatchService(_mockWeeklyLiveMatchRepository.Object);

            // Action
            var result = service.GetWeekHeaders(fromDate, toDate, numberOfWeeks).ToArray();

            // Assert
            Assert.Equal(expected[0], result[0]);
            Assert.Equal(expected[1], result[1]);
            Assert.Equal(expected[2], result[2]);
        }
    }
}
