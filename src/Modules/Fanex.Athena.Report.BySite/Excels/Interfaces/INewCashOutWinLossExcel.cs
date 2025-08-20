namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface INewCashOutWinLossExcel
    {
        byte[] Export(string template, SortableReportResultModel<CashOutWinLoss> data, NewCashOutWinLossMetaModel metaModel);
    }
}