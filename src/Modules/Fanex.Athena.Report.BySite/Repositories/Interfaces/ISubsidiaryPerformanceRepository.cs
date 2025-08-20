namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs.SubsidiaryPerformance;
    using MetaModels;

    public interface ISubsidiaryPerformanceRepository
    {
        Task<IEnumerable<SubsidiaryPerformance>> GetData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<ReportResultModel<SubsidiaryPerformanceDetail>> GetDetailData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}
