using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Common.Extensions;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class TurnoverExcel : ITurnoverExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        public const string FormatDate = "MM/dd/yyyy";

        public const int PageSize = 30000;

        protected const int ColumnIndex = 0;
        private const int ColumnDay = 1;
        private const int ColumnOpeningBalance = 2;
        private const int ColumnDeposit = 3;
        private const int ColumnWithdrawal = 4;
        private const int ColumnBonus = 5;
        private const int ColumnAdjustment = 6;
        private const int ColumnPromotion = 7;
        private const int ColumnTurnOver = 8;
        private const int ColumnDrawGame = 9;
        private const int ColumnNetTurnOver = 10;
        private const int ColumnPendingFunds = 11;
        private const int ColumnCustomerWinLoss = 12;
        private const int ColumnCustomerComm = 13;
        private const int ColumnCustomerTotal = 14;
        private const int ColumnClosingBalance = 15;
        private const int ColumnAffiliateTransfer = 16;
        private const int ColumnCasinoTransfer = 17;
        private const int ColumnTemplateCount = 18;

        public const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 3;
        private const int RowNoInfoTemp = 5;
        private const int RowTableHeaderTemplate = 1;
        private const int RowTotalTemplate = 4;

        private TurnoverMetaModel _metaModel;

        private TurnoverExcelOption _options;

        private IList<TurnoverDto> _data;

        private TurnoverDto _totalData;

        private readonly IExcelBuilder _builder;

        public TurnoverExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, TurnoverResultModel data, TurnoverMetaModel metaModel, TurnoverExcelOption excelOption)
        {
            _metaModel = metaModel;
            _options = excelOption;
            _data = data.Data;
            _totalData = data.TotalData;

            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        private IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            if (_data.Any())
            {
                var numberOfSheets = GetNumberOfSheets(_data.Count, PageSize);

                for (var i = 0; i < numberOfSheets; i++)
                {
                    var sheet = _builder.CloneSheet(workBook, TemplateSheetName, "Data" + (i + 1));

                    AssignTitle(sheet, templateSheet);
                    AssignHeader(sheet, templateSheet);
                    AssignSheetData(sheet, templateSheet, i);
                    ProcessUi(sheet);
                }
            }
            else
            {
                var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);

                AssignTitle(sheet, templateSheet);
                AssignHeader(sheet, templateSheet);
                AssignNoInfoRow(sheet, templateSheet);
                ProcessUi(sheet);
            }

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);
            var row2 = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate + 1, RowHeaderTemplate + 1);
            var headerTextList = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.OpeningBalance,
                ViewResources.Deposit,
                ViewResources.Withdrawal,
                ViewResources.Bonus,
                ViewResources.Adjustment,
                ViewResources.ExtraPayout,
                ViewResources.Turnover,
                ViewResources.DrawGame,
                ViewResources.NetTurnover,
                ViewResources.PendingFunds,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.ClosingBalance,
                ViewResources.AffiliateTransfer,
                ViewResources.CasinoTransfer
            };

            ExcelHelper.SetTextRowHeader(row, headerTextList);

            row2.Cells[ColumnCustomerWinLoss].SetCellValue(ViewResources.WinLoss);
            row2.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row2.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemp, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        /// <summary>
        /// Assign data to excel sheet.
        /// </summary>
        /// <param name="sheet">Excel sheet.</param>
        /// <param name="templateSheet">Template Excel sheet.</param>
        /// <param name="pageIndex">Start index.</param>
        /// <returns>Row index.</returns>
        protected void AssignSheetData(ISheet sheet, ISheet templateSheet, int pageIndex)
        {
            var rowCount = Math.Min(_data.Count, checked((pageIndex + 1) * PageSize));
            var currentRow = 3;

            for (var index = pageIndex * PageSize; index < rowCount; index++)
            {
                var turnover = _data[index];

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, currentRow);

                row.Cells[ColumnIndex].SetCellValue(index + 1);
                row.Cells[ColumnDay].SetCellValue(turnover.Date);
                row.Cells[ColumnOpeningBalance].SetCellValue((double)turnover.OpeningBalance);
                row.Cells[ColumnClosingBalance].SetCellValue((double)turnover.ClosingBalance);

                AssignCommonData(row, turnover);

                currentRow++;
            }

            AssignTotalCellData(sheet, templateSheet, currentRow);
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var titleNoUserName = string.Format(
                        ViewResources.TurnoverTitleNoUser,
                        _metaModel.FromDate.ToString(FormatDate),
                        _metaModel.ToDate.ToString(FormatDate),
                        ExcelHelper.GetTitleBaseCurrency(_metaModel.BaseCurrency, _metaModel.CurrencyName));
            var title = string.Format(
                                        ViewResources.TurnoverTitle,
                                         _metaModel.FromDate.ToString(FormatDate),
                                         _metaModel.ToDate.ToString(FormatDate),
                                         ViewResources.Customer,
                                         _metaModel.Username,
                                         _metaModel.BaseCurrency.ToUpperInvariant());
            var reportTitle = _metaModel.Username.IsNullOrTrimEmpty() ? titleNoUserName : title;
            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        /// <summary>
        /// Process UI, hide redundant row and column.
        /// </summary>
        /// <param name="sheet">Excel sheet.</param>
        protected void ProcessUi(ISheet sheet)
        {
            if (!_options.CustomerCommVisible)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);

                // If row isn't hide (in case BA visible), then merge cell and set value for header
                _builder.MergeRows(sheet, RowTableHeaderTemplate, RowTableHeaderTemplate + 1, ColumnCustomerWinLoss);
                sheet.GetRow(RowTableHeaderTemplate).GetCell(ColumnCustomerWinLoss).SetCellValue(ViewResources.CustomerW_L);
            }

            if (!_options.CustomerCommVisible)
            {
                _builder.HideColumn(sheet, RowTableHeaderTemplate + 1);
            }

            if (!_options.IsShowCasinoTransfer)
            {
                _builder.HideColumn(sheet, ColumnCasinoTransfer);
            }

            if (_options.IsEnableSingleWallet)
            {
                _builder.HideColumn(sheet, ColumnOpeningBalance);
                _builder.HideColumn(sheet, ColumnDeposit);
                _builder.HideColumn(sheet, ColumnWithdrawal);
                _builder.HideColumn(sheet, ColumnClosingBalance);
            }

            if (_options.IsHidePlayerWL)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm, ColumnCustomerWinLoss);
            }
        }

        private static void AssignCommonData(IRow row, TurnoverDto turnover)
        {
            row.Cells[ColumnDeposit].SetCellValue((double)turnover.Deposit);
            row.Cells[ColumnWithdrawal].SetCellValue((double)turnover.Withdrawal);
            row.Cells[ColumnBonus].SetCellValue((double)turnover.Bonus);
            row.Cells[ColumnAdjustment].SetCellValue((double)turnover.Adjustment);
            row.Cells[ColumnPromotion].SetCellValue((double)turnover.Promotion);
            row.Cells[ColumnTurnOver].SetCellValue((double)turnover.Turnover);
            row.Cells[ColumnDrawGame].SetCellValue((double)turnover.DrawGame);
            row.Cells[ColumnNetTurnOver].SetCellValue((double)turnover.NetTurnover);
            row.Cells[ColumnPendingFunds].SetCellValue((double)turnover.PendingFunds);
            row.Cells[ColumnCustomerWinLoss].SetCellValue((double)turnover.WinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue((double)turnover.PlayerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)(turnover.WinLoss + turnover.PlayerComm));
            row.Cells[ColumnAffiliateTransfer].SetCellValue((double)turnover.AffiliateTransfer);
            row.Cells[ColumnCasinoTransfer].SetCellValue((double)turnover.Transfer3rd);
        }

        protected void AssignTotalCellData(ISheet sheet, ISheet templateSheet, int currentRow)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, currentRow);
            row.Cells[0].SetCellValue(ViewResources.Total);
            row.Cells[ColumnOpeningBalance].SetCellValue("-");
            row.Cells[ColumnClosingBalance].SetCellValue("-");

            AssignCommonData(row, _totalData);

            _builder.MergeCols(sheet, ColumnIndex, ColumnDay, currentRow);
        }

        protected static int GetNumberOfSheets(int numberOfItems, int pageSize)
        {
            return numberOfItems % pageSize == 0 ? numberOfItems / pageSize : (numberOfItems / pageSize) + 1;
        }
    }
}