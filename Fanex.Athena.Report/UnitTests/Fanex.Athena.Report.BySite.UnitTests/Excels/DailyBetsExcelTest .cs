using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.Extensions;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Excels;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Resources;
using Fanex.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Excels
{
    public class DailyBetsExcelTest
    {
        protected static readonly IFixture fixture = new Fixture();

        private readonly Mock<IHostingEnvironment> ExcelEnviroment;

        private readonly Mock<IOptions<ExcelOptions>> ExcelOptionAccessor;

        private readonly Mock<IFileService> FileService;

        private Mock<IDailyBetsRepository> Repository;

        protected readonly string RootPath = Directory.GetCurrentDirectory();

        protected readonly ExcelOptions ExcelOptions = new ExcelOptions()
        {
            Path = "",
            CSVSeparateCharacter = ",",
            MaximumRowPerFile = 10
        };

        private static readonly string[] _commonHeaderText = {
                 "#",
                ViewResources.PlayerName,
                ViewResources.Currency,
                ViewResources.RefNo,
                ViewResources.TransTime,
                ViewResources.Product,
                ViewResources.BetType,
                ViewResources.Turnover,
                ViewResources.WinLoss,
                $"{ViewResources.Turnover} (USD)",
                $"{ViewResources.WinLoss} (USD)",
                ViewResources.Status
            };

        public DailyBetsExcelTest()
        {
            ExcelEnviroment = new Mock<IHostingEnvironment>();
            ExcelEnviroment.Setup(e => e.ContentRootPath).Returns(RootPath);

            ExcelOptionAccessor = CreateMockExcelOption();
            FileService = CreateMockFileService();

            DbSettingProviderManager
             .StartTestingSession()
             .Run();
        }

        protected Mock<IFileService> CreateMockFileService()
        {
            var fileService = new Mock<IFileService>();
            var directoryInfo = new DirectoryInfo(RootPath);
            fileService.Setup(s => s.GetParentDirectory(RootPath)).Returns(directoryInfo);

            return fileService;
        }

        protected Mock<IOptions<ExcelOptions>> CreateMockExcelOption()
        {
            var excelOptionsAccessor = new Mock<IOptions<ExcelOptions>>();
            excelOptionsAccessor.Setup(eoa => eoa.Value).Returns(ExcelOptions);

            return excelOptionsAccessor;
        }

        protected Mock<IDailyBetsRepository> CreateMockRepository(DailyBetsMetaModel metaModel,int totalRows, AuditTicket totalData)
        {
            var repository = new Mock<IDailyBetsRepository>();
            repository.Setup(s => s.GetTotalDataAsync(metaModel)).Returns(Task.FromResult((totalRows, totalData)));

            return repository;
        }

        private DailyBetsExcel CreateExcelService(StreamWriter streamWriter)
        {
            FileService.Setup(f => f.CreateStreamWriter(It.Is<string>(x => x.Contains(RootPath)), false, Encoding.UTF8))
            .Returns(streamWriter);

            return new DailyBetsExcel(Repository.Object, FileService.Object, ExcelOptionAccessor.Object, ExcelEnviroment.Object);
        }

        [Fact]
        public async Task Export_WhenCall_PassesCorrectParameter()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            var mockObjectDB = SetUpObjectDb(mockIDataReader);
            var metaModel = fixture.Create<DailyBetsMetaModel>();
            var auditTicket = fixture.Create<AuditTicket>();          
            Repository = CreateMockRepository(metaModel, 1, auditTicket);

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);

                // Assert
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("SiteName", metaModel.SiteName), Times.Once);
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("Date", metaModel.Date), Times.Once);
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("ProductCodes", metaModel.ProductCodeList), Times.Once);
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("CurrencyList", DailyBetsMetaModelExtension.CurrencyListToXml(metaModel)), Times.Once);
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("PageSize", metaModel.PageSize), Times.Once);
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("PageIndex", metaModel.PageIndex), Times.Once);
                mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("BaseCurrency", metaModel.BaseCurrency), Times.Once);

                streamWriter.Dispose();
                DeleteFile(fileName);
            }
        }

        [Fact]
        public async Task Export_WhenCall_GenerateHeaderCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<DailyBetsMetaModel>();
            var auditTicket = fixture.Create<AuditTicket>();
            var mockDataReader = new Mock<IDataReader>();
            SetUpObjectDb(mockDataReader);
            Repository = CreateMockRepository(metaModel, 1, auditTicket);

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var file = await excelService.Export(metaModel);

                var contentLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var header = contentLines.First();

                // Assert
                Assert.True(IsCorrectHeader(header));

                streamWriter.Dispose();
                DeleteFile(file);
            }
        }

        [Fact]
        public async Task Export_NoData_ReturnNull()
        {
            // Arrange
            var metaModel = fixture.Create<DailyBetsMetaModel>();
            var auditTicket = new AuditTicket();
            Repository = CreateMockRepository(metaModel, 0, auditTicket);

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var file = await excelService.Export(metaModel);

                // Assert
                Assert.Null(file);

                streamWriter.Dispose();
            }
        }

        [Fact]
        public async Task Export_HaveData_GenerateContentRowCorrectly()
        {
            // Arrange
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockDataReader.Setup(dr => dr.NextResult()).Returns(false);

            var expected = fixture.Create<AuditTicket>();
            expected.ProductCode = "SB";
            expected.Status = "1";

            ConfigureReaderForResult(expected, mockDataReader);
            SetUpObjectDb(mockDataReader);
            var metaModel = fixture.Create<DailyBetsMetaModel>();
            Repository = CreateMockRepository(metaModel, 1, expected);

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);
                var contentLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // Assert
                AssertBodyData(expected, contentLines);

                streamWriter.Dispose();
                DeleteFile(fileName);
            }
        }

        [Fact]
        public async Task Export_HaveData_GenerateFooterRowCorrectly()
        {
            // Arrange
            // Arrange
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockDataReader.Setup(dr => dr.NextResult()).Returns(false);

            var expected = fixture.Create<AuditTicket>();
            expected.ProductCode = "SB";
            expected.Status = "1";

            ConfigureReaderForResult(expected, mockDataReader);
            SetUpObjectDb(mockDataReader);
            var metaModel = fixture.Create<DailyBetsMetaModel>();
            Repository = CreateMockRepository(metaModel, 1, expected);

            using (var memoryStream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(memoryStream);
                var excelService = CreateExcelService(streamWriter);

                // Action
                var fileName = await excelService.Export(metaModel);
                var totalLines = Encoding.UTF8.GetString(memoryStream.ToArray())
                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // Assert
                AssertTotalData(expected, metaModel, totalLines);

                streamWriter.Dispose();
                DeleteFile(fileName);
            }
        }

        private static void AssertBodyData(AuditTicket expected, string[] contentLines)
        {
            var dataLines = contentLines.ElementAt(1).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal("1", dataLines[0]);
            Assert.Equal(expected.PlayerName, dataLines[1]);
            Assert.Equal(expected.CurrencyName, dataLines[2]);
            Assert.Equal(expected.TransTime.ToString(), dataLines[5]);
            Assert.Equal(DailyBetsMetaModelExtension.GetProductName[expected.ProductCode.ToString()](), dataLines[6]);
            Assert.Equal(expected.BetType, dataLines[7]);
            Assert.Equal(expected.Turnover.ToString(), dataLines[8]);
            Assert.Equal(expected.WinLoss.ToString(), dataLines[9]);
            Assert.Equal(DailyBetsMetaModelExtension.GetStatusName[Convert.ToInt16(expected.Status)](), dataLines[10]);
        }

        private static void AssertTotalData(AuditTicket expected, DailyBetsMetaModel model, string[] contentLines)
        {
            var dataLines = contentLines.ElementAt(2).Split(new[] { ',' });

            Assert.Equal(String.Format(ViewResources.Totalin, model.BaseCurrency), dataLines[8]);
            Assert.Equal(expected.TurnoverBased.ToString("#.00"), dataLines[9]);
            Assert.Equal(expected.WinLossBased.ToString("#.00"), dataLines[10]);
        }

        private bool IsCorrectHeader(string header)
        {
            var textHeader = _commonHeaderText.ToList();
            
            var expectedHeader = string.Join(ExcelOptions.CSVSeparateCharacter, textHeader);

            return expectedHeader.Equals(header);
        }

        protected void DeleteFile(params string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        public static Mock<IObjectDb> SetUpObjectDb(Mock<IDataReader> mockIDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockIDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }

        private static void ConfigureReaderForResult(AuditTicket expected, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["Index"]).Returns(1);
            reader.Setup(r => r["PlayerName"]).Returns(expected.PlayerName);
            reader.Setup(r => r["CurrencyName"]).Returns(expected.CurrencyName);
            reader.Setup(r => r["RefNo"]).Returns(expected.RefNo);
            reader.Setup(r => r["TransTime"]).Returns(expected.TransTime);
            reader.Setup(r => r["ProductCode"]).Returns(expected.ProductCode);
            reader.Setup(r => r["BetType"]).Returns(expected.BetType);
            reader.Setup(r => r["Turnover"]).Returns(expected.Turnover);
            reader.Setup(r => r["WinLoss"]).Returns(expected.WinLoss);
            reader.Setup(r => r["Status"]).Returns(expected.Status);
        }
    }
}