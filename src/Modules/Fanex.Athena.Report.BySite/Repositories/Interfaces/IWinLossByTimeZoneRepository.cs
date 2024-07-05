namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IWinLossByTimeZoneRepository
    {
        Task<ReportResultModel<WinLossByTimeZone>> GetData(WinLossByTimeZoneMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}