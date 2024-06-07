using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.Resources.Helpers;
using Fanex.Common.Extensions;
using Fanex.NAP.Core.Web;

namespace Fanex.Athena.Report.Core.Services
{
    public class BetTypeService : IBetTypeService
    {
        private readonly IBetTypeRepository _betTypeRepository;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public BetTypeService(IBetTypeRepository betTypeRepository, ICacheRefAdapter cacheRefAdapter)
        {
            _betTypeRepository = betTypeRepository;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<IList<BettypeProductItem>> GetBettypes(IEnumerable<SportProductItem> sports, string languageCode, bool isHideSaba)
        {
            if (sports != null && sports.Any())
            {
                var sportsParameters = BuildSportXmlParameter(sports);
                var betypes = await _betTypeRepository.GetBettypes(sportsParameters).ConfigureAwait(false);
                return betypes.Select(bettype => BuildBetTypeCheckItem(bettype, languageCode, isHideSaba)).ToList();
            }

            return new List<BettypeProductItem>();
        }

        private BettypeProductItem BuildBetTypeCheckItem((string ProductPrefix, int BetTypeId) betType, string languageCode, bool isHideSaba)
        {
            var betTypeId = betType.BetTypeId;
            var bettypeName = _cacheRefAdapter.GetBetTypeName(betTypeId, languageCode, isHideSaba);
            if (MappingHelper.MapBettypeSportBySport.ContainsKey(betType.BetTypeId))
            {
                var suffix = _cacheRefAdapter.GetSportName(MappingHelper.MapBettypeSportBySport[betTypeId], languageCode);
                bettypeName += " (" + suffix + ")";
            }

            return new BettypeProductItem
            {
                Checked = true,
                Id = betTypeId,
                Name = bettypeName,
                Value = bettypeName,
                ProductPrefix = string.IsNullOrEmpty(betType.ProductPrefix) ? Product.SB : betType.ProductPrefix
            };
        }

        private static string BuildSportXmlParameter(IEnumerable<SportProductItem> sports)
        {
            const string SportXmlItemPattern = "<product ID=\"{0}\" Sport=\"{1}\"> </product>";
            var sportItemXml = new StringBuilder();

            var sportProductItems = sports.ToList();
            sportProductItems.Where(i => i.ParentId == -1).ForEach(product =>
            {
                var childSports = sportProductItems.Where(i => i.ParentId == product.Id && i.ParentId > -1)
                                                   .Select(i => i.Id).ToArray();
                if (childSports.Length == 0)
                {
                    childSports = new[] { product.Id };
                }

                sportItemXml.Append(string.Format(SportXmlItemPattern, product.Id, string.Join(",", childSports)));
            });

            var sportsParameters = "<products>" + sportItemXml + "</products>";
            return sportsParameters;
        }

        public async Task<IList<CheckedItem>>
            GetBetTypesByProduct(int licenseeId, string productCode, string languageCode, bool isHideSaba) =>
            await _betTypeRepository.GetBetTypesByProduct(licenseeId, productCode, languageCode, isHideSaba)
                                    .ConfigureAwait(false);

        public async Task<IEnumerable<CheckedItem>> GetBetTypeForProducts(
            int licenseeId, string lang, string productCodes, bool isHideSaba)
        {
            var betTypes = await _betTypeRepository.GetBetTypeForProductsAsync(licenseeId, productCodes);
            var result = betTypes.GroupBy(_ => _.ProductCode,
                _ => new CheckedItem
                {
                    Checked = true,
                    Children = null,
                    ID = _.BetType,
                    Name = GetBetTypeName(_.BetType, lang, isHideSaba),
                    Value = $"{_.BetType}_{MapBAProductCode(_.ProductCode)}"
                },
                (key, items) => new CheckedItem
                {
                    Checked = true,
                    Children = items,
                    Name = DataHelper.GetProductName(key),
                    Value = key
                });
            return result;
        }

        private string GetBetTypeName(int betType, string languageCode, bool isHideSaba = false)
        {
            var suffix = GetBetTypeNameSuffix(betType);
            return _cacheRefAdapter.GetBetTypeName(Convert.ToInt32(betType), languageCode, isHideSaba) + suffix;
        }

        private string MapBAProductCode(string productCode)
            => productCode == Product.BA || productCode == $"{Product.SB},{Product.BA}" ? Product.SB : productCode;

        private string GetBetTypeNameSuffix(int betType)
        {
            if (betType == BetType.NumberGameHappy5OddEven)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.NumberGameHappy5) + ")";
            }

            if (betType >= BetType.VirtualBasketballOverUnder && betType <= BetType.VirtualBasketballFirstHalfHandicap)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.VirtualBasketball) + ")";
            }

            if (betType == BetType.VirtualSportCorrectScoreTennis)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.Tennis) + ")";
            }

            return string.Empty;
        }
    }
}
