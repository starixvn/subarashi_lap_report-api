namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;

    public interface ICustomerTurnoverService
    {
        Task<ReportResultModel<CustTurnover>> GetDataList(CustomerTurnoverMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string languageCode);
    }
}
