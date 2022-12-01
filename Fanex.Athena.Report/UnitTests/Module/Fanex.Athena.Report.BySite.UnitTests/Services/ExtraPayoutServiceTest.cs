using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class ExtraPayoutServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>().With(_ => _.DrillDownLevel, 0).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayout>>>();

            var repository = new Mock<IExtraPayoutRepository>();
            repository.Setup(_ => _.GetExtraPayoutMainData(metaModel)).Returns(expectedData);

            var service = new ExtraPayoutService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMemberData()
        {
            // Arrange
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>().With(_ => _.DrillDownLevel, 1).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayout>>>();

            var repository = new Mock<IExtraPayoutRepository>();
            repository.Setup(_ => _.GetExtraPayoutMemberData(metaModel)).Returns(expectedData);

            var service = new ExtraPayoutService(repository.Object);

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnDetailData()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayoutDetail>>>();

            var repository = new Mock<IExtraPayoutRepository>();
            repository.Setup(_ => _.GetExtraPayoutDetailData(metaModel)).Returns(expectedData);

            var service = new ExtraPayoutService(repository.Object);

            // Action
            var result = service.GetDetailDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }
    }
}