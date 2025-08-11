namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IExtraPayoutExcel
    {
        byte[] Export(string template, SortableReportResultModel<ExtraPayout> data, ExtraPayoutMetaModel metaModel);
    }
}