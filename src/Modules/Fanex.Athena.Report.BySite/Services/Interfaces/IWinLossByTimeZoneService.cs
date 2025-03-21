namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IWinLossByTimeZoneService
    {
        Task<ReportResultModel<WinLossByTimeZone>> GetData(WinLossByTimeZoneMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}