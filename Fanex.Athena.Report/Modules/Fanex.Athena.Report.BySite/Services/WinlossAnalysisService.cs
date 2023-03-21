using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.Params;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services
{
    public class WinlossAnalysisService : IWinlossAnalysisService
    {
        private const string ALL = "ALL";

        private readonly IWinlossAnalysisRepository _repository;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IBalanceUpService _balanceUpService;

        public WinlossAnalysisService(IWinlossAnalysisRepository repository, ICacheRefAdapter cacheRefAdapter, IBalanceUpService balanceUpService)
        {
            _repository = repository;
            _cacheRefAdapter = cacheRefAdapter;
            _balanceUpService = balanceUpService;
        }

        public async Task<object> GetData(WinlossAnalysisParam param)
        {
            await AssignMoveBetDayAsync(param);
            var data = new WinlossAnalysisAggregateDTO();
            if (IsRequestDataForAngency(param))
            {
                data = await _repository.GetDataByAgencyCriteria(param);
            }
            else
            {
                data = await _repository.GetDataByPlayerCriteria(param);
            }

            return new { TotalRow = data.TotalRow, Data = BuildTreeListItem(data, param), SumTotal = data.SumTotal };
        }

        private async Task AssignMoveBetDayAsync(WinlossAnalysisParam param)
        {
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            param.MovebetDay = balanceUp.MoveBetsDay;
        }

        private IEnumerable<object> BuildTreeListItem(WinlossAnalysisAggregateDTO data, WinlossAnalysisParam param)
        {
            if(param.BreakdownMode == Enums.BreakdownMode.Product)
            {
                AssignProductName(data.MainData);
            }

            if (param.GroupMode == Enums.GroupByMode.Sport)
            {
                AssignSportName(data.GroupSumData);
                return BuildTreeListWithProductNameIsRootNode(data);
            }
            if (param.GroupMode == Enums.GroupByMode.Bettype)
            {
                AssignBetType(data.GroupSumData);
                return BuildTreeListWithProductNameIsRootNode(data);
            }
            if (param.GroupMode == Enums.GroupByMode.Player)
            {
                AssignPlayerInfo(data.GroupSumData);
            }
            if(param.GroupMode == Enums.GroupByMode.Product)
            {
                AssignProductName(data.GroupSumData);
            }
            if (param.GroupMode == Enums.GroupByMode.Platform)
            {
                AssignPlatformName(data.GroupSumData);
            }
            return BuildDefaultTreeListItem(data);
        }

        private void AssignBetType(IEnumerable<WinlossAnalysisDTO> groupSumData)
        {
            foreach (var item in groupSumData)
            {
                var spiltName = item.GroupBy.Split('#');
                var productCode = spiltName[0];
                var betType = spiltName[1];
                item.ProductName = DataHelper.GetProductName(productCode);
                item.BetTypeName = _cacheRefAdapter.GetSportName(betType);
            }
        }

        private void AssignSportName(IEnumerable<WinlossAnalysisDTO> groupSumData)
        {
            foreach (var item in groupSumData)
            {
                var spiltName = item.GroupBy.Split('#');
                var productCode = spiltName[0];
                var sportType = spiltName[1];
                item.ProductName = DataHelper.GetProductName(productCode);
                item.SportName = _cacheRefAdapter.GetSportName(sportType);
            }
        }

        private void AssignPlayerInfo(IEnumerable<WinlossAnalysisDTO> groupSumData)
        {
            foreach (var item in groupSumData)
            {
                var spiltName = item.GroupBy.Split('#');
                item.SystemId= spiltName[0];
                item.ExternalId= spiltName[1];
            }
        }

        private void AssignPlatformName(IEnumerable<WinlossAnalysisDTO> groupSumData)
        {
            foreach (var item in groupSumData)
            {
                item.PlatformName = DataHelper.GetPlatformName(int.Parse(item.GroupBy));
            }
        }

        private IEnumerable<object> BuildTreeListWithProductNameIsRootNode(WinlossAnalysisAggregateDTO data)
        {
            var dicMainData = data.MainData.GroupBy(_ => _.RowIndex, _ => _).ToDictionary(_ => _.Key, _ => _);
            var groupSumData = data.GroupSumData.GroupBy(_ => _.ProductName, _ => new TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>
            {
                Item = _,
                Childern = dicMainData[_.RowIndex]
            })
            .Select(_ => new TreeViewItem<string, object>
            {
                Item = _.Key,
                Childern = _.ToList()
            });

            return groupSumData;
        }

        private IEnumerable<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>> BuildDefaultTreeListItem(WinlossAnalysisAggregateDTO data)
        {
            var dicMainData = data.MainData.GroupBy(_ => _.RowIndex, _ => _).ToDictionary(_ => _.Key, _ => _);
            return data.GroupSumData.Select(_ => new TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>
            {
                Item = _,
                Childern = dicMainData[_.RowIndex]
            });
        }

        private void AssignProductName(IEnumerable<WinlossAnalysisDTO> groupSumData)
        {
            foreach (var item in groupSumData)
            {
                item.ProductName = DataHelper.GetProductName(item.GroupBy);
            }
        }

        private bool IsRequestDataForAngency(WinlossAnalysisParam param)
        {
            return (param.SearchCustID == null || param.SearchCustID <=0 )
                && !param.IsVipOnly 
                && param.CustomerClass.Equals(ALL, StringComparison.InvariantCultureIgnoreCase)
                && param.OddsGroup.Equals(ALL,StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
