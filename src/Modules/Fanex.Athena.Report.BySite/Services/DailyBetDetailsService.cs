namespace Fanex.Athena.Report.BySite.Services
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DailyBetDetailsService : IDailyBetDetailsService
    {
        private readonly IDailyBetDetailsRepository _repository;

        public DailyBetDetailsService(IDailyBetDetailsRepository dailyBetsRepository)
        {
            _repository = dailyBetsRepository;
        }

        public Task<ResultModel<DailyBetDetails>> GetDataAsync(DailyBetDetailsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            return _repository.GetDataAsync(metaModel, auditCurrencyList);
        }
    }
}