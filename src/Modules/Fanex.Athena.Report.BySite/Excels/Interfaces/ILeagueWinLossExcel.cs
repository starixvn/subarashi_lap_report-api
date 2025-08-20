namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface ILeagueWinLossExcel
    {
        byte[] Export(string template, SortableReportResultModel<LeagueWinLoss> data, LeagueWinLossMetaModel metaModel);
    }
}