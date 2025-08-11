namespace Fanex.Athena.Report.ByPlayer.UnitTests.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ByPlayer.Service;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;
    using Moq;
    using Repository.Interfaces;
    using Xunit;

    public class CustomerTurnoverServiceTest
    {
        private readonly IReadOnlyDictionary<int, string> _currencyList = new Dictionary<int, string> { { 20, "UUS" } };

        private readonly Mock<ICustomerTurnoverRepository> _customerTurnoverRepository;

        public CustomerTurnoverServiceTest()
        {
            _customerTurnoverRepository = new Mock<ICustomerTurnoverRepository>();
        }

        [Fact]
        public async Task GetDataList_ValidModel_NoData_ReturnEmptyData()
        {
            // Arrange
            var metaModel = new CustomerTurnoverMetaModel
            {
                FromDate = DateTimeHelper.CreateDateTime(2017, 1, 1),
                ToDate = DateTimeHelper.CreateDateTime(2017, 2, 2),
                Bettypes = "1"
            };

            _customerTurnoverRepository.Setup(x => x.GetDataList(metaModel, _currencyList, string.Empty)).ReturnsAsync(
                new ReportResultModel<CustTurnover>
                {
                    DataList = new List<CustTurnover>()
                });

            var service = CreateService();

            // Action
            var result = await service.GetDataList(metaModel, _currencyList, string.Empty);

            // Assert
            Assert.Empty(result.DataList);
        }

        [Fact]
        public async Task GetDataList_ValidMetaModel_ReturnDataFromDataAccess()
        {
            // Arrange
            var metaModel = new CustomerTurnoverMetaModel
            {
                ShowDraw = true,
                FromDate = DateTimeHelper.CreateDateTime(2017, 1, 1),
                ToDate = DateTimeHelper.CreateDateTime(2017, 2, 2),
                Bettypes = "1"
            };
            var dataList = new List<CustTurnover>
            {
                new() { ExternalId = "1" },
                new() { ExternalId = "1" },
                new() { ExternalId = "2" },
                new() { ExternalId = "2" },
                new() { ExternalId = "3" }
            };
            var expectedResult = new ReportResultModel<CustTurnover>
            {
                DataList = dataList,
                TotalData = new CustTurnover
                {
                    CustomerComm = 123
                },
                TotalRows = 999
            };
            _customerTurnoverRepository.Setup(x => x.GetDataList(metaModel, _currencyList, string.Empty))
                                       .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Action
            var result = await service.GetDataList(metaModel, _currencyList, string.Empty);

            // Assert
            _customerTurnoverRepository.Verify(x => x.GetDataList(metaModel, _currencyList, string.Empty), Times.Once);
            Assert.Equal(dataList.Count, result.DataList.Count());
            Assert.Equal(expectedResult.TotalData, result.TotalData);
            Assert.Equal(result.TotalRows, expectedResult.TotalRows);
        }

        private CustomerTurnoverService CreateService() => new(_customerTurnoverRepository.Object);
    }
}
