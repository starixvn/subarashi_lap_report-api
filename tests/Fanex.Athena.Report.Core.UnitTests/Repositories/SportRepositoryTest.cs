using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Repositories;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Data;
using Moq;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Repositories
{
    public class SportRepositoryTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        private readonly Mock<IProductService> _productService;

        public SportRepositoryTest()
        {
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _objectDb = new Mock<IObjectDb>();
            _reader = new Mock<IDataReader>();
            _productService = new Mock<IProductService>();

            DbSettingProviderManager
               .StartTestingSession()
               .Run();
        }

        [Fact]
        public async Task GetSportsAndProducts_WhenCall_ParametersPassedCorrectly()
        {
            // Arrange
            const int LicenseeId = 1;
            const string LanguageCode = "en-US";

            var repository = GetConfiguredRepository();

            // Action
            await repository.GetSportsAndProducts(LicenseeId, LanguageCode);

            // Assert
            _objectDb.Verify(x => x.SetParameterValue("licenseeId", LicenseeId));
        }

        [Theory, InlineData(SportType.UnknowThaiSport), InlineData(SportType.DuplicatedCricket),]
        public async Task GetSportsAndProducts_HiddenSport_NotAddToResult(int sportType)
        {
            // Arrange
            _reader.Setup(r => r["ID"])
                   .Returns(sportType);

            _reader.Setup(r => r["ParentId"]).Returns(-1);
            _reader.Setup(r => r["Name"]).Returns("Any");
            _reader.SetupSequence(r => r.Read()).Returns(true).Returns(false);

            var repository = GetConfiguredRepository();

            // Action
            var result = await repository.GetSportsAndProducts(0, string.Empty);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSportsAndProducts_NotHiddenSport_AddToResult()
        {
            // Arrange
            _reader.Setup(r => r["ID"])
                   .Returns(SportType.BitCoin);

            _reader.Setup(r => r["ParentId"]).Returns(-1);
            _reader.Setup(r => r["Name"]).Returns("Any");
            _reader.SetupSequence(r => r.Read()).Returns(true).Returns(false);

            var repository = GetConfiguredRepository();

            // Action
            var result = await repository.GetSportsAndProducts(0, string.Empty);

            // Assert
            Assert.NotEmpty(result);
        }

        private SportRepository GetConfiguredRepository()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);

            return new SportRepository(_cacheRefAdapter.Object, _productService.Object);
        }
    }
}