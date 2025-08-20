namespace Fanex.Athena.Report.BySite.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Models;
    using Repositories.Interfaces;
    using Resources.Constants;

    public class SettledMatchCountService : ISettledMatchCountService
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly ISettledMatchCountRepository _settledMatchCountRepository;

        public SettledMatchCountService(ISettledMatchCountRepository settledMatchCountRepository,
                                        ICacheRefAdapter cacheRefAdapter)
        {
            _settledMatchCountRepository = settledMatchCountRepository;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<IList<SettledMatchCountResultModel>> GetDataList(SettledMatchCountMetaModel metaModel)
        {
            var result = await _settledMatchCountRepository.GetDataList(metaModel.IsShowAll, metaModel.LicenseeId)
                                                           .ConfigureAwait(false);

            return FilterValidSports(result, metaModel.ProductAccess);
        }

        private static bool IsValidSportType(int sportType, ProductAccess productAccess)
        {
            return sportType switch
            {
                SportType.NumberGame => productAccess.IsAllowNg,
                SportType.VirtualSport => productAccess.IsAllowVs,
                _ => true
            };
        }

        private IList<SettledMatchCountResultModel> ConvertDtoDataToViewData(IList<SettledMatchCount> dtoData)
        {
            var resultList = dtoData.Where(x => x.IsToday == 0)
                                    .Select(i => new SettledMatchCountResultModel
                                    {
                                        SportName = _cacheRefAdapter.GetSportName(i.SportType),
                                        YesterdayEntered = i.Entered,
                                        YesterdayNonEntered = i.NonEntered,
                                        TodayEntered =
                                             dtoData.Where(g => g.IsToday == 1 && g.SportType == i.SportType)
                                                    .Select(j => j.Entered)
                                                    .FirstOrDefault(),
                                        TodayNonEntered =
                                             dtoData.Where(g => g.IsToday == 1 && g.SportType == i.SportType)
                                                    .Select(j => j.NonEntered)
                                                    .FirstOrDefault()
                                    }).ToList();

            var otherResultList = dtoData.Where(x => x.IsToday == 1)
                                         .Select(i => new SettledMatchCountResultModel
                                         {
                                             SportName = _cacheRefAdapter.GetSportName(i.SportType),
                                             TodayEntered = i.Entered,
                                             TodayNonEntered = i.NonEntered,
                                             YesterdayEntered =
                                                  dtoData.Where(g => g.IsToday == 0 && g.SportType == i.SportType)
                                                         .Select(j => j.Entered)
                                                         .FirstOrDefault(),
                                             YesterdayNonEntered =
                                                  dtoData.Where(g => g.IsToday == 0 && g.SportType == i.SportType)
                                                         .Select(j => j.NonEntered)
                                                         .FirstOrDefault()
                                         }).ToList();

            resultList.AddRange(otherResultList.Where(item => resultList.TrueForAll(x => x.SportName != item.SportName)));
            return resultList;
        }

        private IList<SettledMatchCountResultModel> FilterValidSports(
            IEnumerable<SettledMatchCount> settledMatchCountList, ProductAccess productAccess)
        {
            var listAfterCheckProduct = settledMatchCountList
                                       .Where(item => IsValidSportType(item.SportType, productAccess)).ToList();

            return ConvertDtoDataToViewData(listAfterCheckProduct);
        }
    }
}
