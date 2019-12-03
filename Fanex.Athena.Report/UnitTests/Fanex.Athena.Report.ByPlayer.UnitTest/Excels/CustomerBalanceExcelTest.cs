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
using Fanex.Athena.Report.Resources;
using System.Linq;
using System;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Excels
{
    public class CustomerBalanceExcelTest : CustomerBalanceHelperTest
    {
        private const string CSVSeparateCharacter = ",";

        private const string RootPath = "D:";

        private readonly string _firstFooterString = $"{CSVSeparateCharacter}{CSVSeparateCharacter}Total{CSVSeparateCharacter}";

        private readonly IFixture _fixture;

        private readonly IFileService _fileService;

        private readonly Mock<IOptions<ExcelOptions>> _excelOptionsAccessor;

        private readonly ExcelOptions _excelOptions;

        private readonly Mock<IHostingEnvironment> _enviroment;

        private readonly ICustomerBalanceExcel _excel;

        public CustomerBalanceExcelTest()
        {
            _fixture = new Fixture();
            _enviroment = new Mock<IHostingEnvironment>();
            _enviroment.Setup(e => e.ContentRootPath).Returns(RootPath);

            _fileService = new FileService();

            _excelOptionsAccessor = new Mock<IOptions<ExcelOptions>>();
            _excelOptions = new ExcelOptions()
            {
                Path = "LapReportTempExcel",
                CSVSeparateCharacter = CSVSeparateCharacter
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
        public async Task Export_WhenCall_ReturnsCorrectFileName()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            var root = _fileService.GetParentDirectory(RootPath);
            var path = Path.Combine(root.FullName, _excelOptions.Path);

            // Action
            var fileName = await _excel.Export(metaModel);
            File.Delete(fileName);

            // Assert
            Assert.Contains(path, fileName);
            Assert.Contains(".dat", fileName);
        }

        [Fact]
        public async Task Export_NoData_FileHavesHeaderAndNoInfoLine()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            // Action
            var fileName = await _excel.Export(metaModel);
            var count = File.ReadAllLines(fileName).Length;
            var header = File.ReadLines(fileName).First();
            var noInfoLine = GetLine(fileName, 2);
            File.Delete(fileName);

            // Assert
            Assert.True(IsCommonHeader(header) && count == 2 && noInfoLine.Contains(ViewResources.NoInfo));
        }

        [Fact]
        public async Task Export_HaveData_FileHavesData()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, 1, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            // Action
            var fileName = await _excel.Export(metaModel);
            var count = File.ReadAllLines(fileName).Length;
            var header = File.ReadLines(fileName).First();
            File.Delete(fileName);

            // Assert
            Assert.True(count > 2 && IsCommonHeader(header));
        }

        [Fact]
        public async Task Export_HaveDataNotHasCommAndNotIsAllowedBA_FileHavesCorrectData()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            expected.NetTurnover = expected.Turnover - expected.DrawGame;
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, 1, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                HasComm = false,
                IsAllowedBA = false,
                ProductOptions = GetProductOptions()
            };

            // Action
            var fileName = await _excel.Export(metaModel);
            var header = File.ReadLines(fileName).First();
            var footer = GetLine(fileName, 3);
            var totals = GetTotals(footer);
            File.Delete(fileName);

            // Assert
            Assert.True(
                IsCommonHeader(header)
                && !IsHasCommHeader(header)
                && !IsHasBAHeader(header)
                && IsValidFooter(expected, footer, totals, metaModel)
            );
        }

        [Fact]
        public async Task Export_HasCommAndNotIsAllowedBA_FileHavesCorrectData()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            expected.NetTurnover = expected.Turnover - expected.DrawGame;
            expected.TotalWL = expected.WinLoss + expected.PlayerComm;
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, 1, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                HasComm = true,
                IsAllowedBA = false,
                ProductOptions = GetProductOptions()
            };

            // Action
            var fileName = await _excel.Export(metaModel);
            var header = File.ReadLines(fileName).First();
            var footer = GetLine(fileName, 3);
            var totals = GetTotals(footer);
            File.Delete(fileName);

            // Assert
            Assert.True(
                IsCommonHeader(header)
                && IsHasCommHeader(header)
                && !IsHasBAHeader(header)
                && IsValidFooter(expected, footer, totals, metaModel)
            );
        }

        [Fact]
        public async Task Export_NotHasCommAndIsAllowedBA_FileHavesCorrectData()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            expected.NetTurnover = expected.Turnover - expected.DrawGame;
            expected.TotalWL = expected.WinLoss + expected.PlayerComm;
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, 1, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                HasComm = false,
                IsAllowedBA = true,
                ProductOptions = GetProductOptions()
            };

            // Action
            var fileName = await _excel.Export(metaModel);
            var header = File.ReadLines(fileName).First();
            var footer = GetLine(fileName, 3);
            var totals = GetTotals(footer);
            File.Delete(fileName);

            // Assert
            Assert.True(
                IsCommonHeader(header)
                && !IsHasCommHeader(header)
                && IsHasBAHeader(header)
                && IsValidFooter(expected, footer, totals, metaModel)
            );
        }

        [Fact]
        public async Task Export_HasCommAndIsAllowedBA_FileHavesCorrectData()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            expected.NetTurnover = expected.Turnover - expected.DrawGame;
            expected.TotalWL = expected.WinLoss + expected.PlayerComm;
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, 1, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                HasComm = true,
                IsAllowedBA = true,
                ProductOptions = GetProductOptions()
            };

            // Action
            var fileName = await _excel.Export(metaModel);
            var header = File.ReadLines(fileName).First();
            var footer = GetLine(fileName, 3);
            var totals = GetTotals(footer);
            File.Delete(fileName);

            // Assert
            Assert.True(
                IsCommonHeader(header)
                && IsHasCommHeader(header)
                && IsHasBAHeader(header)
                && IsValidFooter(expected, footer, totals, metaModel)
            );
        }

        private string GetLine(string fileName, int line)
        {
            using (var stream = new StreamReader(fileName))
            {
                for (int index = 1; index < line; index++)
                {
                    stream.ReadLine();
                }

                return stream.ReadLine();
            }
        }

        private bool IsCommonHeader(string header)
        {
            return header.Contains("#")
                && header.Contains(ViewResources.ExternalId)
                && header.Contains(ViewResources.SystemId)
                && header.Contains(ViewResources.OpeningBalance)
                && header.Contains(ViewResources.Deposit)
                && header.Contains(ViewResources.Withdrawal)
                && header.Contains(ViewResources.Bonus)
                && header.Contains(ViewResources.Adjustment)
                && header.Contains(ViewResources.Promotion)
                && header.Contains(ViewResources.Turnover)
                && header.Contains(ViewResources.DrawGame)
                && header.Contains(ViewResources.NetTurnover)
                && header.Contains(ViewResources.PendingFunds)
                && header.Contains(ViewResources.CustomerW_L)
                && header.Contains(ViewResources.ClosingBalance)
                && header.Contains(ViewResources.AffiliateTransfer);
        }

        private bool IsHasCommHeader(string header)
        {
            return header.Contains($"{ViewResources.CustomerW_L} {ViewResources.Comm}")
                && header.Contains($"{ViewResources.CustomerW_L} {ViewResources.Total}");
        }

        private bool IsHasBAHeader(string header)
        {
            return header.Contains($"{ViewResources.BA} {ViewResources.Turnover}")
                && header.Contains(ViewResources.BACommission)
                && header.Contains(ViewResources.AmtDueToBA);
        }

        private bool IsValidFooter(CustomerBalance expected, string footer, IList<decimal> totals, CustomerBalanceMetaModel metaModel)
        {
            var isCommon = footer.Contains(_firstFooterString)
                && expected.OpeningBalance == totals[0]
                && expected.Deposit == totals[1]
                && expected.Withdrawal == totals[2]
                && expected.Bonus == totals[3]
                && expected.Adjustment == totals[4]
                && expected.Promotion == totals[5]
                && expected.Turnover == totals[6]
                && expected.DrawGame == totals[7]
                && expected.NetTurnover == totals[8]
                && expected.PendingFunds == totals[9]
                && expected.WinLoss == totals[10];

            var count = 11;
            if (metaModel.HasComm)
            {
                isCommon = isCommon
                    && expected.PlayerComm == totals[count]
                    && expected.TotalWL == totals[count + 1];
                count = 13;
            }

            isCommon = isCommon
                && expected.ClosingBalance == totals[count]
                && expected.AffiliateTransfer == totals[count + 1];

            if (metaModel.IsAllowedBA)
            {
                var totalCount = totals.Count();
                isCommon = isCommon
                    && expected.TurnoverBA == totals[totalCount - 3]
                    && expected.CommBA == totals[totalCount - 2]
                    && expected.AmountDueToBA == totals[totalCount - 1];
            }

            return isCommon;
        }

        private IList<decimal> GetTotals(string footer)
        {
            var onlyNumber = footer.Remove(0, _firstFooterString.Length);
            return onlyNumber.Split(CSVSeparateCharacter).Select(number => Convert.ToDecimal(number)).ToList();
        }
    }
}