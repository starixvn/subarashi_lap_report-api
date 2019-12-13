using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using OfficeOpenXml;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Excels
{
    public class CustomerWinLossExcelTest : BaseReportExcelTest
    {
        private readonly string[] HeaderTextList = new[]
            {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.EffectiveBettingValue,
                ViewResources.LicenseeW_L,
                ViewResources.BACommission,
                ViewResources.AmtDueToBA
            };

        public const string FormatDate = "MM/dd/yyyy";

        private const int StartHeaderRowIndex = 2;
        private const int EndHeaderRowIndex = 3;
        private const int ContentRowIndex = 4;
        private const int StartCustomerWLColumn = 6;
        private const int EndCustomerWLColumn = 8;
        private const int TotalRowIndex = 5;

        private const int ColumnIndex = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnTurnover = 4;
        private const int ColumnTotalTransaction = 5;

        private const int ColumnCustomerWl = 6;
        private const int ColumnCustomerComm = 7;
        private const int ColumnCustomerTotal = 8;

        private const int ColumnBetEffective = 9;
        private const int ColumnLicenseeWl = 10;
        private const int ColumnBaCommission = 11;
        private const int ColumnAmtDueToBa = 12;

        [Fact]
        public void Export_WhenCall_RenderTitleCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<CustomerWinLossMetaModel>();
            var expectedTitle = BuildExcelTitle(metaModel);

            // Action
            var fileName = DoExport(metaModel);

            // Assert
            using (var excel = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                var result = worksheet.GetValue<string>(1, 1);

                Assert.Equal(expectedTitle, result);
            }

            DeleteFile(fileName);
        }

        [Fact]
        public void Export_WhenCall_RenderHeaderCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<CustomerWinLossMetaModel>();
            metaModel.SystemIdVisible = true;
            metaModel.HasComm = true;
            metaModel.IsAllowedBA = true;

            // Action
            var fileName = DoExport(metaModel);

            // Assert
            AssertHeader(fileName);

            DeleteFile(fileName);
        }

        [Fact]
        public void Export_NoData_RenderRowContentCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<CustomerWinLossMetaModel>();

            // Action
            var fileName = DoExport(metaModel);

            // Assert
            using (var excel = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                var result = worksheet.GetValue<string>(4, 1);

                Assert.Equal(ViewResources.NoInfo, result);
            }

            DeleteFile(fileName);
        }

        [Fact]
        public void Export_ProcessUI_HideColumnByConfigCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<CustomerWinLossMetaModel>();
            metaModel.SystemIdVisible = false;
            metaModel.HasComm = false;
            metaModel.IsAllowedBA = false;

            // Action
            var fileName = DoExport(metaModel);

            // Assert
            using (var excel = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();

                Assert.True(worksheet.Column(ColumnSystemId).Hidden);
                Assert.True(worksheet.Column(ColumnCustomerTotal).Hidden);
                Assert.True(worksheet.Column(ColumnCustomerComm).Hidden);
                Assert.True(worksheet.Column(ColumnBaCommission).Hidden);
                Assert.True(worksheet.Column(ColumnAmtDueToBa).Hidden);

                Assert.True(worksheet.Row(EndHeaderRowIndex).Hidden);
            }

            DeleteFile(fileName);
        }

        [Fact]
        public void Export_HaveData_RenderContentRowCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<CustomerWinLossMetaModel>();
            metaModel.SystemIdVisible = true;
            metaModel.HasComm = true;
            metaModel.IsAllowedBA = true;

            var dataItem = fixture.Create<CustomerWinLoss>();
            var resultData = new ResultModel<CustomerWinLoss>
            {
                DataList = new List<CustomerWinLoss> { dataItem },
                TotalRows = 1,
                TotalData = dataItem
            };

            // Action
            var fileName = DoExport(metaModel, resultData);

            // Assert
            using (var excel = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();

                Assert.Equal(1, worksheet.GetValue<int>(ContentRowIndex, ColumnIndex));
                Assert.Equal(dataItem.ExternalId, worksheet.GetValue<string>(ContentRowIndex, ColumnExternalId));
                Assert.Equal(dataItem.SystemId, worksheet.GetValue<string>(ContentRowIndex, ColumnSystemId));
                AssertCommonCells(dataItem, worksheet);
            }

            DeleteFile(fileName);
        }

        [Fact]
        public void Export_HaveData_RenderTotalRowCorrectly()
        {
            // Arrange
            var metaModel = fixture.Create<CustomerWinLossMetaModel>();
            metaModel.SystemIdVisible = true;
            metaModel.HasComm = true;
            metaModel.IsAllowedBA = true;

            var dataItem = fixture.Create<CustomerWinLoss>();
            var resultData = new ResultModel<CustomerWinLoss>
            {
                DataList = new List<CustomerWinLoss> { dataItem },
                TotalRows = 1,
                TotalData = dataItem
            };

            // Action
            var fileName = DoExport(metaModel, resultData);

            // Assert
            using (var excel = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();

                var mergedCellRange = worksheet.Cells[TotalRowIndex, ColumnIndex, TotalRowIndex, ColumnSystemId];
                Assert.True(mergedCellRange.Merge);
                Assert.Equal(ViewResources.Total, worksheet.GetValue<string>(TotalRowIndex, 1));

                AssertCommonCells(dataItem, worksheet);
            }

            DeleteFile(fileName);
        }

        private string DoExport(CustomerWinLossMetaModel metaModel, ResultModel<CustomerWinLoss> dataResult = null)
        {
            var fileTemplate = CreateWorkBook();
            var data = dataResult ?? new ResultModel<CustomerWinLoss>
            {
                DataList = new List<CustomerWinLoss>(),
                TotalRows = 0
            };
            var excelService = CreateExcelService();

            var fileName = excelService.Export(fileTemplate, data, metaModel);
            DeleteFile(fileTemplate);

            return fileName;
        }

        private CustomerWinLossExcel CreateExcelService()
        {
            return new CustomerWinLossExcel(
                FileService.Object,
                ExcelOptionAccessor.Object,
                ExcelEnviroment.Object);
        }

        private void AssertHeader(string fileName)
        {
            var excelFile = new FileInfo(fileName);
            using (var excel = new ExcelPackage(excelFile))
            {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                var columns = worksheet.Dimension.Columns;
                for (int j = 1; j <= columns; j++)
                {
                    string content = worksheet.GetValue<string>(StartHeaderRowIndex, j);

                    if (j < StartCustomerWLColumn || j > EndCustomerWLColumn)
                    {
                        Assert.Equal(HeaderTextList[j - 1], content);
                    }
                }

                Assert.Equal(12, columns);
                Assert.Equal($"{ViewResources.Win}/{ViewResources.Loss}", worksheet.GetValue<string>(EndHeaderRowIndex, 6));
                Assert.Equal(ViewResources.Comm, worksheet.GetValue<string>(EndHeaderRowIndex, 7));
                Assert.Equal(ViewResources.Total, worksheet.GetValue<string>(EndHeaderRowIndex, 8));
            }
        }

        private static string BuildExcelTitle(CustomerWinLossMetaModel metaModel)
        {
            var filterTitle = ExcelTitleBuilder.GetTitleCurrencyName(ViewResources.Viewin, metaModel.IsConvert, metaModel.BaseCurrency, metaModel.CurrencyName)
                            + ExcelTitleBuilder.GetLicenseeWLProduct(metaModel.ProductOptions, ViewResources.LicenseeW_L)
                            + (string.IsNullOrEmpty(metaModel.Username) ? string.Empty : $", {ViewResources.Username}: {metaModel.Username}");

            return string.Format(ViewResources.CustomerWinLossTitle,
                                        metaModel.FromDate.ToString(FormatDate),
                                        metaModel.ToDate.ToString(FormatDate),
                                        filterTitle);
        }

        private static void AssertCommonCells(CustomerWinLoss dataItem, ExcelWorksheet worksheet)
        {
            var customerTotal = dataItem.CustomerWL + dataItem.PlayerComm;
            Assert.Equal(dataItem.Turnover, worksheet.GetValue<decimal>(ContentRowIndex, ColumnTurnover));
            Assert.Equal(dataItem.TotalTransaction, worksheet.GetValue<int>(ContentRowIndex, ColumnTotalTransaction));
            Assert.Equal(dataItem.CustomerWL, worksheet.GetValue<decimal>(ContentRowIndex, ColumnCustomerWl));
            Assert.Equal(dataItem.PlayerComm, worksheet.GetValue<decimal>(ContentRowIndex, ColumnCustomerComm));
            Assert.Equal(customerTotal, worksheet.GetValue<decimal>(ContentRowIndex, ColumnCustomerTotal));
            Assert.Equal(dataItem.BetEffective, worksheet.GetValue<decimal>(ContentRowIndex, ColumnBetEffective));
            Assert.Equal(dataItem.LicenseeWL, worksheet.GetValue<decimal>(ContentRowIndex, ColumnLicenseeWl));
            Assert.Equal(dataItem.BAComm, worksheet.GetValue<decimal>(ContentRowIndex, ColumnBaCommission));
            Assert.Equal(dataItem.AmtDueToBA, worksheet.GetValue<decimal>(ContentRowIndex, ColumnAmtDueToBa));
        }
    }
}