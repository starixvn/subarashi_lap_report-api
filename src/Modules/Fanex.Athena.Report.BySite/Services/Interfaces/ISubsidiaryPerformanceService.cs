using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface ISubsidiaryPerformanceService
    {
        Task<SubsidiaryPerformanceResultModel> GetData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<ReportResultModel<SubsidiaryPerformanceDetail>> GetDetailData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}
