using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface ITurnoverExcel
    {
        byte[] Export(string template, TurnoverResultModel data, TurnoverMetaModel metaModel, TurnoverExcelOption excelOption);
    }
}
