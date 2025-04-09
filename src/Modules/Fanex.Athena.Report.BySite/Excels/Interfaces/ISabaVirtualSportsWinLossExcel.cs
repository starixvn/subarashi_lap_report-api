using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface ISabaVirtualSportsWinLossExcel
    {
        byte[] Export(string template, SortableReportResultModel<SabaVirtualSportsWinLossDetail> data, SabaVirtualSportsWinLossMetaModel metaModel, SabaVirtualSportsWinLossExcelOption option);
    }
}
