using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using System.Data;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IMatchWinLossExcel
    {
        byte[] Export(string template, DataSet data, MatchWinLossMetaModel metaModel,
                      MatchWinLossExcelOption excelOption);
    }
}
