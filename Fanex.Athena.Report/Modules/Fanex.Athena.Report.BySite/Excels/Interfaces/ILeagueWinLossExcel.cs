namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface ILeagueWinLossExcel
    {
        byte[] Export(string template, SortableReportResultModel<LeagueWinLoss> data, LeagueWinLossMetaModel metaModel);
    }
}