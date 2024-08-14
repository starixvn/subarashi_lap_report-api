namespace Fanex.Athena.Report.BySite.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Repositories.Interfaces;
    using Resources.Constants;

    public class AgCasinoBalanceService : IAgCasinoBalanceService
    {
        private readonly IAgCasinoBalanceRepository _agCasinoBalanceRepository;

        public AgCasinoBalanceService(IAgCasinoBalanceRepository agCasinoBalanceRepository)
        {
            _agCasinoBalanceRepository = agCasinoBalanceRepository;
        }

        public async Task<SortableReportResultModel<AgCasinoBalance>> GetData(
            ThirdPartyWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            switch (metaModel.DrillDownLevel)
            {
                case (int)ReportLevel.Licensee:
                {
                    return await _agCasinoBalanceRepository.GetAgentBalanceDetail(metaModel, currencyList)
                        .ConfigureAwait(false);
                }
                case (int)ReportLevel.Customer:
                {
                    return await _agCasinoBalanceRepository.GetCustomerBalanceDetail(metaModel, currencyList)
                        .ConfigureAwait(false);
                }
                default:
                {
                    var result = await _agCasinoBalanceRepository.GetSummaryBalance(metaModel, currencyList)
                        .ConfigureAwait(false);
                    AssignRowSpan(result.DataList);
                    return result;
                }
            }
        }


        private static void AssignRowSpan(IEnumerable<AgCasinoBalance> dataList)
        {
            var date = DateTimeHelper.CreateDateTime();
            var index = 1;

            foreach (var item in dataList)
            {
                if (date != item.Date)
                {
                    date = item.Date ?? DateTimeHelper.CreateDateTime();
                    item.RowSpan = dataList.Count(x => x.Date == item.Date);
                    item.Index = index++;
                }
                else
                {
                    item.RowSpan = -1;
                }
            }
        }
    }
}