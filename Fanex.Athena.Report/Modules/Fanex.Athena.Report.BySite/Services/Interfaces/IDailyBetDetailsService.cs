namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface IDailyBetDetailsService
    {
        Task<ResultModel<DailyBetDetails>> GetDataAsync(DailyBetDetailsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList);
    }
}