namespace Fanex.Athena.Report.BySite.Excels
{
    using System.IO;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
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
            var row = CopyRow(sheet, sourceSheet, sourceRowIndex, destRowIndex);

            MergeCells(sheet, 0, lastRow: destRowIndex, firstCol: destRowIndex, lastCol: columnCount - 1);
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

                    CopyCell(sheet, oldCell, newCell);
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

        private static int MergeCells(ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            var rangeAddress = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
            return sheet.AddMergedRegion(rangeAddress);
        }

        private static ICell CopyCell(ISheet sheet, ICell sourceCell, ICell destCell)
        {
            if (sourceCell == null)
            {
                return null;
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

            return destCell;
        }

        private static void DeleteRow(ISheet sheet, int rowIndex)
        {
            sheet.RemoveRow(sheet.GetRow(rowIndex));
        }
    }
}
