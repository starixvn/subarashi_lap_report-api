using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.Enums;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Helpers;
using Fanex.Common.Extensions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Fanex.Athena.Report.BySite.Constants.ReportConstant;
using System;
using System.Collections.Generic;
using Fanex.Athena.Report.Core.Services.Interfaces;

namespace Fanex.Athena.Report.BySite.Services
{
    public class WinlossAnalysisService : IWinlossAnalysisService
    {
        private const string ORIGINAL_DATE_FORMAT = "yyyy-MM-dd";
        private const string DATE_FORMAT = "MM/dd/yyyy";
        private const string MONTH_FORMAT = "MM/yyyy";
        private const int DEFAULT_SORT_BY = 0;

        private readonly IWinlossAnalysisRepository _repository;
        private readonly INameService _nameService;
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;

        public WinlossAnalysisService(IWinlossAnalysisRepository repository,
            INameService nameService,
            IBalanceUpService balanceUpService,
            ICurrencyService currencyService,
            IProductService productService,
            ILogger<WinlossAnalysisService> logger)
        {
            _repository = repository;
            _nameService = nameService;
            _balanceUpService = balanceUpService;
            _currencyService = currencyService;
            _productService = productService;
        }

        public async Task<WinlossAnalysisAggregateDTO> GetRawDataAsync(WinlossAnalysisMetaModel param)
        {
            await AssignMoveBetDayAsync(param);

            if (param.ViewInTime == ViewInTimeMode.Settlement)
            {
                return await _repository.GetDataBySettlementTimeCriteria(param);
            }

            return IsRequestDataForAgency(param)
                ? await _repository.GetDataByAgencyCriteria(param)
                : await _repository.GetDataByPlayerCriteria(param);
        }

        public async Task<WinlossAnalysisResultModel> GetDataAsync(WinlossAnalysisMetaModel param)
        {
            var data = await GetRawDataAsync(param);
            var result = new WinlossAnalysisResultModel { TotalRow = data.TotalRow, Data = await BuildTreeListItem(data, param), SumTotal = data.SumTotal };
            return result;
        }

        private async Task AssignMoveBetDayAsync(WinlossAnalysisMetaModel param)
        {
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            param.MovebetDay = balanceUp.MoveBetsDay;
        }

        private async Task<IEnumerable<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>>> BuildTreeListItem(WinlossAnalysisAggregateDTO data, WinlossAnalysisMetaModel param)
        {
            var isHideSaba = SabaHelper.IsHideSaba(param.SpecialPlatform);
            var breakDownDatas = await BuildDataListForBreakdownData(param, data.MainData);
            var groupByDatas = await BuildDataListForGroupByData(param, data.GroupSumData, isHideSaba);
            return BuildDefaultTreeListItem(groupByDatas, breakDownDatas);
        }

