namespace Fanex.Athena.Report.Common.Excel
{
    using System.IO;
    using Interfaces;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;

    public class ExcelBuilder : IExcelBuilder
    {
        public const string TemplateSheetName = "Template";

        public IWorkbook LoadTemplate(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var workbook = new HSSFWorkbook(stream);
                AssignDefaultStyle(workbook);

                return workbook;
            }
        }

        public IWorkbook CreateWorkbook()
        {
            var workbook = new HSSFWorkbook();
            AssignDefaultStyle(workbook);
            return workbook;
        }

        private static void AssignDefaultStyle(IWorkbook workbook)
        {
            var defaultStyle = workbook.CreateCellStyle();
            defaultStyle.ShrinkToFit = true;
            defaultStyle.WrapText = true;
        }

        public ISheet CloneSheet(IWorkbook workbook, string sourceSheetName, string newSheetName)
        {
            var sheetIndex = workbook.GetSheetIndex(sourceSheetName);
            var newSheet = workbook.CloneSheet(sheetIndex);
            var newSheetIndex = workbook.GetSheetIndex(newSheet);
            workbook.SetSheetName(newSheetIndex, newSheetName);
            return newSheet;
        }

        public void RemoveSheet(IWorkbook workbook, string sheetName)
        {
            var sheet = workbook.GetSheet(sheetName);

            if (sheet != null)
            {
                var sheetIndex = workbook.GetSheetIndex(sheet);
                workbook.RemoveSheetAt(sheetIndex);
            }
        }

        public void SetRowValue(
           ISheet sheet,
           ISheet sourceSheet,
           int sourceRowIndex,
           int destRowIndex,
           int columnCount,
           string value)
        {
            var row = CopyRow(sheet, sourceSheet, sourceRowIndex, destRowIndex);
            var destColIndex = columnCount - 1;

            if (destColIndex > 0)
            {
                MergeCells(sheet, firstRow: destRowIndex, lastRow: destRowIndex, firstCol: 0, lastCol: destColIndex);
            }

            row.Cells[0].SetCellValue(value);
        }

        public IRow CopyRow(ISheet sheet, ISheet sourceSheet, int sourceRowIndex, int destRowIndex)
        {
            if (sheet.LastRowNum < destRowIndex)
            {
                for (var i = sheet.LastRowNum + 1; i <= destRowIndex; i++)
                {
                    sheet.CreateRow(i);
                }
            }

            // Get the source / new row
            var newRow = sheet.GetRow(destRowIndex);
            newRow ??= sheet.CreateRow(destRowIndex);

            var sourceRow = sourceSheet.GetRow(sourceRowIndex);
            if (sourceRow != null)
            {
                // Loop through source columns to add to new row
                for (int i = 0; i < sourceRow.LastCellNum; i++)
                {
                    // Grab a copy of the old/new cell
                    var oldCell = sourceRow.GetCell(i);
                    var newCell = newRow.CreateCell(i);

                    CopyCell(oldCell, newCell);
                }
            }

            return newRow;
        }

        public void ClearTemplate(ISheet sheet, int startRowIndex)
        {
            int lastRowNum = sheet.LastRowNum;
            for (int i = startRowIndex; i <= lastRowNum; i++)
            {
                DeleteRow(sheet, i);
            }
        }

        public int MergeCells(ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            var rangeAddress = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
            if (CanMergeRegion(sheet, rangeAddress))
            {
                return sheet.AddMergedRegion(rangeAddress);
            }
            return -1;
        }

        public int MergeRows(ISheet sheet, int firstRow, int lastRow, int colIndex)
        {
            var rangeAddress = new CellRangeAddress(firstRow, lastRow, colIndex, colIndex);
            if (CanMergeRegion(sheet, rangeAddress))
            {
                return sheet.AddMergedRegion(rangeAddress);
            }
            return -1;
        }

        public int MergeCols(ISheet sheet, int firstCol, int lastCol, int rowIndex)
        {
            var rangeAddress = new CellRangeAddress(rowIndex, rowIndex, firstCol, lastCol);
            if (CanMergeRegion(sheet, rangeAddress))
            {
                return sheet.AddMergedRegion(rangeAddress);
            }
            return -1;
        }

        private static bool CanMergeRegion(ISheet sheet, CellRangeAddress candidateRegion)
        {
            if (candidateRegion.FirstRow == candidateRegion.LastRow &&
                candidateRegion.FirstColumn == candidateRegion.LastColumn)
            {
                return false;
            }

            for (int i = 0; i < sheet.NumMergedRegions; i++)
            {
                var existingRegion = sheet.GetMergedRegion(i);
                if (existingRegion.Intersects(candidateRegion))
                {
                    return false;
                }
            }
            return true;
        }

        public void HideRow(ISheet sheet, params int[] rowIndex)
        {
            const int NumberOfColumn = 255;

            // Clear content
            foreach (int i in rowIndex)
            {
                for (int j = 0; j < NumberOfColumn; j++)
                {
                    var cell = GetCell(sheet, i, j);
                    if (cell != null)
                    {
                        cell.SetCellValue(string.Empty);
                    }
                }
            }

            // Hide row
            foreach (int i in rowIndex)
            {
                var row = sheet.GetRow(i);
                if (row != null)
                {
                    row.ZeroHeight = true;
                }
            }
        }

        public void HideColumn(ISheet sheet, params int[] columnIndex)
        {
            // Clear content
            var firstRowNum = sheet.FirstRowNum;
            var lastRowNum = sheet.LastRowNum;

            for (int i = firstRowNum; i <= lastRowNum; i++)
            {
                foreach (int j in columnIndex)
                {
                    var cell = GetCell(sheet, i, j);
                    if (cell != null)
                    {
                        cell.SetCellValue(string.Empty);
                    }
                }
            }

            // Hide column
            foreach (int i in columnIndex)
            {
                sheet.SetColumnWidth(i, 0);
                sheet.SetColumnHidden(i, true);
            }
        }

        private static ICell GetCell(ISheet sheet, int rowIndex, int colIndex)
        {
            return sheet.GetRow(rowIndex).GetCell(colIndex);
        }

        private static void CopyCell(ICell sourceCell, ICell destCell)
        {
            if (sourceCell == null)
            {
                return;
            }

            destCell.CellStyle = sourceCell.CellStyle;

            if (destCell.CellComment != null)
            {
                destCell.CellComment = sourceCell.CellComment;
            }

            if (sourceCell.Hyperlink != null)
            {
                destCell.Hyperlink = sourceCell.Hyperlink;
            }

            destCell.SetCellType(sourceCell.CellType);

            switch (sourceCell.CellType)
            {
                case CellType.Blank:
                    destCell.SetCellValue(sourceCell.StringCellValue);
                    break;

                case CellType.Boolean:
                    destCell.SetCellValue(sourceCell.BooleanCellValue);
                    break;

                case CellType.Error:
                    destCell.SetCellErrorValue(sourceCell.ErrorCellValue);
                    break;

                case CellType.Numeric:
                    destCell.SetCellValue(sourceCell.NumericCellValue);
                    break;

                case CellType.String:
                    destCell.SetCellValue(sourceCell.RichStringCellValue);
                    break;

                default:
                    destCell.SetCellValue(sourceCell.StringCellValue);
                    break;
            }
        }

        private static void DeleteRow(ISheet sheet, int rowIndex)
        {
            sheet.RemoveRow(sheet.GetRow(rowIndex));
        }
    }
}