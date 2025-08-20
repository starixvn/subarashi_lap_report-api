namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs.SubsidiaryPerformance;
    using MetaModels;
    using Models;

    public interface ISubsidiaryPerformanceService
    {
        Task<SubsidiaryPerformanceResultModel> GetData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<ReportResultModel<SubsidiaryPerformanceDetail>> GetDetailData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}
