using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.Resources.Helpers;

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
