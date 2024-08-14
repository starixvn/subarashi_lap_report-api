using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IAgCasinoBalanceExcel
    {
        byte[] Export(string template, SortableReportResultModel<AgCasinoBalance> data, ThirdPartyWinLossMetaModel metaModel, ThirdPartyWinLossExcelOption option);
    }
}
