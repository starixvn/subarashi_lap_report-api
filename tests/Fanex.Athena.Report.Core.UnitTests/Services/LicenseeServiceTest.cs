using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Services;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class LicenseeServiceTest
    {
        private readonly Mock<IObjectDb> _objectDb;
        private readonly Mock<IDataReader> _reader;

        public LicenseeServiceTest()
        {
            DbSettingProviderManager
               .StartTestingSession()
               .Run();

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public async Task CheckProductPermission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            const int productId = 3;
            const string licenseeName = "Name";
            _objectDb.Setup(_ => _.ExecuteScalarAsync(null, null, default)).ReturnsAsync(true);

            SetupObjectDb();
            var service = new LicenseeService();

            // Action
            var result = await service.CheckProductPermission(licenseeName, productId);

            // Assert

            _objectDb.Verify(x => x.SetParameterValue(It.Is<string>(_ => _.Equals("productId")), productId));
            _objectDb.Verify(x => x.SetParameterValue(It.Is<string>(_ => _.Equals("licenseeName")), licenseeName));

            Assert.True(result);
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
