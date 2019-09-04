namespace Fanex.Athena.Report.Common.Excel
{
    using NPOI.SS.UserModel;

    public interface IExcelBuilder
    {
        IWorkbook LoadTemplate(string path);

        ISheet CloneSheet(IWorkbook workbook, string sourceSheetName, string newSheetName);

        void RemoveSheet(IWorkbook workbook, string sheetName);

        IRow CopyRow(ISheet sheet, ISheet sourceSheet, int sourceRowIndex, int destRowIndex);

        void SetRowValue(ISheet sheet, ISheet sourceSheet, int sourceRowIndex, int destRowIndex, int columnCount, string value);

        void ClearTemplate(ISheet sheet, int startRowIndex);
    }
}
