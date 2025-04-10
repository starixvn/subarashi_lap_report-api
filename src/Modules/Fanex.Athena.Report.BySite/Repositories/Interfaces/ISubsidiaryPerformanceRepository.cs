using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface ISubsidiaryPerformanceRepository
    {
        Task<IEnumerable<SubsidiaryPerformance>> GetData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<ReportResultModel<SubsidiaryPerformanceDetail>> GetDetailData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}
