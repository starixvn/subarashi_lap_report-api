namespace Fanex.Athena.Report.ByPlayer.Excels
{
    using Fanex.Athena.Report.ByPlayer.DTO;
    using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Ionic.Zip;
    using Microsoft.Extensions.Options;
    using NPOI.SS.UserModel;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CustomerWinLossExcel : ICustomerWinLossExcel
    {
        public const string FormatDate = "MM/dd/yyyy";

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;
        private const int RowNoInfoTemplate = 5;

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 1;
        private const int ColumnSystemId = 2;
        private const int ColumnTurnover = 3;
        private const int ColumnTotalTransaction = 4;
        private const int ColumnCustomerWl = 5;
        private const int ColumnCustomerComm = 6;
        private const int ColumnCustomerTotal = 7;
        private const int ColumnBetEffective = 8;
        private const int ColumnLicenseeWl = 9;
        private const int ColumnBaCommission = 10;
        private const int ColumnAmtDueToBa = 11;

        private const int ColumnTemplateCount = 12;

        private readonly IExcelBuilder _builder;

        private readonly ITempFileService _tempFileService;

        private readonly ExcelFileOptions _excelFileOptions;

        public CustomerWinLossExcel(IExcelBuilder builder, ITempFileService tempFileService, IOptions<ExcelFileOptions> excelFileOptions)
        {
            _builder = builder;
            _tempFileService = tempFileService;
            _excelFileOptions = excelFileOptions.Value;
        }

        public byte[] Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public byte[] ExportFile(string template, IList<ResultModel<CustomerWinLoss>> data, CustomerWinLossMetaModel metaModel)
        {
            var excelFiles = GetListPathWorkbook(template, data, metaModel);
            var stream = CreateZipFile(excelFiles);
            return stream.ToArray();
        }

        private MemoryStream CreateZipFile(IList<string> excelFiles)
        {
            var mem = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var count = 1;
                foreach (var filePath in excelFiles)
                {
                    zip.AddEntry($"CustomerWinLoss{count}.xlsx", _tempFileService.ReadBytes(filePath));
                    count++;
                }

                CleanDataFile(excelFiles);
                zip.Save(mem);
            }

            return mem;
        }

        private void CleanDataFile(IList<string> dataPath)
        {
            foreach (var item in dataPath)
            {
                var dataFilePath = Path.Combine(_excelFileOptions.WebAppFileLAP, item);
                _tempFileService.Delete(dataFilePath);
            }
        }

        private IWorkbook GetWorkbook(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            const byte pageIndex = 0;
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(TemplateSheetName);
            var title = BuildExcelTitle(metaModel);

            if (data.DataList.Any())
            {
                var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
                AssignTitle(sheet, templateSheet, title);
                AssignHeader(sheet, templateSheet);
                AssignSheetData(sheet, templateSheet, pageIndex, metaModel.PageSize, data);
                ProcessUI(sheet, metaModel);
            }
            else
            {
                var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
                AssignTitle(sheet, templateSheet, title);
                AssignHeader(sheet, templateSheet);
                AssignNoInfoRow(sheet, templateSheet);
                ProcessUI(sheet, metaModel);
            }

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private IList<string> GetListPathWorkbook(string template, IList<ResultModel<CustomerWinLoss>> groupResult, CustomerWinLossMetaModel metaModel)
        {
            var excelFiles = new List<string>();

            foreach (var resultModel in groupResult)
            {
                var workBook = GetWorkbook(template, resultModel, metaModel);
                var dataFileName = $"{Guid.NewGuid().ToString()}.xlsx";
                var dataFilePath = Path.Combine(_excelFileOptions.WebAppFileLAP, dataFileName);
                SaveFile(workBook, dataFilePath);

                excelFiles.Add(dataFileName);
            }

            return excelFiles;
        }

        private static void SaveFile(IWorkbook workbook, string dataFilePath)
        {
            using (var stream = new FileStream(dataFilePath, FileMode.Create))
            {
                workbook.Write(stream);
                stream.Close();
                workbook.Close();
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

        private void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);

            var headerTextList = new[]
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
            ExcelHelper.SetTextRowHeader(row, headerTextList);

            row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate + 1, destRowIndex: RowHeaderTemplate + 1);

            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.WinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, string reportTitle)
        {
            _builder.SetRowValue(
                sheet,
                templateSheet,
                sourceRowIndex: RowTitleTemplate,
                destRowIndex: RowTitleTemplate,
                columnCount: ColumnTemplateCount,
                value: reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        private void AssignSheetData(ISheet sheet,
            ISheet templateSheet,
            int pageIndex,
            int pageSize,
            ResultModel<CustomerWinLoss> data)
        {
            var dataList = data.DataList;
            var rowCount = Math.Min(dataList.Count(), checked((pageIndex + 1) * pageSize));
            var rowIndex = 0;

            for (int index = pageIndex * pageSize; index < rowCount; index++)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, rowIndex + RowContentTemplate);

                var customerWinloss = dataList.ElementAt(index);

                row.Cells[ColumnIndex].SetCellValue(index + 1);
                row.Cells[ColumnExternalId].SetCellValue(customerWinloss.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(customerWinloss.SystemId);
                row.Cells[ColumnTurnover].SetCellValue(Convert.ToDouble(customerWinloss.Turnover));
                row.Cells[ColumnTotalTransaction].SetCellValue(customerWinloss.TotalTransaction);
                row.Cells[ColumnCustomerWl].SetCellValue(Convert.ToDouble(customerWinloss.CustomerWL));
                row.Cells[ColumnCustomerComm].SetCellValue(Convert.ToDouble(customerWinloss.PlayerComm));
                row.Cells[ColumnCustomerTotal].SetCellValue(Convert.ToDouble(customerWinloss.CustomerWL + customerWinloss.PlayerComm));
                row.Cells[ColumnBetEffective].SetCellValue(Convert.ToDouble(customerWinloss.BetEffective));
                row.Cells[ColumnLicenseeWl].SetCellValue(Convert.ToDouble(customerWinloss.LicenseeWL));
                row.Cells[ColumnBaCommission].SetCellValue(Convert.ToDouble(customerWinloss.BAComm));
                row.Cells[ColumnAmtDueToBa].SetCellValue(Convert.ToDouble(customerWinloss.AmtDueToBA));

                rowIndex++;
            }

            AssignSumData(sheet, templateSheet, data.TotalData, rowIndex);
        }

        private void ProcessUI(ISheet sheet, CustomerWinLossMetaModel metaModel)
        {
            if (!metaModel.SystemIdVisible)
            {
                _builder.HideColumn(sheet, ColumnSystemId);
            }

            if (!metaModel.IsAllowedCustomerComm)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);
                _builder.HideRow(sheet, RowHeaderTemplate + 1);
            }

            if (!metaModel.IsAllowBA)
            {
                _builder.HideColumn(sheet, ColumnBaCommission, ColumnAmtDueToBa);
            }
        }

        private void AssignSumData(ISheet sheet, ISheet templateSheet, CustomerWinLoss totalData, int startIndex)
        {
            var rowTotalNumber = checked(startIndex + RowContentTemplate);

            var row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, rowTotalNumber);

            row.Cells[ColumnTurnover].SetCellValue(Convert.ToDouble(totalData.Turnover));
            row.Cells[ColumnTotalTransaction].SetCellValue(totalData.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue(Convert.ToDouble(totalData.CustomerWL));
            row.Cells[ColumnCustomerComm].SetCellValue(Convert.ToDouble(totalData.PlayerComm));
            row.Cells[ColumnCustomerTotal].SetCellValue(Convert.ToDouble(totalData.CustomerWL + totalData.PlayerComm));
            row.Cells[ColumnBetEffective].SetCellValue(Convert.ToDouble(totalData.BetEffective));
            row.Cells[ColumnLicenseeWl].SetCellValue(Convert.ToDouble(totalData.LicenseeWL));
            row.Cells[ColumnBaCommission].SetCellValue(Convert.ToDouble(totalData.BAComm));
            row.Cells[ColumnAmtDueToBa].SetCellValue(Convert.ToDouble(totalData.AmtDueToBA));

            _builder.MergeCells(sheet, firstRow: rowTotalNumber, lastRow: rowTotalNumber, firstCol: ColumnIndex, lastCol: ColumnSystemId);
            row.Cells[0].SetCellValue(ViewResources.Total);
        }
    }
}