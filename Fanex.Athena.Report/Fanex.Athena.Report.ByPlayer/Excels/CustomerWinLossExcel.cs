namespace Fanex.Athena.Report.ByPlayer.Excels
{
    using System;
    using System.IO;
    using System.Linq;
    using Fanex.Athena.Report.ByPlayer.DTO;
    using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using NPOI.SS.UserModel;

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

        public CustomerWinLossExcel(IExcelBuilder builder)
        {
            _builder = builder;
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

        private IWorkbook GetWorkbook(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(TemplateSheetName);
            var title = BuildExcelTitle(metaModel);

            if (data.DataList.Any())
            {
                var numberOfSheet = ExcelHelper.GetNumberOfSheets(data.TotalRows, metaModel.PageSize);

                for (int i = 0; i < numberOfSheet; i++)
                {
                    var sheet = _builder.CloneSheet(workBook, TemplateSheetName, "Data" + (i + 1).ToString());
                    AssignTitle(sheet, templateSheet, title);
                    AssignHeader(sheet, templateSheet);
                    AssignSheetData(sheet, templateSheet, i, metaModel.PageSize, data);
                    ProcessUI(sheet, metaModel);
                }
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