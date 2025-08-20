namespace Fanex.Athena.Report.BySite.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.DTO;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Repositories.Interfaces;

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