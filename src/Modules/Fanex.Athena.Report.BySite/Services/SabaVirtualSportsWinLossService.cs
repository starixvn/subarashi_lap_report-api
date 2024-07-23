using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.BySite.Services
{
    public class SabaVirtualSportsWinLossService : ISabaVirtualSportsWinLossService
    {
        private static readonly IEnumerable<(int Id, int SportId, string Name, IEnumerable<int> SabaSportIds)> SportList =
            new List<(int, int, string, IEnumerable<int>)>
            {
                (1, SportType.Soccer, ViewResources.Soccer, new[] { SportType.SabaSoccer, SportType.SabaFutsal, }),
                (-1, SportType.Soccer, ViewResources.SoccerPinGoal, new[] { SportType.SabaSoccerPingoal, }),
                (2, SportType.BasketBall, ViewResources.Basketball, new[] { SportType.SabaBasketball, }),
                (-2, SportType.BasketBall, ViewResources.BasketballPinGoal, new[] { SportType.SabaBasketballPinGoal, }),
                (50, SportType.Cricket, ViewResources.Cricket, new[] { SportType.SabaCricket, }),
                (-43, SportType.Esports, ViewResources.ESportsPinGoal, new[] { SportType.SabaESportsPinGoal, }),
                (5, SportType.Tennis, ViewResources.Tennis, new[] { SportType.SabaTennis, }),
                (4, SportType.IceHockey, ViewResources.IceHockey, new[] { SportType.SabaIceHockey, }),
                (47, SportType.Kabaddi, ViewResources.Kabaddi, new[] { SportType.SabaKabaddi, })
            };


        private readonly ISabaVirtualSportsWinLossRepository _sabaVirtualSportsWinLossRepository;

        public SabaVirtualSportsWinLossService(ISabaVirtualSportsWinLossRepository sabaVirtualSportsWinLossRepository)
        {
            _sabaVirtualSportsWinLossRepository = sabaVirtualSportsWinLossRepository;
        }

        public async Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetDataWithDrillDownAsync(
            SabaVirtualSportsWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            SortableReportResultModel<SabaVirtualSportsWinLossDetail> result;

            if (metaModel.DrillDownLevel == 0)
            {
                result = await _sabaVirtualSportsWinLossRepository.GetSummaryWinlossAsync(metaModel, currencyList)
                                                                  .ConfigureAwait(false);
            }
            else if (metaModel.LicenseeId != LicenseeIdentity.OneTwoBet || metaModel.DrillDownLevel == (int)ReportLevel.Customer)
            {
                result = await _sabaVirtualSportsWinLossRepository.GetCustomerWinlossDetailAsync(metaModel, currencyList)
                                                                  .ConfigureAwait(false);
            }
            else
            {
                result = await _sabaVirtualSportsWinLossRepository.GetAgentWinlossDetailAsync(metaModel, currencyList)
                                                                  .ConfigureAwait(false);
            }

            return result;
        }

        public void SortData(SortableList<SabaVirtualSportsWinLossDetail> dataList, string sortBy, string sortOrder = "asc")
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return;
            }

            var propertyName = sortBy;

            // Sort for grouping columns, get the real column name
            if (sortBy.EndsWith("Single", StringComparison.OrdinalIgnoreCase))
            {
                propertyName = sortBy.Remove(sortBy.LastIndexOf("Single", StringComparison.OrdinalIgnoreCase));
            }

            var property = typeof(SabaVirtualSportsWinLossDetail).GetProperty(propertyName);
            if (property is not null)
            {
                dataList.Sort(propertyName, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(SabaVirtualSportsWinLossDetail.Index), true);
                return;
            }

            string productCode;

            if (propertyName.Contains(nameof(SabaVirtualSportsWinLossDetail.LicenseeWL)))
            {
                const string LicenseeWlColumn = nameof(SabaVirtualSportsWinLossDetail.LicenseeWL);

                productCode = propertyName[LicenseeWlColumn.Length..];
                propertyName = LicenseeWlColumn;
            }
            else
            {
                const string TurnOverColumn = nameof(SabaVirtualSportsWinLossDetail.TurnOver);

                productCode = propertyName.Substring(TurnOverColumn.Length);
                propertyName = TurnOverColumn;
            }

            dataList.SortByDictionary(productCode, propertyName, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(SabaVirtualSportsWinLossDetail.Index), true);
        }

        public IEnumerable<(int Id, int SportId, string Name, IEnumerable<int> SabaSportIds)> GetSportList() => SportList;
    }
}
