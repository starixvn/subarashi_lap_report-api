using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IThirdPartyWinLossExcel<T> where T : ThirdPartyWinLoss
    {
        byte[] Export(string template, ReportResultModel<T> data, ThirdPartyWinLossMetaModel metaModel, ThirdPartyWinLossExcelOption option);
    }
}
