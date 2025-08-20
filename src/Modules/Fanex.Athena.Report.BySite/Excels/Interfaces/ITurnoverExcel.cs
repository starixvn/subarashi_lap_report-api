namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using MetaModels;
    using Models;
    using Options;

    public interface ITurnoverExcel
    {
        byte[] Export(string template, TurnoverResultModel data, TurnoverMetaModel metaModel, TurnoverExcelOption excelOption);
    }
}
