namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.DTO;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IDailyBetDetailsRepository
    {
        Task<ResultModel<DailyBetDetails>> GetDataAsync(DailyBetDetailsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList);
    }
}