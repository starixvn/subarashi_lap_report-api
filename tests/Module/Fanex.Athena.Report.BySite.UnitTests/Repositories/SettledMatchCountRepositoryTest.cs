using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Data;
using Moq;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class SettledMatchCountRepositoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        public SettledMatchCountRepositoryTest()
        {
            DbSettingProviderManager
               .StartTestingSession()
               .Run();

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var expectedReturnedData = _fixture.CreateMany<SettledMatchCount>().ToList();
            _objectDb.Setup(x => x.QueryAsync<SettledMatchCount>(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedReturnedData);
            SetupObjectDb();

            var repository = new SettledMatchCountRepository();

            // Action
            var result = await repository.GetDataList(It.IsAny<bool>(), It.IsAny<int>());

            // Assert
            Assert.Equal(expectedReturnedData, result);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);
        }
    }
}
