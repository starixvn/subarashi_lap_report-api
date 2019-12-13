using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excels;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Excels
{
    public class CustomerBalanceExcelTest : CustomerBalanceHelperTest
    {
        private const string CSVSeparateCharacter = ",";

        private readonly string RootPath = Directory.GetCurrentDirectory();

        private readonly IFixture _fixture;

        private readonly ExcelOptions _excelOptions = new ExcelOptions()
        {
            Path = "LapReportTempExcel",
            CSVSeparateCharacter = CSVSeparateCharacter
        };

        private readonly Mock<IHostingEnvironment> _enviroment;

        private static readonly string[] _commonHeaderText = {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.OpeningBalance,
                ViewResources.Deposit,
                ViewResources.Withdrawal,
                ViewResources.Bonus,
                ViewResources.Adjustment,
                ViewResources.Promotion,
                ViewResources.Turnover,
                ViewResources.DrawGame,
                ViewResources.NetTurnover,
                ViewResources.PendingFunds,
                ViewResources.CustomerW_L
            };

        public CustomerBalanceExcelTest()
        {
            _fixture = new Fixture();
            _enviroment = new Mock<IHostingEnvironment>();
            _enviroment.Setup(e => e.ContentRootPath).Returns(RootPath);
        }

        private Mock<IFileService> CreateMockFileService(StreamWriter streamWriter)
        {
            var fileService = new Mock<IFileService>();
            var directoryInfo = new DirectoryInfo(RootPath);
            fileService.Setup(s => s.GetParentDirectory(RootPath)).Returns(directoryInfo);
            fileService.Setup(f => f.CreateStreamWriter(It.Is<string>(x => x.Contains(RootPath)), false, Encoding.UTF8)).Returns(streamWriter);

            return fileService;
        }

        private Mock<IOptions<ExcelOptions>> CreateMockExcelOption()
        {
            var excelOptionsAccessor = new Mock<IOptions<ExcelOptions>>();
            excelOptionsAccessor.Setup(eoa => eoa.Value).Returns(_excelOptions);

            return excelOptionsAccessor;
        }

        private CustomerBalanceExcel CreateExcelService(StreamWriter streamWriter)
        {
            var fileService = CreateMockFileService(streamWriter);
            var excelOptionAccessor = CreateMockExcelOption();

            return new CustomerBalanceExcel(fileService.Object, excelOptionAccessor.Object, _enviroment.Object);
        }

        private void TearDown(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
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

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);

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

                streamWriter.Dispose();
                TearDown(fileName);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task Export_WhenCall_GenerateHeaderCorrectly(bool hasComm, bool isAllowedBA)
        {
            // Arrange
            var mockDataReader = new Mock<IDataReader>();
            SetUpObjectDb(mockDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            metaModel.HasComm = hasComm;
            metaModel.IsAllowedBA = isAllowedBA;

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);

                var contentLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var header = contentLines.First();

                // Assert
                Assert.True(IsCorrectHeader(header, hasComm, isAllowedBA));

                streamWriter.Dispose();
                TearDown(fileName);
            }
        }

        [Fact]
        public async Task Export_NoData_GenerateNoInfoLine()
        {
            // Arrange
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { false, true }).Dequeue);
            mockDataReader.Setup(dr => dr.NextResult()).Returns(true);
            mockDataReader.Setup(r => r["TotalRows"]).Returns(0);
            SetUpObjectDb(mockDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);

                var contentLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var noInfoLine = contentLines.ElementAt(1);

                // Assert
                Assert.Equal(ViewResources.NoInfo, noInfoLine);

                streamWriter.Dispose();
                TearDown(fileName);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task Export_HaveData_GenerateContentRowCorrectly(bool hasComm, bool isAllowedBA)
        {
            // Arrange
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockDataReader.Setup(dr => dr.NextResult()).Returns(false);
            var expected = _fixture.Create<CustomerBalance>();
            ConfigureReaderForResult(expected, mockDataReader);
            SetUpObjectDb(mockDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            metaModel.HasComm = hasComm;
            metaModel.IsAllowedBA = isAllowedBA;

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);
                var contentLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // Assert
                AssertBodyData(expected, contentLines, hasComm, isAllowedBA);

                streamWriter.Dispose();
                TearDown(fileName);
            }
        }

        [Fact]
        public async Task Export_HaveData_GenerateFooterRowCorrectly()
        {
            // Arrange
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { false, true }).Dequeue);
            mockDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            ConfigureReaderForTotalData(expected, 1, mockDataReader);
            SetUpObjectDb(mockDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            metaModel.HasComm = true;
            metaModel.IsAllowedBA = true;

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);
                var totalLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // Assert
                AssertTotalData(expected, totalLines);

                streamWriter.Dispose();
                TearDown(fileName);
            }
        }

        private static void AssertBodyData(CustomerBalance expected, string[] contentLines, bool hasComm = true, bool isAllowedBA = true)
        {
            var dataLines = contentLines.ElementAt(1).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var externalId = "=" + expected.ExternalId;

            Assert.Equal("1", dataLines[0]);
            Assert.Equal(externalId, dataLines[1]);
            Assert.Equal(expected.SystemId, dataLines[2]);
            AssertCommonData(expected, dataLines, hasComm);

            if (isAllowedBA)
            {
                AssertBA(expected, dataLines);
            }
        }

        private static void AssertTotalData(CustomerBalance expected, string[] contentLines, bool hasComm = true)
        {
            var dataLines = contentLines.ElementAt(1).Split(new[] { ',' });

            Assert.Empty(dataLines[0]);
            Assert.Empty(dataLines[1]);
            Assert.Equal(ViewResources.Total, dataLines[2]);
            AssertCommonData(expected, dataLines, hasComm);
            AssertBA(expected, dataLines);
        }

        private static void AssertCommonData(CustomerBalance expected, string[] dataLines, bool hasComm)
        {
            var netTurnover = expected.Turnover - expected.DrawGame;

            Assert.Equal(expected.OpeningBalance.ToString(), dataLines[3]);
            Assert.Equal(expected.Deposit.ToString(), dataLines[4]);
            Assert.Equal(expected.Withdrawal.ToString(), dataLines[5]);
            Assert.Equal(expected.Bonus.ToString(), dataLines[6]);
            Assert.Equal(expected.Adjustment.ToString(), dataLines[7]);
            Assert.Equal(expected.Promotion.ToString(), dataLines[8]);
            Assert.Equal(expected.Turnover.ToString(), dataLines[9]);
            Assert.Equal(expected.DrawGame.ToString(), dataLines[10]);
            Assert.Equal(netTurnover.ToString(), dataLines[11]);
            Assert.Equal(expected.PendingFunds.ToString(), dataLines[12]);
            Assert.Equal(expected.WinLoss.ToString(), dataLines[13]);

            var indexClosingBalnce = hasComm ? 16 : 14;
            var indexAffTransfer = hasComm ? 17 : 15;
            Assert.Equal(expected.ClosingBalance.ToString(), dataLines[indexClosingBalnce]);
            Assert.Equal(expected.AffiliateTransfer.ToString(), dataLines[indexAffTransfer]);

            if (hasComm)
            {
                AssertWinLossComm(expected, dataLines);
            }
        }

        private static void AssertWinLossComm(CustomerBalance expected, string[] dataLines)
        {
            var totalWinLoss = expected.WinLoss + expected.PlayerComm;

            Assert.Equal(expected.PlayerComm.ToString(), dataLines[14]);
            Assert.Equal(totalWinLoss.ToString(), dataLines[15]);
        }

        private static void AssertBA(CustomerBalance expected, string[] dataLines)
        {
            Assert.Equal(expected.TurnoverBA.ToString(), dataLines[18]);
            Assert.Equal(expected.CommBA.ToString(), dataLines[19]);
            Assert.Equal(expected.AmountDueToBA.ToString(), dataLines[20]);
        }

        private bool IsCorrectHeader(string header, bool hasComm, bool isAllowedBA)
        {
            var textHeader = _commonHeaderText.ToList();
            if (hasComm)
            {
                textHeader.Add($"{ViewResources.CustomerW_L} {ViewResources.Comm}");
                textHeader.Add($"{ViewResources.CustomerW_L} {ViewResources.Total}");
            }

            textHeader.Add(ViewResources.ClosingBalance);
            textHeader.Add(ViewResources.AffiliateTransfer);

            if (isAllowedBA)
            {
                textHeader.Add($"{ViewResources.BA} {ViewResources.Turnover}");
                textHeader.Add(ViewResources.BACommission);
                textHeader.Add(ViewResources.AmtDueToBA);
            }

            var expectedHeader = string.Join(CSVSeparateCharacter, textHeader);

            return expectedHeader.Equals(header);
        }
    }
}