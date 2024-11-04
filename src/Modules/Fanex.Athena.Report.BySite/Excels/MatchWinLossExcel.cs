using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Resources;
using Fanex.Common;
using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class MatchWinLossExcel : IMatchWinLossExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        private const string NewProductIdColumnName = "newproductid";

        private const string MatchNameColumn = "matchname";

        public const string FormatDate = "MM/dd/yyyy";

        private const int ColumnIndex = 0;

        private const int ColumnMatch = 1;

        private const int ColumnCustomerWl = 4;

        private const int ColumnCustomerComm = 5;

        private const int ColumnCustomerTotal = 6;

        private const int ColumnBaCommission = 8;

        private const int ColumnAmtDueToBa = 9;

        private const int ColumnTemplateCount = 10;

        private const int ColumnNumbericDataStartIndex = 2;

        public const int RowTitleTemplate = 0;

        public const int RowHeaderTemplate = 1;

        private const int RowTableHeaderTemplate = 1;

        private const int RowMatchTemplate = 3;

        private const int RowContentTemplate = 4;

        private const int RowBetTypeContentTemplate = 5;

        private const int RowTotalTemplate = 6;

        private const int RowSpecialBetTypeTemplate = 7;

        private const int RowGrandTotalTemplate = 8;

        private const int RowNoInfoTemplate = 9;

        private const string BetTypeColumn = "bettype";

        private static readonly IEnumerable<int> SpecialBetTypes = new[]
        {
            BetType.MixParlay,
            BetType.Outright,
            BetType.CombinationMixParlay,
            BetType.VirtualSports2MixParlay
        };

        private static readonly IList<string> CommonColumnNameList = new List<string>
        {
            "turnover",
            "betcount",
            "winlost",
            "playercomm",
            "totalwl",
            "licenseewl",
            "commba",
            "amtduetoba"
        };

        private readonly IExcelBuilder _builder;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        private DataTable _contentTable;

        private MatchWinLossMetaModel _model;

        private MatchWinLossExcelOption _option;

        private DataTable _totalTable;

        public MatchWinLossExcel(ICacheRefAdapter cacheRefAdapter, IExcelBuilder builder)
        {
            _cacheRefAdapter = cacheRefAdapter;
            _builder = builder;
        }

        public byte[] Export(string template, DataSet data, MatchWinLossMetaModel metaModel,
                             MatchWinLossExcelOption excelOption)
        {
            _model = metaModel;
            _option = excelOption;

            _contentTable = GetContentData(data.Tables[0]);
            _totalTable = data.Tables[1];

            AddExtraColumns();

            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        private DataTable GetContentData(DataTable table)
        {
            table = AddColumnsForSorting(table);
            table.DefaultView.Sort = $"{NewProductIdColumnName} asc, leaguename asc, matchname ASC";
            return table.DefaultView.ToTable();
        }

        private DataTable AddColumnsForSorting(DataTable table)
        {
            table.Columns.Add(NewProductIdColumnName, typeof(int));
            table.Columns.Add(MatchNameColumn, typeof(string));
            table.Columns.Add("leaguename", typeof(string));

            foreach (DataRow row in table.Rows)
            {
                var betType = Converter.ToInt(row["bettype"]);
                var sport = Converter.ToInt(row["SportType"]);
                row[NewProductIdColumnName] = ProductHelper.GetProductId(betType);

                AssignMatchName(row, betType, sport);
                AssignLeagueName(row, sport);
            }

            return table;
        }

        protected internal virtual void AssignMatchName(DataRow row, int betType, int sport)
        {
            if ((int)row[NewProductIdColumnName] == ProductTypes.NumberGame)
            {
                var prefix = BetTypeHelper.IsNumberGameHappy5(betType) ?
                    ViewResources.No + "." :
                    ViewResources.NumberGameNo;
                row[MatchNameColumn] = string.Format("{0} {1}", prefix, row["matchcode"]);
            }
            else
            {
                if (SportHelper.IsVirtualBetRadarSports(sport))
                {
                    var distance = row["Distance"].ToString();
                    var vendorMatchId = row["VendorMatchId"].ToString();
                    var id = !string.IsNullOrEmpty(distance) ? $" - ID {distance}" : string.Empty;
                    row[MatchNameColumn] = $"{ViewResources.Event} {vendorMatchId}{id}";
                }
                else
                {
                    var homeName = _cacheRefAdapter.GetTeamName(row["homeid"]) +
                               (Converter.ToBool(row["isneutral"])
                                    ? " " + ViewResources.IsNeutral
                                    : string.Empty);
                    var awayName = _cacheRefAdapter.GetTeamName(row["awayid"]);
                    row[MatchNameColumn] = homeName + "-vs-" + awayName;
                }
            }
        }

        protected internal virtual void AssignLeagueName(DataRow row, int sportType)
        {
            row["leaguename"] = (int)row[NewProductIdColumnName] == ProductTypes.NumberGame ?
                ViewResources.ProductNG :
                GetLeagueName(row, sportType);
        }

        private string GetLeagueName(DataRow row, int sportType)
        {
            if (SportHelper.IsVirtualBetRadarSports(sportType))
            {
                return _cacheRefAdapter.GetLeagueName3rd(row["leagueid"]);
            }
            else
            {
                return _cacheRefAdapter.GetLeagueName(row["leagueid"]);
            }
        }

        private IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);

            if (_contentTable.Rows.Count > 0)
            {
                AssignSheetData(sheet, templateSheet);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            ProcessUi(sheet);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected virtual void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);

            var headerTextList = new[]
            {
                "#",
                ViewResources.Match,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L,
                ViewResources.BACommission,
                ViewResources.AmtDueToBA
            };
            SetTextRowHeader(row, headerTextList);

            row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate + 1, RowHeaderTemplate + 1);

            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.WinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        protected virtual void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowMatchTemplate, ColumnTemplateCount,
                ViewResources.NoInfo);
        }

        private static int CheckGreaterZero(int value) => value > 0 ? 1 : 0;

        protected virtual void AssignSheetData(ISheet sheet, ISheet templateSheet)
        {
            var currentRowIndex = 3;
            var leagueName = string.Empty;
            var matchId = 0;
            var matchName = "Default";
            var formatMatchName = string.Empty;
            var columnNameList = CommonColumnNameList;
            var rowCount = _contentTable.Rows.Count;
            var matchInfoList = new List<DataRow>();
            var currentIndex = 1;
            var specialBetType = new List<DataRow>();

            for (var index = 0; index < rowCount; index++)
            {
                var exportRow = _contentTable.Rows[index];
                var betType = Converter.ToInt(exportRow[BetTypeColumn]);

                if (SpecialBetTypes.Contains(betType) || IsHappy5LinkedGames(betType))
                {
                    specialBetType.Add(exportRow);
                    continue;
                }

                var currentLeagueName = Converter.ToString2(exportRow["leaguename"]);

                if (_option.IsHideSaba)
                {
                    currentLeagueName = SabaHelper.RemoveSabaText(currentLeagueName);
                }

                if (leagueName != currentLeagueName)
                {
                    currentRowIndex = AssignMatchData(sheet, templateSheet, matchInfoList, formatMatchName,
                        currentRowIndex,
                        currentIndex);
                    currentIndex += CheckGreaterZero(matchInfoList.Count);
                    matchInfoList = new List<DataRow>();
                    leagueName = currentLeagueName;
                    _builder.SetRowValue(sheet, templateSheet, RowMatchTemplate, currentRowIndex, ColumnTemplateCount,
                        leagueName);
                    currentRowIndex++;
                }

                var currentMatchId = Converter.ToInt(exportRow["matchid"]);
                var currentMatchName = Converter.ToString2(exportRow["matchname"]);

                if (matchId != currentMatchId || matchName != currentMatchName)
                {
                    currentRowIndex = AssignMatchData(sheet, templateSheet, matchInfoList, formatMatchName,
                        currentRowIndex,
                        currentIndex);
                    currentIndex += CheckGreaterZero(matchInfoList.Count);
                    matchId = currentMatchId;
                    matchName = currentMatchName;
                    formatMatchName = GetFormatMatchName(exportRow, matchName);
                    matchInfoList = new List<DataRow>();
                }

                matchInfoList.Add(exportRow);
            }

            currentRowIndex = AssignMatchData(sheet, templateSheet, matchInfoList, formatMatchName, currentRowIndex,
                currentIndex);

            _contentTable.DefaultView.RowFilter =
                "bettype <> '9' and bettype <> '10' and bettype <> '29' and bettype <> '2799' and (bettype < '3601' or '3610' < bettype)";
            var sumTable = _contentTable.DefaultView.ToTable();

            if (_contentTable.Rows.Count > sumTable.Rows.Count)
            {
                currentRowIndex = AssignTotalSummary(sheet, templateSheet, columnNameList, currentRowIndex, sumTable);
            }

            currentRowIndex = AssignSpecialBetType(sheet, templateSheet, specialBetType, currentRowIndex);
            AssignGrandTotalSummary(sheet, templateSheet, currentRowIndex);
        }

        protected virtual void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var reportTitle = string.Format(
                                  ViewResources.MatchWinLossTitle,
                                  _model.Date.ToString(FormatDate),
                                  ExcelHelper.GetTitleCurrencyName(_option)) +
                              ExcelHelper.GetLicenseeWlProduct(_model.ProductOptions);

            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount,
                reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowMatchTemplate);

            // Remove content below header before insert new data
            var lastRowNum = sheet.LastRowNum;

            for (var i = RowContentTemplate; i <= lastRowNum; i++)
            {
                sheet.RemoveRow(sheet.GetRow(i));
            }
        }

        protected virtual void ProcessUi(ISheet sheet)
        {
            if (!_option.CustomerCommVisible)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);
                _builder.HideRow(sheet, RowTableHeaderTemplate + 1);
            }

            if (!_option.BaVisible)
            {
                _builder.HideColumn(sheet, ColumnBaCommission, ColumnAmtDueToBa);
            }
        }

        private static int AssignDoubleData(IRow row, int cellIndex, params double[] doubleValues)
        {
            for (int i = 0, j = doubleValues.Length; i < j && cellIndex < row.Cells.Count; i++)
            {
                row.Cells[cellIndex].SetCellValue(Math.Round(doubleValues[i]));
                cellIndex++;
            }

            return cellIndex;
        }

        private string GetFormatMatchName(DataRow exportRow, string matchName)
        {
            string formatMatchName;

            switch ((int)exportRow["newproductid"])
            {
                case ProductTypes.NumberGame:
                case ProductTypes.VirtualSports:
                    formatMatchName = matchName;
                    break;

                default:
                    var homeName = _cacheRefAdapter.GetTeamName(exportRow["homeid"]) +
                                   (Converter.ToBool(exportRow["isneutral"]) ? " " + "(N)" : string.Empty);
                    var awayName = _cacheRefAdapter.GetTeamName(exportRow["awayid"]);
                    formatMatchName = homeName + "\n" + awayName;
                    break;
            }

            return formatMatchName;
        }

        private void AddExtraColumns()
        {
            _contentTable.Columns.Add("totalwl", typeof(decimal), "winlost + playercomm");
            _totalTable.Columns.Add("sumtotalwl", typeof(decimal), "sumwinlost + sumplayercomm");
        }

        protected internal virtual void AssignGrandTotalSummary(ISheet sheet, ISheet templateSheet, int currentRow)
        {
            var cellIndex = 2;

            var row = _builder.CopyRow(sheet, templateSheet, RowGrandTotalTemplate, currentRow);

            row.Cells[0].SetCellValue(ViewResources.GrandTotal);
            _builder.MergeCols(sheet, 0, 1, currentRow);

            var columnNameList = GetTotalColumnNameList();

            foreach (var columnName in columnNameList)
            {
                cellIndex = AssignDoubleData(row, cellIndex, Convert.ToDouble(_totalTable.Rows[0][columnName]));
            }

            _builder.MergeCols(sheet, ColumnIndex, 1, currentRow);
        }

        protected internal virtual int AssignMatchData(ISheet sheet, ISheet templateSheet, IList<DataRow> matchInfoList,
                                                       string matchName,
                                                       int currentRow, int currentIndex)
        {
            if (matchInfoList.Count == 0)
            {
                return currentRow;
            }

            var rowStartIndex = currentRow;
            var columnNameList = CommonColumnNameList;
            var rowCount = matchInfoList.Count;

            // Assign match data
            var matchRow = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, currentRow);
            matchRow.Cells[ColumnIndex].SetCellValue(currentIndex);
            matchRow.Cells[ColumnMatch].SetCellValue(matchName);

            // Calculate match win loss
            for (var i = 0; i < columnNameList.Count; i++)
            {
                var columnName = columnNameList[i];
                double columnValue = 0;

                for (var j = 0; j < rowCount; j++)
                {
                    var matchWinloss = matchInfoList[j];
                    columnValue += Converter.To<double>(matchWinloss[columnName]);
                }

                matchRow.Cells[i + ColumnNumbericDataStartIndex].SetCellValue(columnValue);
            }

            currentRow++;

            if (matchName.Contains('\n'))
            {
                const short MatchRowLine = 2;
                matchRow.Height = (short)(matchRow.Height * MatchRowLine);
            }

            // Assign bettype
            for (var i = 0; i < rowCount; i++)
            {
                var matchWinloss = matchInfoList[i];
                var betTypeRow = _builder.CopyRow(sheet, templateSheet, RowBetTypeContentTemplate, currentRow);
                var betType = Converter.ToInt(matchWinloss[BetTypeColumn]);
                var liveIndicator = Converter.To<bool>(matchWinloss["liveindicator"]);
                var betTypeName = "         " + _cacheRefAdapter.GetBettype(betType, liveIndicator);

                betTypeRow.Cells[1].SetCellValue(betTypeName);
                AssignRowContent(betTypeRow, matchWinloss, columnNameList, ColumnNumbericDataStartIndex);
                currentRow++;
            }

            _builder.MergeCells(sheet, rowStartIndex, rowStartIndex + rowCount, 0, 0);

            return currentRow;
        }

        private int AssignSpecialBetType(ISheet sheet, ISheet templateSheet, IList<DataRow> exportData,
                                         int currentRowIndex)
        {
            var rowCount = exportData.Count;
            exportData = exportData.OrderBy(d => Converter.ToInt(d[BetTypeColumn])).ToList();

            // Assign bettype
            for (var i = 0; i < rowCount; i++)
            {
                var dataRow = exportData[i];
                var betTypeRow = _builder.CopyRow(sheet, templateSheet, RowSpecialBetTypeTemplate, currentRowIndex);
                var betType = Converter.ToInt(dataRow[BetTypeColumn]);
                var productCode = Converter.ToString2(dataRow["ProductCode"]);

                var betTypeName = _cacheRefAdapter.GetBettypeWithProductName(betType, productCode);

                betTypeRow.Cells[0].SetCellValue(betTypeName);
                AssignRowContent(betTypeRow, dataRow, CommonColumnNameList, ColumnNumbericDataStartIndex);
                _builder.MergeCols(sheet, 0, 1, currentRowIndex);
                currentRowIndex++;
            }

            return currentRowIndex;
        }

        protected int AssignTotalSummary(ISheet sheet, ISheet templateSheet, IEnumerable<string> columnNameList,
                                         int currentRow, DataTable sumTable)
        {
            var cellIndex = 2;

            var row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, currentRow);

            row.Cells[0].SetCellValue(ViewResources.Total);
            _builder.MergeCols(sheet, 0, 1, currentRow);

            foreach (var columnName in columnNameList)
            {
                var sumData = sumTable.Compute("Sum(" + columnName + ")", string.Empty);
                cellIndex = AssignDoubleData(row, cellIndex, Convert.ToDouble(sumData));
            }

            _builder.MergeCols(sheet, ColumnIndex, 1, currentRow);

            return currentRow + 1;
        }

        private static IList<string> GetTotalColumnNameList() => CommonColumnNameList.Select(x => "sum" + x).ToList();

        private static bool IsHappy5LinkedGames(int betType)
            => BetType.BeginHappy5LinkedGames <= betType && betType <= BetType.EndHappy5LinkedGames;

        protected static void SetTextRowHeader(IRow row, IEnumerable<string> headerTextList)
        {
            for (var i = 0; i < headerTextList.Count(); i++)
            {
                row.Cells[i].SetCellValue(headerTextList.ElementAt(i));
            }
        }

        protected static int AssignRowContent(IRow row, DataRow data, IEnumerable<string> columnNameList, int columnStart = 0)
        {
            var tableColumns = data.Table.Columns;

            foreach (var columnName in columnNameList)
            {
                if (tableColumns.Contains(columnName))
                {
                    row.Cells[columnStart].SetCellValue(Convert.ToDouble(data[columnName]));
                }

                columnStart++;
            }

            return columnStart;
        }
    }
}
