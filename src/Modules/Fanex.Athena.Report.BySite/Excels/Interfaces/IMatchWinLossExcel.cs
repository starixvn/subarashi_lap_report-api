namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using System.Data;
    using MetaModels;
    using Options;

    public interface IMatchWinLossExcel
    {
        byte[] Export(string template, DataSet data, MatchWinLossMetaModel metaModel,
                      MatchWinLossExcelOption excelOption);
    }
}
