namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IExtraPayoutDetailExcel
    {
        byte[] Export(string template, SortableReportResultModel<ExtraPayoutDetail> data, ExtraPayoutDetailMetaModel metaModel);
    }
}