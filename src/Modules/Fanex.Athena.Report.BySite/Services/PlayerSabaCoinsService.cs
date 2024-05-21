namespace Fanex.Athena.Report.BySite.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using NPOI.HSSF.Record.Chart;

    public class PlayerSabaCoinsService : IPlayerSabaCoinsService
    {
        private readonly IPlayerSabaCoinsRepository _repository;

        public PlayerSabaCoinsService(IPlayerSabaCoinsRepository repository)
        {
            _repository = repository;
        }

        public async Task<SortableReportResultModel<PlayerSabaCoins>> GetDataAsync(PlayerSabaCoinsMetaModel metaModel)
        {
            var (mainData, extraInfoData) = await GetDataFromDBAsync(metaModel).ConfigureAwait(false);
            var dataList = JoinData(mainData, extraInfoData).ToList();
            var sortedDataList = PaginateAndSortData(new SortableList<PlayerSabaCoins>(dataList), metaModel);

            return new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = sortedDataList,
                TotalData = SumTotalData(dataList),
                TotalRows = dataList.Count,
            };
        }

        private async Task<(IEnumerable<PlayerSabaCoins>, IEnumerable<PlayerSabaCoins>)>
            GetDataFromDBAsync(PlayerSabaCoinsMetaModel metaModel)
        {
            var getMainData = _repository.GetMainData(metaModel);
            var getExtraInfoData = _repository.GetExtraInfoData(metaModel);
            await Task.WhenAll(getMainData, getExtraInfoData).ConfigureAwait(false);

            return (await getMainData, await getExtraInfoData);
        }

        private static IEnumerable<PlayerSabaCoins> JoinData(IEnumerable<PlayerSabaCoins> main, IEnumerable<PlayerSabaCoins> extraInfo)
        {
            if (main != null && main.Any())
            {
                if (extraInfo != null && extraInfo.Any())
                {
                    return main.Join(extraInfo,
                          m => m.CustId,
                          e => e.CustId,
                          (main, extra) => new PlayerSabaCoins
                          {
                              CustId = main.CustId,
                              Username = main.Username,
                              ExternalId = main.ExternalId,
                              OpeningBalance = main.OpeningBalance,
                              Turnover = main.Turnover,
                              Outstanding = main.Outstanding,
                              WinLoss = main.WinLoss,
                              ClosingBalance = main.ClosingBalance,
                              RewardPromotionAmt = extra.RewardPromotionAmt,
                              ExchangeAmt = extra.ExchangeAmt
                          }
                        );
                }

                return main;
            }

            return new List<PlayerSabaCoins>();
        }

        private static PlayerSabaCoins SumTotalData(List<PlayerSabaCoins> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new PlayerSabaCoins();
            }

            return new PlayerSabaCoins
            {
                OpeningBalance = dataList.Sum(_ => _.OpeningBalance),
                Turnover = dataList.Sum(_ => _.Turnover),
                Outstanding = dataList.Sum(_ => _.Outstanding),
                WinLoss = dataList.Sum(_ => _.WinLoss),
                ClosingBalance = dataList.Sum(_ => _.ClosingBalance),
                RewardPromotionAmt = dataList.Sum(_ => _.RewardPromotionAmt),
                ExchangeAmt = dataList.Sum(_ => _.ExchangeAmt)
            };
        }

        private static SortableList<PlayerSabaCoins> PaginateAndSortData(SortableList<PlayerSabaCoins> dataList, PlayerSabaCoinsMetaModel metaModel)
        {
            var paginateDataList = metaModel.PageIndex == 0 && metaModel.PageSize == 0
                ? dataList : PaginateData(dataList, metaModel.PageIndex, metaModel.PageSize);

            return SortData(paginateDataList, metaModel.SortBy, metaModel.SortOrder);
        }

        private static SortableList<PlayerSabaCoins> SortData(SortableList<PlayerSabaCoins> dataList, string sortBy, string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                var property = typeof(PlayerSabaCoins).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    dataList.Sort(property.Name, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(PlayerSabaCoins.Index), true);
                }
            }

            return dataList;
        }

        private static SortableList<PlayerSabaCoins> PaginateData(SortableList<PlayerSabaCoins> dataList, int pageIndex = 1, int pageSize = 50)
            => new(dataList.Skip(pageSize * (pageIndex - 1)).Take(pageSize));
    }
}