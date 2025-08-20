namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IGodOfFortuneWinLossExcel
    {
        byte[] Export(string template, SortableReportResultModel<GodOfFortuneWinLoss> data, GodOfFortuneWinLossMetaModel metaModel);
    }
}