        private async Task<IEnumerable<WinlossAnalysisDTO>> BuildDataListForBreakdownData(WinlossAnalysisMetaModel param, IEnumerable<WinlossAnalysisDTO> mainData)
        {
            var listData = new List<WinlossAnalysisDTO>(mainData.Count());
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

        private async Task<WinlossAnalysisDTO> BuildAndAssginRelevantFieldForBreakdownData(WinlossAnalysisMetaModel param, WinlossAnalysisDTO item)
        {
            switch (param.BreakdownMode)
            {
                case BreakdownMode.Product:
                    item.BreakDownFieldText = _productService.GetProductName(item.BreakBy);
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

        private async Task<IEnumerable<WinlossAnalysisDTO>> BuildDataListForGroupByData(WinlossAnalysisMetaModel param, IEnumerable<WinlossAnalysisDTO> groupSumData, bool isHideSaba)
        {
            var groupByMode = param.GroupMode;
            var listData = new SortedList<dynamic, WinlossAnalysisDTO>(groupSumData.Count());
            foreach (var data in groupSumData)
            {
                var item = await BuildAndAssginRelevantFieldForGroupSumData(groupByMode, data, isHideSaba);
                if (groupByMode == GroupByMode.Currency && param.SortBy == DEFAULT_SORT_BY)
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

        private async Task<WinlossAnalysisDTO> BuildAndAssginRelevantFieldForGroupSumData(GroupByMode groupByMode, WinlossAnalysisDTO item, bool isHideSaba)
        {
            switch (groupByMode)
            {
                case GroupByMode.Sport:
                    AssignSportName(item, isHideSaba);
                    break;

                case GroupByMode.Bettype:
                    AssignBetType(item, isHideSaba);
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

                case GroupByMode.Hour:
                    AssignDateTimeAndHour(item, DATE_FORMAT);
                    break;

                default:
                    AssignGroupByField(item);
                    break;
            }
            return item;
        }

        private async Task AssignCurrencyName(WinlossAnalysisDTO item)
        {
            var currencies = await _currencyService.GetCurrencyListAsync();
            var currency = currencies.FirstOrDefault(_ => _.ID == Convert.ToInt32(item.GroupBy));
            if (currency != null)
            {
                item.GroupByFieldText = currency.Name;
            }
        }

        private static void AssignDateTime(WinlossAnalysisDTO item, string formatDate)
        {
            var date = DateTime.ParseExact(item.GroupBy, ORIGINAL_DATE_FORMAT, CultureInfo.InvariantCulture);
            item.GroupByFieldText = date.ToString(formatDate);
        }

        private static void AssignDateTimeAndHour(WinlossAnalysisDTO item, string formatDate)
        {
            var texts = item.GroupBy.Split('>');
            var dateText = texts.Length > 0 ? texts[0].Trim() : "";
            var hourText = texts.Length > 1 ? texts[1].Trim() : "";

            var date = DateTime.ParseExact(dateText, ORIGINAL_DATE_FORMAT, CultureInfo.InvariantCulture);
            item.GroupByFieldText = $"{date.ToString(formatDate)} > {hourText}";
        }

        private static void AssignGroupByField(WinlossAnalysisDTO item)
            => item.GroupByFieldText = item.GroupBy;

        private void AssignBetType(WinlossAnalysisDTO item, bool isHideSaba)
        {
            var spiltName = item.GroupBy.Split('#');
            var productCode = spiltName[0];
            var betType = spiltName[1];
            var productName = _productService.GetProductName(productCode);
            var betTypeName = _nameService.GetBetTypeName(Convert.ToInt32(betType), isHideSaba);
            item.GroupByFieldText = $"{productName} > {betTypeName}";
        }

        private void AssignSportName(WinlossAnalysisDTO item, bool isHideSaba)
        {
            var spiltName = item.GroupBy.Split('#');
            var productCode = spiltName[0];
            var sportType = spiltName[1];
            var productName = _productService.GetProductName(productCode);
            var sportName = _nameService.GetSportName(Convert.ToInt32(sportType), isHideSaba);
            item.GroupByFieldText = $"{productName} > {sportName}";
        }

        private static void AssignPlayerInfo(WinlossAnalysisDTO item)
        {
            var spiltName = item.GroupBy.Split('#');
            item.SystemId = spiltName[0];
            item.ExternalId = spiltName[1];
            item.GroupByFieldText = item.SystemId;
        }

        private static void AssignPlatformName(WinlossAnalysisDTO item)
            => item.GroupByFieldText = DataHelper.GetPlatformName(int.Parse(item.GroupBy));

        private void AssignProductName(WinlossAnalysisDTO item)
            => item.GroupByFieldText = _productService.GetProductName(item.GroupBy);

        private IEnumerable<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>> BuildDefaultTreeListItem(
            IEnumerable<WinlossAnalysisDTO> groupByDatas,
            IEnumerable<WinlossAnalysisDTO> breakDownDatas)
        {
            var groupByRowIndexData = breakDownDatas.GroupBy(_ => _.RowIndex, _ => _);
            var dicMainData = new Dictionary<int, IGrouping<int, WinlossAnalysisDTO>>(groupByRowIndexData.Count());
            groupByRowIndexData.ForEach(_ => dicMainData.Add(_.Key, _));
            var result = new List<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>>(groupByDatas.Count());
            groupByDatas.ForEach(_ => result.Add(new TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>
            {
                KeyItem = _,
                Children = dicMainData.ContainsKey(_.RowIndex) ? dicMainData[_.RowIndex] : Enumerable.Empty<WinlossAnalysisDTO>()
            }));

            return result;
        }

        private static void ReAssignDisplayIndex(IEnumerable<WinlossAnalysisDTO> list, int startIndex)
        {
            foreach (var item in list)
            {
                item.DisplayRowIndex = startIndex;
                startIndex++;
            }
        }

        private static bool IsRequestDataForAgency(WinlossAnalysisMetaModel param)
            => string.IsNullOrWhiteSpace(param.SearchUserName)
                && !param.IsVipOnly
                && param.CustomerClass.Equals(ALL, StringComparison.InvariantCultureIgnoreCase)
                && param.OddsGroup.Equals(ALL, StringComparison.InvariantCultureIgnoreCase);
    }
}