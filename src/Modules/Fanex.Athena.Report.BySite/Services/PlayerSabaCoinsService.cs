namespace Fanex.Athena.Report.BySite.Services
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

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

            if (!mainData.Any())
            {
                return GetEmptyResult();
            }

            var dataList = JoinData(mainData, extraInfoData);
            var groupedDataList = GroupData(dataList);
            var sortedDataList = PaginateAndSortData(new SortableList<PlayerSabaCoins>(groupedDataList), metaModel);

            return new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = sortedDataList,
                TotalData = SumTotalData(groupedDataList),
                TotalRows = groupedDataList.Count(),
            };
        }

        private async Task<(IEnumerable<PlayerSabaCoinsMain>, IEnumerable<PlayerSabaCoinsExtraInfo>)>
            GetDataFromDBAsync(PlayerSabaCoinsMetaModel metaModel)
        {
            var getMainData = _repository.GetMainData(metaModel);
            var getExtraInfoData = _repository.GetExtraInfoData(metaModel);
            await Task.WhenAll(getMainData, getExtraInfoData).ConfigureAwait(false);

            return (await getMainData, await getExtraInfoData);
        }

        private static IEnumerable<PlayerSabaCoinsMain> JoinData(IEnumerable<PlayerSabaCoinsMain> main, IEnumerable<PlayerSabaCoinsExtraInfo> extraInfo)
        {
            return main.LeftOuterJoin(extraInfo, main => new
            {
                main.CustId,
                main.SabaId
            }, extra => new
            {
                extra.CustId,
                extra.SabaId
            },
                (main, extra) => new PlayerSabaCoinsMain
                {
                    CustId = main.CustId,
                    SabaId = main.SabaId,
                    Username = main.Username,
                    ExternalId = main.ExternalId,
                    OpeningBalance = main.OpeningBalance,
                    Turnover = main.Turnover,
                    Outstanding = main.Outstanding,
                    WinLoss = main.WinLoss,
                    ClosingBalance = main.ClosingBalance,
                    RewardPromotionAmt = extra?.RewardPromotionAmt ?? 0,
                    ExchangeAmt = extra?.ExchangeAmt ?? 0
                }
            );
        }

        private static IEnumerable<PlayerSabaCoins> GroupData(IEnumerable<PlayerSabaCoinsMain> dataList)
        {
            return dataList.GroupBy(_ => _.SabaId,
                (key, g) => new PlayerSabaCoins
                {
                    SabaId = key,
                    OpeningBalance = g.FirstOrDefault().OpeningBalance,
                    Turnover = g.FirstOrDefault().Turnover,
                    Outstanding = g.FirstOrDefault().Outstanding,
                    WinLoss = g.FirstOrDefault().WinLoss,
                    ClosingBalance = g.FirstOrDefault().ClosingBalance,
                    RewardPromotionAmt = g.FirstOrDefault().RewardPromotionAmt,
                    ExchangeAmt = g.FirstOrDefault().ExchangeAmt,
                    Accounts = g.Select(_ => new PlayerSabaCoinsAccount
                    {
                        CustId = _.CustId,
                        ExternalId = _.ExternalId,
                        Username = _.Username
                    })
                }
            );
        }

        private static PlayerSabaCoins SumTotalData(IEnumerable<PlayerSabaCoins> dataList)
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
                    dataList.Sort(property.Name, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase));
                }
            }

            return dataList;
        }

        private static SortableList<PlayerSabaCoins> PaginateData(SortableList<PlayerSabaCoins> dataList, int pageIndex = 1, int pageSize = 50)
            => new(dataList.Skip(pageSize * (pageIndex - 1)).Take(pageSize));

        private static SortableReportResultModel<PlayerSabaCoins> GetEmptyResult() =>
            new()
            {
                DataList = new SortableList<PlayerSabaCoins>(),
                TotalData = new PlayerSabaCoins()
            };
    }
}