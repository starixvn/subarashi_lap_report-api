namespace Fanex.Athena.Report.ByPlayer.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTO;
    using Interfaces;
    using MetaModels;
    using Repository.Interfaces;

    public class CustomerTurnoverService : ICustomerTurnoverService
    {
        private readonly ICustomerTurnoverRepository _customerTurnoverRepository;

        public CustomerTurnoverService(ICustomerTurnoverRepository customerTurnoverRepository)
        {
            _customerTurnoverRepository = customerTurnoverRepository;
        }

        public async Task<ReportResultModel<CustTurnover>> GetDataList(CustomerTurnoverMetaModel metaModel,
                                                                       IReadOnlyDictionary<int, string> currencyList,
                                                                       string languageCode)
        {
            var result = await _customerTurnoverRepository.GetDataList(metaModel, currencyList, languageCode).ConfigureAwait(false);

            if (result.DataList.Any())
            {
                result.DataList = RestructureData(result.DataList);
            }

            return result;
        }

        private static IEnumerable<CustTurnover> RestructureData(IEnumerable<CustTurnover> datalist)
        {
            var sortedData = datalist.OrderBy(x => x.Index)
                                     .ThenByDescending(i => i.ReportBy)
                                     .ThenBy(i => i.ExternalId)
                                     .ThenBy(i => i.SportType)
                                     .ThenBy(i => i.Bettype).ToList();

            var newDataList = new List<CustTurnover>();

            var groupData = sortedData.GroupBy(i => i.Index);

            foreach (var item in groupData)
            {
                var isFirstRowOfGroup = true;
                var numOfItem = item.Count();
                var groupSportdata = item.GroupBy(i => i.SportType);
                var totalTurnover = item.Sum(i => i.Turnover);
                foreach (var sports in groupSportdata)
                {
                    var groupedList = ProcessSportGroup(sports, isFirstRowOfGroup, numOfItem, totalTurnover);
                    isFirstRowOfGroup = false;
                    newDataList.AddRange(groupedList);
                }
            }

            return newDataList;
        }

        private static IEnumerable<CustTurnover> ProcessSportGroup(IGrouping<string, CustTurnover> sportGroup,
                                                                   bool isFirstRowOfGroup,
                                                                   int numOfItem,
                                                                   decimal totalTurnover)
        {
            var isFirstRowOfSportGroup = true;
            var numOfSportItem = sportGroup.Count();
            var list = new List<CustTurnover>();
            var hasRowSpan = isFirstRowOfGroup;

            foreach (var item in sportGroup)
            {
                var temp = item;
                temp.TotalTurnover = totalTurnover;

                temp.RowSpan = GetRowSpan(hasRowSpan, numOfItem);
                hasRowSpan = false;

                temp.RowSpan2 = GetRowSpan(isFirstRowOfSportGroup, numOfSportItem);
                isFirstRowOfSportGroup = false;

                list.Add(temp);
            }

            return list;
        }

        private static int GetRowSpan(bool hasRowSpan, int numOfItems)
        {
            if (hasRowSpan && numOfItems > 1)
            {
                return numOfItems;
            }

            if (numOfItems > 1)
            {
                return -1;
            }

            return 0;
        }
    }
}
