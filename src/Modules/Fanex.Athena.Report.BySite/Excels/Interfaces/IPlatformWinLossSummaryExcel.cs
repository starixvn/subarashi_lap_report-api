namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IPlatformWinLossSummaryExcel
    {
        byte[] Export(string template, ResultModel<PlatformWinLossSummary> data, PlatformWinLossMetaModel metaModel);
    }
}