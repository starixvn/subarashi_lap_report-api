using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IPlatformWinLossSummaryExcel
    {
        IWorkbook Export(string template, ResultModel<PlatformWinLossSummary> data, PlatformWinLossMetaModel metaModel);
    }
}
