using Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface ISubsidiaryPerformanceSummaryExcel
    {
        string Export(string template, SubsidiaryPerformanceResultModel data, SubsidiaryPerformanceMetaModel metaModel, SubsidiaryPerformanceExcelOption option);
    }

    public interface ISubsidiaryPerformanceDetailExcel
    {
        string Export(string template, ReportResultModel<SubsidiaryPerformanceDetail> data, SubsidiaryPerformanceMetaModel metaModel);
    }
}
