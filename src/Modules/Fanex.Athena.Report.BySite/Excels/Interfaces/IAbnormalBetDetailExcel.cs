namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IAbnormalBetDetailExcel
    {
        byte[] Export(string template, SortableReportResultModel<AbnormalBetDetail> data, AbnormalBetMetaModel metaModel);
    }
}