using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class DailyBetDetailsServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IDailyBetDetailsRepository> _repository;

        public DailyBetDetailsServiceTest()
        {
            _repository = new Mock<IDailyBetDetailsRepository>();
        }

        [Fact]
        public async Task GetDataAsync_WhenCalled_CallsRepositoryGetDataAsync()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();
            var expectedResult = _fixture.Create<ResultModel<DailyBetDetails>>();

            _repository.Setup(x => x.GetDataAsync(metaModel, auditCurrencyList))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel, auditCurrencyList);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataAsync(metaModel, auditCurrencyList), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();

            _repository.Setup(x => x.GetDataAsync(metaModel, auditCurrencyList))
                .ReturnsAsync((ResultModel<DailyBetDetails>)null);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel, auditCurrencyList);

            // Assert
            Assert.Null(result);
            _repository.Verify(x => x.GetDataAsync(metaModel, auditCurrencyList), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WhenRepositoryThrowsException_ExceptionPropagates()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();
            var expectedException = new System.Exception("Repository error");

            _repository.Setup(x => x.GetDataAsync(metaModel, auditCurrencyList))
                .ThrowsAsync(expectedException);

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => service.GetDataAsync(metaModel, auditCurrencyList));
            Assert.Equal("Repository error", exception.Message);
            _repository.Verify(x => x.GetDataAsync(metaModel, auditCurrencyList), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WithNullMetaModel_PassesNullToRepository()
        {
            // Arrange
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();
            var expectedResult = _fixture.Create<ResultModel<DailyBetDetails>>();

            _repository.Setup(x => x.GetDataAsync(null, auditCurrencyList))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(null, auditCurrencyList);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataAsync(null, auditCurrencyList), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WithNullAuditCurrencyList_PassesNullToRepository()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var expectedResult = _fixture.Create<ResultModel<DailyBetDetails>>();

            _repository.Setup(x => x.GetDataAsync(metaModel, null))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel, null);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataAsync(metaModel, null), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WithEmptyAuditCurrencyList_PassesEmptyListToRepository()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var emptyAuditCurrencyList = new List<AuditCurrency>();
            var expectedResult = _fixture.Create<ResultModel<DailyBetDetails>>();

            _repository.Setup(x => x.GetDataAsync(metaModel, emptyAuditCurrencyList))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel, emptyAuditCurrencyList);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataAsync(metaModel, emptyAuditCurrencyList), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_WithValidData_ReturnsExpectedResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();
            var expectedDataList = _fixture.Create<List<DailyBetDetails>>();
            var expectedTotalRows = _fixture.Create<int>();
            var expectedResult = new ResultModel<DailyBetDetails>
            {
                DataList = expectedDataList,
                TotalRows = expectedTotalRows
            };

            _repository.Setup(x => x.GetDataAsync(metaModel, auditCurrencyList))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel, auditCurrencyList);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDataList, result.DataList);
            Assert.Equal(expectedTotalRows, result.TotalRows);
            _repository.Verify(x => x.GetDataAsync(metaModel, auditCurrencyList), Times.Once);
        }

        [Fact]
        public async Task GetDataAsync_CallsRepositoryOnlyOnce()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();
            var expectedResult = _fixture.Create<ResultModel<DailyBetDetails>>();

            _repository.Setup(x => x.GetDataAsync(metaModel, auditCurrencyList))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            await service.GetDataAsync(metaModel, auditCurrencyList);

            // Assert
            _repository.Verify(x => x.GetDataAsync(metaModel, auditCurrencyList), Times.Once);
            _repository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetDataAsync_WithDifferentMetaModels_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var metaModel1 = _fixture.Create<DailyBetDetailsMetaModel>();
            var metaModel2 = _fixture.Create<DailyBetDetailsMetaModel>();
            var auditCurrencyList = _fixture.Create<List<AuditCurrency>>();
            var expectedResult1 = _fixture.Create<ResultModel<DailyBetDetails>>();
            var expectedResult2 = _fixture.Create<ResultModel<DailyBetDetails>>();

            _repository.Setup(x => x.GetDataAsync(metaModel1, auditCurrencyList))
                .ReturnsAsync(expectedResult1);
            _repository.Setup(x => x.GetDataAsync(metaModel2, auditCurrencyList))
                .ReturnsAsync(expectedResult2);

            var service = CreateService();

            // Act
            var result1 = await service.GetDataAsync(metaModel1, auditCurrencyList);
            var result2 = await service.GetDataAsync(metaModel2, auditCurrencyList);

            // Assert
            Assert.Equal(expectedResult1, result1);
            Assert.Equal(expectedResult2, result2);
            _repository.Verify(x => x.GetDataAsync(metaModel1, auditCurrencyList), Times.Once);
            _repository.Verify(x => x.GetDataAsync(metaModel2, auditCurrencyList), Times.Once);
        }

        private DailyBetDetailsService CreateService()
        {
            return new DailyBetDetailsService(_repository.Object);
        }
    }
}
