namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.MetaModels;
    using BySite.Repositories;
    using Common.Model.DTO;
    using Common.Services.Interfaces;
    using Data;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class SubsidiaryPerformanceRepositoryTest
    {
        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Fixture _fixture;
        private readonly Mock<IObjectDb> _objectDb;
        private readonly Mock<IDataReader> _reader;

        public SubsidiaryPerformanceRepositoryTest()
        {
            _fixture = new Fixture();
            _balanceUpService = new Mock<IBalanceUpService>();
            _objectDb = new Mock<IObjectDb>();
            _reader = new Mock<IDataReader>();
            DbSettingProviderManager
                .StartTestingSession()
                .Run();
        }

        [Fact]
        public async Task GetData_WithValidParameters_CallsBalanceUpService()
        {
            // Arrange
            var metaModel = FakeHelper.Create<SubsidiaryPerformanceMetaModel>();
            var currencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDb();

            await repository.GetData(metaModel, currencyList);

            _balanceUpService.Verify(x => x.GetBalanceUpAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDetailData_WithValidParameters_CallsBalanceUpService()
        {
            // Arrange
            var metaModel = FakeHelper.Create<SubsidiaryPerformanceMetaModel>();
            var currencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var balanceUp = new BalanceUp { MoveBetsDay = DateTime.Now };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var repository = CreateRepository();
            SetupObjectDb();

            await repository.GetDetailData(metaModel, currencyList);

            _balanceUpService.Verify(x => x.GetBalanceUpAsync(), Times.Once);
        }


        [Fact]
        public void Constructor_WithBalanceUpService_SetsService()
        {
            // Arrange & Act
            var repository = CreateRepository();

            // Assert
            Assert.NotNull(repository);
        }

        private SubsidiaryPerformanceRepository CreateRepository()
        {
            return new SubsidiaryPerformanceRepository(_balanceUpService.Object);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
        }
    }
}