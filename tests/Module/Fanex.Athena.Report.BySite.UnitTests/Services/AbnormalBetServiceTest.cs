using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class AbnormalBetServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IAbnormalBetRepository> _repository;

        public AbnormalBetServiceTest()
        {
            _repository = new Mock<IAbnormalBetRepository>();
        }

        [Fact]
        public async Task GetDataAsync_WhenViewInIsSummary_CallsGetSummaryData()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.Summary;
            
            var expectedResult = _fixture.Create<SortableReportResultModel<AbnormalBetSummary>>();
            _repository.Setup(x => x.GetSummaryData(metaModel))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetSummaryData(metaModel), Times.Once);
            _repository.Verify(x => x.GetIpSummaryData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
            _repository.Verify(x => x.GetDetailData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
        }

        [Fact]
        public async Task GetDataAsync_WhenViewInIsIpSummary_CallsGetIpSummaryData()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.IpSummary;
            
            var expectedResult = _fixture.Create<SortableReportResultModel<AbnormalBetIpSummary>>();
            _repository.Setup(x => x.GetIpSummaryData(metaModel))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetIpSummaryData(metaModel), Times.Once);
            _repository.Verify(x => x.GetSummaryData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
            _repository.Verify(x => x.GetDetailData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
        }

        [Fact]
        public async Task GetDataAsync_WhenViewInIsDetail_CallsGetDetailData()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.Detail;
            
            var expectedResult = _fixture.Create<SortableReportResultModel<AbnormalBetDetail>>();
            _repository.Setup(x => x.GetDetailData(metaModel))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDetailData(metaModel), Times.Once);
            _repository.Verify(x => x.GetSummaryData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
            _repository.Verify(x => x.GetIpSummaryData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
        }

        [Theory]
        [InlineData(999)] // Unknown value
        [InlineData(-1)]  // Negative value
        [InlineData(100)] // Large value
        public async Task GetDataAsync_WhenViewInIsUnknown_CallsGetDetailDataAsDefault(int unknownViewIn)
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = unknownViewIn;
            
            var expectedResult = _fixture.Create<SortableReportResultModel<AbnormalBetDetail>>();
            _repository.Setup(x => x.GetDetailData(metaModel))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDetailData(metaModel), Times.Once);
            _repository.Verify(x => x.GetSummaryData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
            _repository.Verify(x => x.GetIpSummaryData(It.IsAny<AbnormalBetMetaModel>()), Times.Never);
        }

        [Fact]
        public async Task GetDataAsync_WhenRepositoryThrowsException_ExceptionPropagates()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.Summary;
            
            var expectedException = new System.Exception("Repository error");
            _repository.Setup(x => x.GetSummaryData(metaModel))
                .ThrowsAsync(expectedException);

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => service.GetDataAsync(metaModel));
            Assert.Equal("Repository error", exception.Message);
        }

        [Fact]
        public async Task GetDataAsync_WithNullMetaModel_ThrowsArgumentNullException()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(() => service.GetDataAsync(null));
        }

        [Fact]
        public async Task GetDataAsync_WhenSummaryDataReturnsNull_ReturnsNull()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.Summary;
            
            _repository.Setup(x => x.GetSummaryData(metaModel))
                .ReturnsAsync((SortableReportResultModel<AbnormalBetSummary>)null);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Null(result);
            _repository.Verify(x => x.GetSummaryData(metaModel), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WhenIpSummaryDataReturnsNull_ReturnsNull()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.IpSummary;
            
            _repository.Setup(x => x.GetIpSummaryData(metaModel))
                .ReturnsAsync((SortableReportResultModel<AbnormalBetIpSummary>)null);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Null(result);
            _repository.Verify(x => x.GetIpSummaryData(metaModel), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WhenDetailDataReturnsNull_ReturnsNull()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = (int)AbnormalBetViewInLevel.Detail;
            
            _repository.Setup(x => x.GetDetailData(metaModel))
                .ReturnsAsync((SortableReportResultModel<AbnormalBetDetail>)null);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.Null(result);
            _repository.Verify(x => x.GetDetailData(metaModel), Times.Once);
        }

        [Theory]
        [InlineData(0)] // Summary
        [InlineData(1)] // IpSummary  
        [InlineData(2)] // Detail
        public async Task GetDataAsync_WithValidViewInLevels_CallsCorrectRepositoryMethod(int viewInLevel)
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            metaModel.ViewIn = viewInLevel;
            
            var summaryResult = _fixture.Create<SortableReportResultModel<AbnormalBetSummary>>();
            var ipSummaryResult = _fixture.Create<SortableReportResultModel<AbnormalBetIpSummary>>();
            var detailResult = _fixture.Create<SortableReportResultModel<AbnormalBetDetail>>();
            
            _repository.Setup(x => x.GetSummaryData(metaModel)).ReturnsAsync(summaryResult);
            _repository.Setup(x => x.GetIpSummaryData(metaModel)).ReturnsAsync(ipSummaryResult);
            _repository.Setup(x => x.GetDetailData(metaModel)).ReturnsAsync(detailResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            switch (viewInLevel)
            {
                case (int)AbnormalBetViewInLevel.Summary:
                    Assert.Equal(summaryResult, result);
                    _repository.Verify(x => x.GetSummaryData(metaModel), Times.Once);
                    break;
                case (int)AbnormalBetViewInLevel.IpSummary:
                    Assert.Equal(ipSummaryResult, result);
                    _repository.Verify(x => x.GetIpSummaryData(metaModel), Times.Once);
                    break;
                case (int)AbnormalBetViewInLevel.Detail:
                    Assert.Equal(detailResult, result);
                    _repository.Verify(x => x.GetDetailData(metaModel), Times.Once);
                    break;
            }
        }

        private AbnormalBetService CreateService()
        {
            return new AbnormalBetService(_repository.Object);
        }
    }
}
