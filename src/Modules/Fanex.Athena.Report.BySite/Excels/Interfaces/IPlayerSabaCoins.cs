namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IPlayerSabaCoinsExcel
    {
        byte[] Export(string template, SortableReportResultModel<PlayerSabaCoins> data, PlayerSabaCoinsMetaModel metaModel);
    }
}