using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.Enums;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Fanex.Athena.Report.BySite.Constants.ReportConstant;

namespace Fanex.Athena.Report.BySite.Services
{
    public class WinlossAnalysisService : IWinlossAnalysisService
    {
        private const string ORIGINAL_DATE_FORMAT = "yyyy-MM-dd";
        private const string DATE_FORMAT = "MM/dd/yyyy";
        private const string MONTH_FORMAT = "MM/yyyy";

        private readonly IWinlossAnalysisRepository _repository;
        private readonly INameService _nameService;
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICurrencyService _currencyService;

        public WinlossAnalysisService(IWinlossAnalysisRepository repository,
            INameService nameService,
            IBalanceUpService balanceUpService,
            ICurrencyService currencyService)
        {
            _repository = repository;
            _nameService = nameService;
            _balanceUpService = balanceUpService;
            _currencyService = currencyService;
        }

        public async Task<WinlossAnalysisAggregateDTO> GetRawDataAsync(WinlossAnalysisMetaModel param)
        {
            await AssignMoveBetDayAsync(param);
            return IsRequestDataForAgency(param)
                ? await _repository.GetDataByAgencyCriteria(param)
                : await _repository.GetDataByPlayerCriteria(param);
        }

        public async Task<WinlossAnalysisResultModel> GetDataAsync(WinlossAnalysisMetaModel param)
        {
            var data = await GetRawDataAsync(param);
            return new WinlossAnalysisResultModel { TotalRow = data.TotalRow, Data = await BuildTreeListItem(data, param), SumTotal = GetValueForBaseCurrency(data.SumTotal) };
        }

        private async Task AssignMoveBetDayAsync(WinlossAnalysisMetaModel param)
        {
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            param.MovebetDay = balanceUp.MoveBetsDay;
        }

        private async Task<IEnumerable<TreeViewItem<WinlossAnalysisViewModel, WinlossAnalysisViewModel>>> BuildTreeListItem(WinlossAnalysisAggregateDTO data, WinlossAnalysisMetaModel param)
        {
            var breakDownDatas = await BuildDataListForBreakdownData(param, data.MainData);
            var groupByDatas = await BuildDataListForGroupByData(param.GroupMode, data.GroupSumData);
            return BuildDefaultTreeListItem(groupByDatas, breakDownDatas);
        }

        private async Task<IEnumerable<WinlossAnalysisViewModel>> BuildDataListForBreakdownData(WinlossAnalysisMetaModel param, IEnumerable<WinlossAnalysisDTO> mainData)
        {
            var listData = new List<WinlossAnalysisViewModel>(mainData.Count());
            foreach (var data in mainData)
            {
                listData.Add(await BuildAndAssginRelevantFieldForBreakdownData(param, data));
            }
            if (param.BreakdownMode == BreakdownMode.Currency)
            {
                return listData.OrderBy(_ => _.BreakDownFieldText);
            }
            return listData;
        }

        private async Task<WinlossAnalysisViewModel> BuildAndAssginRelevantFieldForBreakdownData(WinlossAnalysisMetaModel param, WinlossAnalysisDTO data)
        {
            var item = param.IsShowInOrginalCurrency ? GetValueForOriginalCurrency(data) : GetValueForBaseCurrency(data);
            switch (param.BreakdownMode)
            {
                case BreakdownMode.Product:
                    item.BreakDownFieldText = DataHelper.GetProductName(item.BreakBy);
                    break;
                case BreakdownMode.Currency:
                    var currencies = await _currencyService.GetCurrencyListAsync();
                    var currency = currencies.FirstOrDefault(_ => _.ID == Convert.ToInt32(item.BreakBy));
                    if (currency != null)
                    {
                        item.BreakDownFieldText = currency.Name;
                    }
                    break;
                case BreakdownMode.Player:
                    var spiltName = item.BreakBy.Split('#');
                    item.SystemId = spiltName[0];
                    item.ExternalId = spiltName[1];
                    item.BreakDownFieldText = item.SystemId;
                    break;
                default:
                    item.BreakDownFieldText = item.BreakBy;
                    break;
            }
            return item;
        }

        private async Task<IEnumerable<WinlossAnalysisViewModel>> BuildDataListForGroupByData(GroupByMode groupByMode, IEnumerable<WinlossAnalysisDTO> groupSumData)
        {
            var listData = new SortedList<dynamic, WinlossAnalysisViewModel>(groupSumData.Count());
            foreach (var data in groupSumData)
            {
                var item = await BuildAndAssginRelevantFieldForGroupSumData(groupByMode, data);
                if (groupByMode == GroupByMode.Currency)
                {
                    listData.Add(item.GroupByFieldText, item);
                }
                else
                {
                    listData.Add(item.RowIndex, item);
                }
            }

            var returnList = listData.Values;
            var startIndex = groupSumData.FirstOrDefault()?.RowIndex;
            if (groupByMode == GroupByMode.Currency && startIndex != null)
            {
                ReAssignDisplayIndex(returnList, startIndex.Value);
            }    
            return returnList;
        }

        private async Task<WinlossAnalysisViewModel> BuildAndAssginRelevantFieldForGroupSumData(GroupByMode groupByMode, WinlossAnalysisDTO data)
        {
            var item = GetValueForBaseCurrency(data);
            switch (groupByMode)
            {
                case GroupByMode.Sport:
                    AssignSportName(item);
                    break;
                case GroupByMode.Bettype:
                    AssignBetType(item);
                    break;
                case GroupByMode.Player:
                    AssignPlayerInfo(item);
                    break;
                case GroupByMode.Product:
                    AssignProductName(item);
                    break;
                case GroupByMode.Platform:
                    AssignPlatformName(item);
                    break;
                case GroupByMode.WinlossDate:
                    AssignDateTime(item, DATE_FORMAT);
                    break;
                case GroupByMode.Month:
                    AssignDateTime(item, MONTH_FORMAT);
                    break;
                case GroupByMode.Currency:
                    await AssignCurrencyName(item);
                    break;
                default:
                    AssignGroupByField(item);
                    break;
            }
            return item;
        }

