namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface IPlatformWinLossSummaryExcel
    {
        byte[] Export(string template, ResultModel<PlatformWinLossSummary> data, PlatformWinLossMetaModel metaModel);
    }
}