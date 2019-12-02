using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Core.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Excels
{
    public class CustomerBalanceExcelTest : CustomerBalanceHelperTest
    {
        private const string rootPath = "D:";

        private const string LapReportTempExcel = "LapReportTempExcel";

        private readonly IFixture _fixture;

        private readonly IFileService _fileService;

        private readonly DirectoryInfo _root;

        private readonly Mock<IOptions<ExcelOptions>> _excelOptionsAccessor;

        private readonly ExcelOptions _excelOptions;

        private readonly Mock<IHostingEnvironment> _enviroment;

        private readonly ICustomerBalanceExcel _excel;

        public CustomerBalanceExcelTest()
        {
            _fixture = new Fixture();
            _enviroment = new Mock<IHostingEnvironment>();
            _enviroment.Setup(e => e.ContentRootPath).Returns(rootPath);

            _fileService = new FileService();
            _root = new DirectoryInfo(rootPath);

            _excelOptionsAccessor = new Mock<IOptions<ExcelOptions>>();
            _excelOptions = new ExcelOptions()
            {
                Path = LapReportTempExcel,
                CSVSeparateCharacter = ","
            };
            _excelOptionsAccessor.Setup(eoa => eoa.Value).Returns(_excelOptions);

            _excel = new CustomerBalanceExcel(_fileService, _excelOptionsAccessor.Object, _enviroment.Object);
        }

        [Fact]
        public async Task Export_WhenCall_PassesCorrectParameter()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            var mockObjectDB = SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                ProductOptions = GetProductOptions()
            };

            // Action
            var fileName = await _excel.Export(metaModel);
            File.Delete(fileName);

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
        public async Task Export_NoData_ReturnsNoInfoFileName()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            var root = _fileService.GetParentDirectory(rootPath);
            var path = Path.Combine(root.FullName, _excelOptions.Path);

            // Action
            var fileName = await _excel.Export(metaModel);
            File.Delete(fileName);

            // Assert
            Assert.Contains(path, fileName);
            Assert.Contains(".dat", fileName);
        }

        [Fact]
        public async Task Export_NoTotalData_ReturnsCorrectFileName()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            var expected = _fixture.Create<CustomerBalance>();
            ConfigureReaderForResult(expected, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            var root = _fileService.GetParentDirectory(rootPath);
            var path = Path.Combine(root.FullName, _excelOptions.Path);

            // Action
            var fileName = await _excel.Export(metaModel);
            File.Delete(fileName);

            // Assert
            Assert.Contains(path, fileName);
            Assert.Contains(".dat", fileName);
        }

        [Fact]
        public async Task Export_TotalRowsIsZero_ReturnsNoInfoFileName()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, 0, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            var root = _fileService.GetParentDirectory(rootPath);
            var path = Path.Combine(root.FullName, _excelOptions.Path);

            // Action
            var fileName = await _excel.Export(metaModel);
            File.Delete(fileName);

            // Assert
            Assert.Contains(path, fileName);
            Assert.Contains(".dat", fileName);
        }

        [Fact]
        public async Task Export_WhenCall_ReturnsCorrectFileName()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            var expectedTotalRow = _fixture.Create<int>();
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, expectedTotalRow, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            metaModel.HasComm = true;
            metaModel.IsAllowedBA = true;

            var root = _fileService.GetParentDirectory(rootPath);
            var path = Path.Combine(root.FullName, _excelOptions.Path);

            // Action
            var fileName = await _excel.Export(metaModel);
            File.Delete(fileName);

            // Assert
            Assert.Contains(path, fileName);
            Assert.Contains(".dat", fileName);
        }
    }
}