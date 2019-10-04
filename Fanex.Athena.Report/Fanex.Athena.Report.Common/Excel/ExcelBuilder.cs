namespace Fanex.Athena.Report.Common.Excel
{
    using System.IO;
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
                var defaultStyle = workbook.CreateCellStyle();
                defaultStyle.ShrinkToFit = true;
                defaultStyle.WrapText = true;

                return workbook;
            }
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
            var row = this.CopyRow(sheet, sourceSheet, sourceRowIndex, destRowIndex);

            this.MergeCells(sheet, firstRow: destRowIndex, lastRow: destRowIndex, firstCol: 0, lastCol: columnCount - 1);
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
            if (newRow == null)
            {
                newRow = sheet.CreateRow(destRowIndex);
            }

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
            return sheet.AddMergedRegion(rangeAddress);
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