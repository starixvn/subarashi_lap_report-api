namespace Fanex.Athena.Report.BySite.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Repositories.Interfaces;

    public class WinLossByTimeZoneService : IWinLossByTimeZoneService
    {
        private readonly IWinLossByTimeZoneRepository _winLossByTimeZoneRepository;

        public WinLossByTimeZoneService(IWinLossByTimeZoneRepository winLossByTimeZoneRepository)
        {
            _winLossByTimeZoneRepository = winLossByTimeZoneRepository;
        }

        public Task<ReportResultModel<WinLossByTimeZone>> GetData(
            WinLossByTimeZoneMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            return _winLossByTimeZoneRepository.GetData(metaModel, currencyList);
        }
    }
}