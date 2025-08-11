namespace Fanex.Athena.Report.ByPlayer.Repository.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;

    public interface ICustomerTurnoverRepository
    {
        Task<ReportResultModel<CustTurnover>> GetDataList(CustomerTurnoverMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string languageCode);
    }
}
