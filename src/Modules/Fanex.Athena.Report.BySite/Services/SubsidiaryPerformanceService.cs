namespace Fanex.Athena.Report.BySite.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using DTOs.SubsidiaryPerformance;
    using Interfaces;
    using MetaModels;
    using Models;
    using Repositories.Interfaces;
    using Resources;

    public class SubsidiaryPerformanceService : ISubsidiaryPerformanceService
    {
        private readonly ISubsidiaryPerformanceRepository _subsidiaryPerformanceRepository;
        private readonly ISubsidiaryService _subsidiaryService;
        private readonly IProductService _productService;

        public SubsidiaryPerformanceService(
            ISubsidiaryService subsidiaryService,
            ISubsidiaryPerformanceRepository subsidiaryPerformanceRepository,
            IProductService productService)
        {
            _subsidiaryService = subsidiaryService;
            _subsidiaryPerformanceRepository = subsidiaryPerformanceRepository;
            _productService = productService;
        }

        public async Task<SubsidiaryPerformanceResultModel> GetData(
            SubsidiaryPerformanceMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            var dataList = await _subsidiaryPerformanceRepository.GetData(metaModel, currencyList);

            if (dataList.Any())
            {
                return await BuildSummaryResult(dataList, metaModel);
            }

            return new SubsidiaryPerformanceResultModel
            {
                Data = new List<SubsidiaryPerformanceGroup>()
            };
        }

        public async Task<ReportResultModel<SubsidiaryPerformanceDetail>> GetDetailData(
            SubsidiaryPerformanceMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            if (string.IsNullOrEmpty(metaModel.AgentListXml))
            {
                return new ReportResultModel<SubsidiaryPerformanceDetail>();
            }

            return await _subsidiaryPerformanceRepository.GetDetailData(metaModel, currencyList).ConfigureAwait(false);
        }

        private static void AssignSumData(SubsidiaryPerformanceBase total, SubsidiaryPerformanceBase detail)
        {
            total.BetCount += detail.BetCount;
            total.Turnover += detail.Turnover;
            total.CustomerWinLoss += detail.CustomerWinLoss;
            total.EffectiveBettingValue += detail.EffectiveBettingValue;
            total.SubsidiaryWinLoss += detail.SubsidiaryWinLoss;
        }

        private async Task<SubsidiaryPerformanceResultModel> BuildSummaryResult(
            IEnumerable<SubsidiaryPerformance> entityDataList,
            SubsidiaryPerformanceMetaModel metaModel)
        {
            var mainModel = new List<SubsidiaryPerformanceSummary>();
            var totalModel = new Dictionary<string, SubsidiaryPerformanceBase>();
            var grandTotal = new SubsidiaryPerformanceBase { CurrencyName = metaModel.BaseCurrency.ToUpperInvariant() };

            var operators = await _subsidiaryService.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList);
            var totalItem = 0;

            var subsidiaryGroup = operators.Join(
                    entityDataList,
                    oper => oper.AgentId,
                    data => data.SubsidiaryId,
                    (oper, data) => new { Subsidiary = oper.OperatorId, Data = data })
                .GroupBy(
                    data => data.Subsidiary,
                    (subsidiary, subsidiaryData) => new
                    {
                        Subsidiary = subsidiary,
                        Details = subsidiaryData.GroupBy(x => x.Data.CurrencyId,
                            (currency, currencyData) => new { CurrencyId = currency, Details = currencyData.Select(y => y.Data) })
                    });

            foreach (var subsidiary in subsidiaryGroup)
            {
                var main = new SubsidiaryPerformanceSummary
                {
                    Subsidiary = subsidiary.Subsidiary,
                    Details = new Dictionary<string, IEnumerable<SubsidiaryPerformanceBase>>()
                };
                var total = new SubsidiaryPerformanceBase { CurrencyName = grandTotal.CurrencyName };
                var currencyData = subsidiary.Details.OrderBy(x => x.CurrencyId);

                foreach (var currencyDetail in currencyData.Select(e => e.Details))
                {
                    var products = GetProducts(currencyDetail, total, metaModel.IsHideSaba);

                    main.Details.Add(currencyDetail.First().CurrencyName, products);
                    main.TotalItem += products.Count;
                }

                mainModel.Add(main);
                totalModel.Add(subsidiary.Subsidiary, total);
                AssignSumData(grandTotal, total);
                totalItem += main.TotalItem;
            }

            if (string.IsNullOrWhiteSpace(metaModel.WhiteLabelList))
            {
                return new SubsidiaryPerformanceResultModel
                {
                    Data = new[] { new SubsidiaryPerformanceGroup { WhiteLabel = string.Empty, Details = mainModel, TotalItem = totalItem } },
                    Total = totalModel,
                    GrandTotal = grandTotal
                };
            }

            var whiteLabels =
                await _subsidiaryService.GetGroupSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList, metaModel.WhiteLabelList);

            return new SubsidiaryPerformanceResultModel
            {
                Data = GroupSubsidiaryByWhiteLabel(mainModel, whiteLabels),
                Total = GroupSubsidiaryTotalByWhiteLabel(totalModel, whiteLabels, metaModel.BaseCurrency.ToUpperInvariant()),
                GrandTotal = grandTotal
            };
        }

        private static IDictionary<string, SubsidiaryPerformanceBase> GroupSubsidiaryTotalByWhiteLabel(
            IDictionary<string, SubsidiaryPerformanceBase> subsidiaryTotal,
            IEnumerable<GroupSubsidiary> whiteLabels,
            string currencyName)
        {
            return subsidiaryTotal
                .Join(
                    whiteLabels,
                    sub => sub.Key,
                    label => label.OperatorId,
                    (sub, label) => new { WhiteLabel = label.WhiteLabelName, Total = sub.Value },
                    StringComparer.OrdinalIgnoreCase)
                .GroupBy(
                    data => data.WhiteLabel,
                    (whiteLabel, group) => new
                    {
                        WhiteLabel = whiteLabel ?? ViewResources.Others,
                        Total = new SubsidiaryPerformanceBase
                        {
                            BetCount = group.Sum(x => x.Total.BetCount),
                            Turnover = group.Sum(x => x.Total.Turnover),
                            CustomerWinLoss = group.Sum(x => x.Total.CustomerWinLoss),
                            EffectiveBettingValue = group.Sum(x => x.Total.EffectiveBettingValue),
                            SubsidiaryWinLoss = group.Sum(x => x.Total.SubsidiaryWinLoss),
                            CurrencyName = currencyName
                        }
                    }
                ).ToDictionary(key => key.WhiteLabel, value => value.Total);
        }

        private static IEnumerable<SubsidiaryPerformanceGroup> GroupSubsidiaryByWhiteLabel(
            IEnumerable<SubsidiaryPerformanceSummary> subsidiaryGroup,
            IEnumerable<GroupSubsidiary> whiteLabels)
        {
            return subsidiaryGroup
                .Join(
                    whiteLabels,
                    sub => sub.Subsidiary,
                    label => label.OperatorId,
                    (sub, label) => new { WhiteLabel = label.WhiteLabelName, Subsidiary = sub },
                    StringComparer.OrdinalIgnoreCase)
                .GroupBy(
                    data => data.WhiteLabel,
                    (whiteLabel, group) => new SubsidiaryPerformanceGroup
                    {
                        WhiteLabel = whiteLabel ?? ViewResources.Others,
                        Details = group.Select(x => x.Subsidiary).ToList(),
                        TotalItem = group.Sum(x => x.Subsidiary.TotalItem)
                    }
                ).ToList();
        }

        private IList<SubsidiaryPerformanceBase> GetProducts(
            IEnumerable<SubsidiaryPerformance> productList,
            SubsidiaryPerformanceBase baseSumTotal,
            bool isHideSaba)
        {
            var groupedProduct = productList.GroupBy(
                x => x.ProductCode,
                (key, item) => new
                {
                    ProductCode = key,
                    Data = new SubsidiaryPerformanceBase
                    {
                        BetCount = item.Sum(x => x.BetCount),
                        Turnover = item.Sum(x => x.Turnover),
                        CustomerWinLoss = item.Sum(x => x.CustomerWinLoss),
                        EffectiveBettingValue = item.Sum(x => x.EffectiveBettingValue),
                        SubsidiaryWinLoss = item.Sum(x => x.SubsidiaryWinLoss)
                    },
                    BaseData = new SubsidiaryPerformanceBase
                    {
                        BetCount = item.Sum(x => x.BaseCurrencyData.BetCount),
                        Turnover = item.Sum(x => x.BaseCurrencyData.Turnover),
                        CustomerWinLoss = item.Sum(x => x.BaseCurrencyData.CustomerWinLoss),
                        EffectiveBettingValue = item.Sum(x => x.BaseCurrencyData.EffectiveBettingValue),
                        SubsidiaryWinLoss = item.Sum(x => x.BaseCurrencyData.SubsidiaryWinLoss)
                    }
                });

            var products = new List<SubsidiaryPerformanceBase>();
            var productAll = new SubsidiaryPerformanceBase
            {
                Product = ViewResources.All
            };
            products.Add(productAll);

            foreach (var product in groupedProduct)
            {
                var productName = _productService.GetProductName(product.ProductCode);
                product.Data.Product = isHideSaba ? SabaHelper.RemoveSabaText(productName) : productName;
                products.Add(product.Data);

                AssignSumData(productAll, product.Data);
                AssignSumData(baseSumTotal, product.BaseData);
            }

            return products;
        }
    }
}