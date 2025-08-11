namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs.SubsidiaryPerformance;
    using MetaModels;
    using Models;
    using Options;

    public interface ISubsidiaryPerformanceSummaryExcel
    {
        string Export(string template, SubsidiaryPerformanceResultModel data, SubsidiaryPerformanceMetaModel metaModel, SubsidiaryPerformanceExcelOption option);
    }

    public interface ISubsidiaryPerformanceDetailExcel
    {
        string Export(string template, ReportResultModel<SubsidiaryPerformanceDetail> data, SubsidiaryPerformanceMetaModel metaModel);
    }
}
