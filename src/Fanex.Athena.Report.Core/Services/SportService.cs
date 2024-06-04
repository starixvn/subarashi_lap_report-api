using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.Resources.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace Fanex.Athena.Report.Core.Services
{
    public class SportService : ISportService
    {
        private const string MemoryCacheSportListForTreeView = "MemoryCache_SportList_TreeView";

        private readonly ISportRepository _sportRepository;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly IMemoryCache _memoryCache;

        public SportService(ISportRepository sportRepository, ICacheRefAdapter cacheRefAdapter, IMemoryCache memoryCache)
        {
            _sportRepository = sportRepository;
            _cacheRefAdapter = cacheRefAdapter;
            _memoryCache = memoryCache;
        }

        public Task<IEnumerable<SportProductItem>> GetSportsAndProducts(int licenseeId, string languageCode, string productCodes = "") 
            => _sportRepository.GetSportsAndProducts(licenseeId, languageCode, productCodes);

        public async Task<IEnumerable<SportProductItem>> GetSportAndProductAndSabaSport(int licenseeId, string languageCode, bool isHideSaba, string productCodes = "")
        {
            var result = new List<SportProductItem>();
            result.AddRange(await GetSportsAndProducts(licenseeId, languageCode, productCodes));

            result.AddRange(GetSabaSports(isHideSaba).Select(t => new SportProductItem
            {
                Checked = true,
                Id = t.Id,
                Name = t.Name,
                ParentId = ProductTypes.Sportsbook
            }));

            return result;
        }

        public async Task<IEnumerable<CheckedItem>> GetSportsForTreeView(int licenseeId, string languageCode, string productCodes = "", bool isHideSaba = false)
        {
            var cacheKey = MemoryCacheSportListForTreeView + licenseeId + languageCode + productCodes;
            var sportList = _memoryCache.Get<IEnumerable<CheckedItem>>(cacheKey);

            if (sportList != null)
            {
                return sportList;
            }

            var sportItems = await _sportRepository.GetSportsForTreeView(licenseeId, productCodes);
            return BuildSportList(sportItems, isHideSaba);
        }

        private ICollection<CheckedItem> BuildSportList(IEnumerable<SportForTreeViewItem> sportItems, bool isHideSaba = false)
        {
            var sportList = sportItems.GroupBy(_ => (_.ProductId, _.ProductCode),
                _ => BuildSportCheckItem(_, isHideSaba),
                (key, elems) => new CheckedItem
                {
                    ID = key.ProductId,
                    Checked = true,
                    Children = elems,
                    Name = DataHelper.GetProductName(key.ProductCode),
                    Value = key.ProductCode
                }).ToList();

            return sportList;
        }

        private CheckedItem BuildSportCheckItem(SportForTreeViewItem sport, bool isHideSaba = false)
            => new()
            {
                Checked = true,
                Children = null,
                ID = sport.SportType,
                Name = GetSportNameForTreeView(sport.SportType, isHideSaba),
                Value = sport.SportType.ToString()
            };
#pragma warning disable S1541 // Methods and properties should not be too complex
        private string GetSportNameForTreeView(int sportType, bool isHideSaba = false)
        {
            var prefixText = isHideSaba ? ViewResources.Virtual : ViewResources.Saba;

            if (sportType > 0)
            {
                return _cacheRefAdapter.GetSportName(sportType);
            }
            if (sportType == HardCodeSportType.Parlay)
            {
                return ViewResources.Parlay;
            }
            var sportName = string.Empty;
            switch (sportType)
            {
                case HardCodeSportType.SabaBasketball:
                    sportName = _cacheRefAdapter.GetSportName(SportType.BasketBall);
                    break;

                case HardCodeSportType.SabaSoccer:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Soccer);
                    break;

                case HardCodeSportType.SabaCricket:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Cricket);
                    break;

                case HardCodeSportType.SabaSoccerPinGoal:
                    sportName = ViewResources.SoccerPinGoal;
                    break;

                case HardCodeSportType.SabaBasketballPinGoal:
                    sportName = ViewResources.BasketballPinGoal;
                    break;

                case HardCodeSportType.SabaESportPinGoal:
                    sportName = ViewResources.ESportsPinGoal;
                    break;

                case HardCodeSportType.SabaFutsal:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Futsal);
                    break;

                case HardCodeSportType.SabaTennis:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Tennis);
                    break;

                case HardCodeSportType.SabaIceHockey:
                    sportName = _cacheRefAdapter.GetSportName(SportType.IceHockey);
                    break;

                case HardCodeSportType.SabaKabaddi:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Kabaddi);
                    break;

                default:
                    _cacheRefAdapter.GetSportName(sportType);
                    break;
            }
            return $"{prefixText}{sportName}";
        }
#pragma warning restore S1541 // Methods and properties should not be too complex

        public IEnumerable<SabaSport> GetSabaSports(bool isHideSaba = false)
            => SportType.GetSabaVirtualSports().Select(t => new SabaSport
            {
                Id = t.Item1,
                Name = GetSabaName(t.Item2, t.Item3, isHideSaba),
                SportTypeId = t.Item2
            });

        private string GetSabaName(int sportId, string viewResource = "", bool isHideSaba = false)
        {
            var prefixText = isHideSaba ? ViewResources.Virtual : ViewResources.Saba;

            var sabaSportName = sportId != 0
                ? _cacheRefAdapter.GetSportName(sportId)
                : ViewResources.ResourceManager.GetString(viewResource);

            return $"{prefixText}{sabaSportName}";
        }
    }
}