        private async Task AssignCurrencyName(WinlossAnalysisViewModel item)
        {
            var currencies = await _currencyService.GetCurrencyListAsync();
            var currency = currencies.FirstOrDefault(_ => _.ID == Convert.ToInt32(item.GroupBy));
            if (currency != null)
            {
                item.GroupByFieldText = currency.Name;
            }
        }

        private static void AssignDateTime(WinlossAnalysisViewModel item, string formatDate)
        {
                var date = DateTime.ParseExact(item.GroupBy, ORIGINAL_DATE_FORMAT, CultureInfo.InvariantCulture);
                item.GroupByFieldText = date.ToString(formatDate);          
        }

        private static void AssignGroupByField(WinlossAnalysisViewModel item)
            => item.GroupByFieldText = item.GroupBy;
        

        private void AssignBetType(WinlossAnalysisViewModel item)
        {
                var spiltName = item.GroupBy.Split('#');
                var productCode = spiltName[0];
                var betType = spiltName[1];
                var productName = DataHelper.GetProductName(productCode);
                var betTypeName = _nameService.GetBetTypeName(Convert.ToInt32(betType));
                item.GroupByFieldText = $"{productName} > {betTypeName}";
        }

        private void AssignSportName(WinlossAnalysisViewModel item)
        {
                var spiltName = item.GroupBy.Split('#');
                var productCode = spiltName[0];
                var sportType = spiltName[1];
                var productName = DataHelper.GetProductName(productCode);
                var sportName = _nameService.GetSportName(Convert.ToInt32(sportType));
                item.GroupByFieldText = $"{productName} > {sportName}";
        }

        private static void AssignPlayerInfo(WinlossAnalysisViewModel item)
        {
                var spiltName = item.GroupBy.Split('#');
                item.SystemId = spiltName[0];
                item.ExternalId = spiltName[1];
                item.GroupByFieldText = item.SystemId;
        }

        private static void AssignPlatformName(WinlossAnalysisViewModel item)        
            => item.GroupByFieldText = DataHelper.GetPlatformName(int.Parse(item.GroupBy));
        

        private static void AssignProductName(WinlossAnalysisViewModel item)
            => item.GroupByFieldText = DataHelper.GetProductName(item.GroupBy);

        private IEnumerable<TreeViewItem<WinlossAnalysisViewModel, WinlossAnalysisViewModel>> BuildDefaultTreeListItem(
            IEnumerable<WinlossAnalysisViewModel> groupByDatas, 
            IEnumerable<WinlossAnalysisViewModel> breakDownDatas)
        {
            var dicMainData = breakDownDatas.GroupBy(_ => _.RowIndex, _ => _).ToDictionary(_ => _.Key, _ => _);
            return groupByDatas.Select(_ => new TreeViewItem<WinlossAnalysisViewModel, WinlossAnalysisViewModel>
            {
                KeyItem = _,
                Children = dicMainData.ContainsKey(_.RowIndex) ? dicMainData[_.RowIndex] : Enumerable.Empty<WinlossAnalysisViewModel>(),
            });
        }

        private static void ReAssignDisplayIndex(IEnumerable<WinlossAnalysisViewModel> list, int startIndex)
        {
            foreach (var item in list)
            {
                item.DisplayRowIndex= startIndex;
                startIndex++;
            }
        }

        private WinlossAnalysisViewModel GetValueForOriginalCurrency(WinlossAnalysisDTO data)
            => new WinlossAnalysisViewModel
            {
                RowIndex = data.RowIndex,
                DisplayRowIndex = data.RowIndex,
                AmtDueToBA = data.AmtDueToBA,
                BetCount = data.BetCount,
                BetEffective = data.BetEffective,
                BreakBy = data.BreakBy,
                GroupBy = data.GroupBy,
                LicComm = data.LicComm,
                LicWinloss = data.LicWinloss,
                Margin = data.Margin,
                Turnover = data.Turnover,
                NetTurnover = data.NetTurnover,
                PlayerComm = data.MemComm,
                PlayerWinloss = data.MemWinloss              
            };

        private WinlossAnalysisViewModel GetValueForBaseCurrency(WinlossAnalysisDTO data)
            => new WinlossAnalysisViewModel
            {
                RowIndex = data.RowIndex,
                DisplayRowIndex = data.RowIndex,
                AmtDueToBA = data.BaseAmtDueToBA,
                BetCount = data.BetCount,
                BetEffective = data.BaseBetEffective,
                BreakBy = data.BreakBy,
                GroupBy = data.GroupBy,
                LicComm = data.BaseLicComm,
                LicWinloss = data.BaseLicWinloss,
                Margin = data.Margin,
                Turnover = data.BaseTurnover,
                NetTurnover = data.BaseNetTurnover,
                PlayerComm = data.BaseMemComm,
                PlayerWinloss = data.BaseMemWinloss
            };

        private static bool IsRequestDataForAgency(WinlossAnalysisMetaModel param)
            => string.IsNullOrWhiteSpace(param.SearchUserName)
                && !param.IsVipOnly
                && param.CustomerClass.Equals(ALL, StringComparison.InvariantCultureIgnoreCase)
                && param.OddsGroup.Equals(ALL, StringComparison.InvariantCultureIgnoreCase);

    }
}
