using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.UnitTests.Common;
using Fanex.Common;
using Fanex.Common.Extensions;
using Microsoft.AspNetCore.Routing;
using Moq;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class MatchWinLossExcelTest
    {
        public const int RowHeaderTemplate = 1;
        public const int RowTitleTemplate = 0;
        public const int MaxRowCount = 30000;

        private const int ColumnCustomerWl = 4;
        private const int ColumnCustomerComm = 5;
        private const int ColumnCustomerTotal = 6;
        private const int ColumnBaCommission = 8;
        private const int ColumnAmtDueToBa = 9;
        private const int ColumnTemplateCount = 10;

        private const int RowMatchTemplate = 3;

        private readonly Mock<ISheet> _sheet;

        private readonly Mock<ISheet> _templateSheet;

        private readonly Mock<ICacheRefAdapter> _cache;

        private readonly Mock<IExcelBuilder> _builder;

        private readonly DataTable _contentData;

        private readonly DataTable _totalData;

        public MatchWinLossExcelTest()
        {
            _sheet = new Mock<ISheet>();
            _templateSheet = new Mock<ISheet>();
            _cache = new Mock<ICacheRefAdapter>();
            _builder = new Mock<IExcelBuilder>();

            _contentData = new DataTable();
            _totalData = new DataTable();
            _contentData.Columns.Add("bettype", typeof(int));
            _contentData.Columns.Add("liveindicator", typeof(int));
            _contentData.Columns.Add("turnover", typeof(decimal));
            _contentData.Columns.Add("betcount", typeof(int));
            _contentData.Columns.Add("winlost", typeof(decimal));
            _contentData.Columns.Add("licenseewl", typeof(decimal));
            _contentData.Columns.Add("commba", typeof(decimal));
            _contentData.Columns.Add("amtduetoba", typeof(decimal));
            _contentData.Columns.Add("amtduetorb", typeof(decimal));
            _contentData.Columns.Add("commrb", typeof(decimal));
            _contentData.Columns.Add("winlossrb", typeof(decimal));
            _contentData.Columns.Add("playercomm", typeof(decimal));
            _contentData.Columns.Add("matchcode", typeof(string));

            _totalData.Columns.Add("sumturnover", typeof(decimal));
            _totalData.Columns.Add("sumbetcount", typeof(int));
            _totalData.Columns.Add("sumwinlost", typeof(decimal));
            _totalData.Columns.Add("sumlicenseewl", typeof(decimal));
            _totalData.Columns.Add("sumcommba", typeof(decimal));
            _totalData.Columns.Add("sumamtduetoba", typeof(decimal));
            _totalData.Columns.Add("sumamtduetorb", typeof(decimal));

            _totalData.Columns.Add("sumcommrb", typeof(decimal));
            _totalData.Columns.Add("sumwinlossrb", typeof(decimal));
            _totalData.Columns.Add("sumplayercomm", typeof(decimal));

            _contentData.Rows.Add(100, 1, 3, 4, 5, 6, 7, 8, 9, 10);
            _totalData.Rows.Add(11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
        }



        [Fact]
        public void AssignTotalSummary_WhenCall_CallRelevantMethods()
        {
            // Arrange
            const int CurrentRow = 10;
            var row = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, CurrentRow);
            var excel = CreateExcel();
            var columns = new[] { "column1" };
            var sumTable = new DataTable();
            sumTable.Columns.Add("column1", typeof(decimal));
            sumTable.Rows.Add(1);
            sumTable.Rows.Add(2);

            // Action
            excel.Public_AssignTotalSummary(_sheet.Object, _templateSheet.Object, columns, CurrentRow, sumTable);

            // Assert
            row.Verify(x => x.Cells[2].SetCellValue(3));
        }
        private MatchWinLossExcelTestable CreateExcel()
        {
            var workBook = new Mock<IWorkbook>();
            workBook.Setup(x => x.GetSheet(It.IsAny<string>())).Returns(_sheet.Object);

            _builder.Setup(x => x.LoadTemplate(It.IsAny<string>())).Returns(workBook.Object);
            var excel = new MatchWinLossExcelTestable(_cache.Object, _builder.Object);
            return excel;
        }

        #region IDisposable Support

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _contentData.Dispose();
            _totalData.Dispose();
        }

        #endregion IDisposable Support

        protected static void AssertSetTextRowHeader(Mock<IRow> row, IEnumerable<string> headers, int startIndex = 0)
        {
            for (int i = 0; i < headers.Count(); i++)
            {
                row.Verify(x => x.Cells[i + startIndex], Times.Once);
                Assert.Equal(headers.ElementAt(i), row.Object.Cells[i + startIndex].StringCellValue);
            }
        }

        protected static void AssertHideColumns(Mock<ISheet> sheet, params int[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                sheet.Verify(x => x.SetColumnWidth(columns[i], 0));
                sheet.Verify(x => x.SetColumnHidden(columns[i], true));
            }
        }

        public class MatchWinLossExcelTestable : MatchWinLossExcel
        {
            public MatchWinLossExcelTestable(ICacheRefAdapter cacheRefAdapter, IExcelBuilder builder) : base(cacheRefAdapter, builder) { }

            public void Public_AssignHeader(ISheet sheet, ISheet templateSheet)
            {
                AssignHeader(sheet, templateSheet);
            }

            public void Public_AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
            {
                AssignNoInfoRow(sheet, templateSheet);
            }
            
            public void Public_AssignTotalSummary(ISheet sheet, ISheet templateSheet, IEnumerable<string> columnNameList, int currentRow, DataTable sumTable)
            {
                AssignTotalSummary(sheet, templateSheet, columnNameList, currentRow, sumTable);
            }

            public void Public_AssignSheetData(ISheet sheet, ISheet templateSheet)
            {
                AssignSheetData(sheet, templateSheet);
            }

            public void Public_AssignTitle(ISheet sheet, ISheet templateSheet)
            {
                AssignTitle(sheet, templateSheet);
            }

            public void Public_ProcessUI(ISheet sheet, ISheet templateSheet)
            {
                ProcessUi(sheet);
            }

        }
    }
}
