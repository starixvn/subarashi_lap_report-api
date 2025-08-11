namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.DTO;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IDailyBetDetailsService
    {
        Task<ResultModel<DailyBetDetails>> GetDataAsync(DailyBetDetailsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList);
    }
}