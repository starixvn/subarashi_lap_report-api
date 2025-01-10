namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDailyBetsService
    {
        Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList);
    }
}