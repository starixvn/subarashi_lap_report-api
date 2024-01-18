using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class ExtraPayoutServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IExtraPayoutRepository> _repository;

        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        public ExtraPayoutServiceTest()
        {
            _repository = new Mock<IExtraPayoutRepository>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>().With(_ => _.DrillDownLevel, 0).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayout>>>();

            _repository.Setup(_ => _.GetExtraPayoutMainData(metaModel)).Returns(expectedData);

            var service = BuildService();

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

            _repository.Setup(_ => _.GetExtraPayoutMemberData(metaModel)).Returns(expectedData);

            var service = BuildService();

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

            _repository.Setup(_ => _.GetExtraPayoutDetailData(metaModel)).Returns(expectedData);

            var service = BuildService();

            // Action
            var result = service.GetDetailDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        private ExtraPayoutService BuildService()
        {
            return new ExtraPayoutService(_repository.Object, _cacheRefAdapter.Object);
        }
    }
}