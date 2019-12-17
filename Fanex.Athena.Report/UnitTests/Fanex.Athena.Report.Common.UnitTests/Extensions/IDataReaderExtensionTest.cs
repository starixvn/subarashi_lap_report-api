using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Extensions
{
    public class IDataReaderExtensionTest
    {
        private const string ColumnName = "ColName";

        public IDataReaderExtensionTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();
        }

        [Theory]
        [InlineData(1)]
        [InlineData("string value")]
        [InlineData(double.Epsilon)]
        public async Task GetVal_WhenCall_ReturnCorrectValue<T>(T value)
        {
            // Action
            var result = await GetValue<T>(value);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task GetVal_ValueIsNull_ReturnDefaultValue()
        {
            // Arrange
            decimal expected = 0;

            using (var reader = await GetDataReader())
            {
                var mockReader = Mock.Get(reader);
                mockReader.Setup(r => r[ColumnName]).Returns(DBNull.Value);

                // Action
                var result = reader.GetVal<decimal>(ColumnName);

                // Assert
                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public async Task GetVal_ConvertFromDoubleToDecimal_ReturnCorrectValue()
        {
            // Arrange
            const double inputValue = 0.68;
            const decimal expected = 0.68m;

            using (var reader = await GetDataReader())
            {
                var mockReader = Mock.Get(reader);
                mockReader.Setup(r => r[ColumnName]).Returns(inputValue);

                // Action
                var result = reader.GetVal<decimal>(ColumnName);

                // Assert
                Assert.Equal(expected, result);
            }
        }

        private async Task<T> GetValue<T>(T value)
        {
            using (var reader = await GetDataReader())
            {
                var mockIDataReader = Mock.Get(reader);
                mockIDataReader.Setup(r => r[ColumnName]).Returns(value);

                return reader.GetVal<T>(ColumnName);
            }
        }

        private async Task<IDataReader> GetDataReader()
        {
            var mockIDataReader = new Mock<IDataReader>();
            var mockObjectDB = CreateObjectDb(mockIDataReader);

            using (var objectDb = mockObjectDB.Object)
            {
                return await objectDb.ExecuteReaderAsync();
            }
        }

        private static Mock<IObjectDb> CreateObjectDb(Mock<IDataReader> mockIDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockIDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }
    }
}