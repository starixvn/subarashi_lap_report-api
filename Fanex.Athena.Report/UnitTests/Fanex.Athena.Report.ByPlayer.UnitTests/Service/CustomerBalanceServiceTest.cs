using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Service
{
    public class CustomerBalanceServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public async Task GetDataList_WhenCall_PassesCorrectParameter()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            var mockObjectDB = CustomerBalanceHelperTest.SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                ProductOptions = CustomerBalanceHelperTest.GetProductOptions()
            };
            var service = new CustomerBalanceService();

            // Action
            await service.GetDataList(metaModel);

            // Assert
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("fromdate", metaModel.FromDate), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("todate", metaModel.ToDate), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("topcount", metaModel.TopCount), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("orderitem", metaModel.OrderItem), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("pagesize", metaModel.PageSize), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("pageindex", metaModel.PageIndex), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("showSB", true), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("showNG", false), Times.Once);
        }

        [Fact]
        public async Task GetDataList_NoData_ReturnsEmptyResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            CustomerBalanceHelperTest.SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            var service = new CustomerBalanceService();

            // Action
            var result = await service.GetDataList(metaModel);

            // Assert
            Assert.Empty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_NoTotalData_ReturnsCorrectResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            var expected = _fixture.Create<CustomerBalance>();
            CustomerBalanceHelperTest.ConfigureReaderForResult(expected, mockIDataReader);
            CustomerBalanceHelperTest.SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            var service = new CustomerBalanceService();

            // Action
            var result = await service.GetDataList(metaModel);

            // Assert
            Assert.NotEmpty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnsCorrectResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            var expectedTotalRow = _fixture.Create<int>();
            CustomerBalanceHelperTest.ConfigureReaderForResult(expected, mockIDataReader);
            CustomerBalanceHelperTest.ConfigureReaderForTotalData(expected, expectedTotalRow, mockIDataReader);
            CustomerBalanceHelperTest.SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            var service = new CustomerBalanceService();

            // Action
            var result = await service.GetDataList(metaModel);
            var firstItem = result.DataList.First();

            // Assert
            Assert.Equal(expected.ExternalId, firstItem.ExternalId);
            Assert.Equal(expected.SystemId, firstItem.SystemId);
            Assert.True(CustomerBalanceHelperTest.IsEqualCommonField(expected, firstItem));
            Assert.True(CustomerBalanceHelperTest.IsEqualCommonField(expected, result.TotalData));
            Assert.Equal(expectedTotalRow, result.TotalRows);
        }
    }
}