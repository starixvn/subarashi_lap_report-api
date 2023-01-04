namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Core.Utils.Service.Interfaces;
    using Fanex.Data;
    using static Fanex.Athena.Report.ByPlayer.Constants.ReportConstant;

    public class AbnormalBetRepository : IAbnormalBetRepository
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IIpService _ipService;
        private readonly ISubIssueService _subIssueService;
        private const int ParlayBettype = 29;

        private readonly Dictionary<int, string> LiveStatus = new()
        {
            { LiveStatusTypes.NonLive, ViewResources.NoneLive },
            { LiveStatusTypes.Live, ViewResources.Live },
            { LiveStatusTypes.Mix, ViewResources.MixLiveAndNoneLive }
        };

        public AbnormalBetRepository(ICacheRefAdapter cacheRefAdapter, IIpService ipService, ISubIssueService subIssueService)
        {
            _cacheRefAdapter = cacheRefAdapter;
            _ipService = ipService;
            _subIssueService = subIssueService;
        }

        private static void UpdateSpCommonParamsAsync(IObjectDb db, AbnormalBetMetaModel metaModel)
        {
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("sportTypeList", metaModel.SportIdList);
            db.SetParameterValue("leagueName", metaModel.LeagueName);
            db.SetParameterValue("betTypeList", metaModel.BetTypeIdList);
            db.SetParameterValue("liveIndicator", metaModel.StatusIdList);
            db.SetParameterValue("reasonList", metaModel.ReasonIdList);
            db.SetParameterValue("pageSize", metaModel.PageSize);
            db.SetParameterValue("pageIndex", metaModel.PageIndex);
            db.SetParameterValue("SearchLanguage", CultureInfo.CurrentUICulture.Name);
        }

        public async Task<SortableReportResultModel<AbnormalBetSummary>> GetSummaryData(AbnormalBetMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_AbnormalBet_Summary";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("LicenseeName", metaModel.LicenseeName);
            UpdateSpCommonParamsAsync(db, metaModel);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadSummaryData(reader);
        }

        public SortableReportResultModel<AbnormalBetSummary> ReadSummaryData(IDataReader reader)
        {
            var dataList = new SortableList<AbnormalBetSummary>();
            var subIssueList = _subIssueService.GetSubIssueList();

            while (reader.Read())
            {
                LiveStatus.TryGetValue(reader.GetVal<int>("Live"), out string live);
                subIssueList.TryGetValue(reader.GetVal<int>("reason"), out string reason);
                var homeId = reader.GetVal<int>("HomeId");
                var awayId = reader.GetVal<int>("AwayId");
                var isNeutral = reader.GetVal<bool>("IsNeutral");
                var leagueId = reader.GetVal<int>("League");
                var sportType = reader.GetVal<int>("Sport");
                var betType = reader.GetVal<int>("betType");
                var finalScore = reader.GetVal<string>("finalScore");
                var matchName = _cacheRefAdapter.GetMatchName(homeId, awayId, isNeutral);

                var item = new AbnormalBetSummary
                {
                    Date = reader.GetVal<DateTime>("Date").ToString("MM/dd/yyyy"),
                    Sport = GetSportName(sportType, betType),
                    League = GetLeagueName(leagueId, sportType),
                    Match = string.Equals(matchName, " - vs - ") ? string.Empty : matchName,
                    KOTime = betType == ParlayBettype ? string.Empty : reader.GetVal<DateTime>("KOTime").ToString("MM/dd/yyyy hh:mm:ss tt"),
                    Live = live,
                    Reason = reason,
                    TotalTransaction = reader.GetVal<int>("TotalTransaction"),
                    BetType = _cacheRefAdapter.GetBettype(betType),
                    Choice = betType == ParlayBettype ? string.Empty : GetChoiceName(betType, reader.GetVal<string>("choice")),
                    FinalScore = string.Equals(finalScore, "-") || betType == ParlayBettype ? string.Empty : finalScore,
                };

                dataList.Add(item);
            }

            return new SortableReportResultModel<AbnormalBetSummary>
            {
                DataList = dataList
            };
        }

        public async Task<SortableReportResultModel<AbnormalBetIpSummary>> GetIpSummaryData(AbnormalBetMetaModel metaModel)
        {
            if (ShouldReturnEmptyResult(metaModel))
            {
                return new SortableReportResultModel<AbnormalBetIpSummary>
                {
                    DataList = new SortableList<AbnormalBetIpSummary>()
                };
            }

            const string UspName = "Lrf_Rpt_AbnormalBet_IPSummary";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("Username", metaModel.UserName);
            db.SetParameterValue("IP", metaModel.Ip);
            UpdateSpCommonParamsAsync(db, metaModel);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadIpSummaryData(reader, metaModel.IsShowIpvn);
        }

        public SortableReportResultModel<AbnormalBetIpSummary> ReadIpSummaryData(IDataReader reader, bool isShowIpvn)
        {
            var dataList = new SortableList<AbnormalBetIpSummary>();

            while (reader.Read())
            {
                var ipLogin = reader.GetVal<string>("iplogin");
                var ticketIP = reader.GetVal<string>("ticketIPs");
                var ticketIpList = string.Join(", ", ticketIP.Split(',').Select(_ => _ipService.GetIpAddress(_, isShowIpvn)).Where(_ => !string.IsNullOrEmpty(_)));
                var ticketLocationList = string.Join(", ", ticketIP.Split(',').Select(_ => _ipService.GetIpLocation(_, isShowIpvn).CountryName).Where(_ => !string.IsNullOrEmpty(_)).Distinct());

                var item = new AbnormalBetIpSummary
                {
                    ExternalId = reader.GetVal<string>("externalID"),
                    SystemID = reader.GetVal<string>("systemID"),
                    LastLoginIP = _ipService.GetIpAddress(ipLogin, isShowIpvn),
                    LastLoginLocation = _ipService.GetIpLocation(ipLogin, isShowIpvn).CountryName,
                    TicketIP = ticketIpList,
                    TicketLocation = ticketLocationList,
                };

                dataList.Add(item);
            }

            return new SortableReportResultModel<AbnormalBetIpSummary>
            {
                DataList = dataList
            };
        }

        public async Task<SortableReportResultModel<AbnormalBetDetail>> GetDetailData(AbnormalBetMetaModel metaModel)
        {
            if (ShouldReturnEmptyResult(metaModel))
            {
                return new SortableReportResultModel<AbnormalBetDetail>
                {
                    DataList = new SortableList<AbnormalBetDetail>()
                };
            }

            const string UspName = "Lrf_Rpt_AbnormalBet_Detail";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("Username", metaModel.UserName);
            db.SetParameterValue("IP", metaModel.Ip);
            db.SetParameterValue("Refno", metaModel.RefNo);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("ConvertToBaseCur", metaModel.IsConvertAllToBaseCurrency);
            UpdateSpCommonParamsAsync(db, metaModel);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadDetaiData(reader, metaModel);
        }

        public SortableReportResultModel<AbnormalBetDetail> ReadDetaiData(IDataReader reader, AbnormalBetMetaModel metaModel)
        {
            var dataList = new SortableList<AbnormalBetDetail>();
            var subIssueList = _subIssueService.GetSubIssueList();

            while (reader.Read())
            {
                LiveStatus.TryGetValue(reader.GetVal<int>("Live"), out string live);
                subIssueList.TryGetValue(reader.GetVal<int>("reason"), out string reason);
                var homeId = reader.GetVal<int>("HomeId");
                var awayId = reader.GetVal<int>("AwayId");
                var isNeutral = reader.GetVal<bool>("IsNeutral");
                var leagueId = reader.GetVal<int>("League");
                var sportType = reader.GetVal<int>("Sport");
                var betType = reader.GetVal<int>("betType");
                var finalScore = reader.GetVal<string>("finalScore");
                var matchName = _cacheRefAdapter.GetMatchName(homeId, awayId, isNeutral);
                var ip = _ipService.GetIpAddress(reader.GetVal<string>("ip"), metaModel.IsShowIpvn);

                var item = new AbnormalBetDetail
                {
                    Date = reader.GetVal<DateTime>("date").ToString("MM/dd/yyyy"),
                    ExternalId = reader.GetVal<string>("externalID"),
                    SystemID = reader.GetVal<string>("systemID"),
                    Currency = reader.GetVal<string>("currency"),
                    RefNo = reader.GetVal<string>("refNo"),
                    Odds = reader.GetVal<decimal>("odds"),
                    Stake = reader.GetVal<decimal>("stake"),
                    HDP = reader.GetVal<decimal>("HDP"),
                    LiveHomeScore = betType == ParlayBettype ? string.Empty : reader.GetVal<int>("liveHomeScore").ToString(),
                    LiveAwayScore = betType == ParlayBettype ? string.Empty : reader.GetVal<int>("liveAwayScore").ToString(),
                    Sport = GetSportName(sportType, betType),
                    BetType = _cacheRefAdapter.GetBettype(betType),
                    Choice = betType == ParlayBettype ? string.Empty : GetChoiceName(betType, reader.GetVal<string>("choice")),
                    TransTime = reader.GetVal<DateTime>("transTime").ToString("MM/dd/yyyy hh:mm:ss tt"),
                    League = GetLeagueName(leagueId, sportType),
                    Match = string.Equals(matchName, " - vs - ") ? string.Empty : matchName,
                    KOTime = betType == ParlayBettype ? string.Empty : reader.GetVal<DateTime>("kOTime").ToString("MM/dd/yyyy hh:mm:ss tt"),
                    Live = live,
                    FinalScore = string.Equals(finalScore, "-") || betType == ParlayBettype ? string.Empty : finalScore,
                    IP = ip,
                    Location = _ipService.GetIpLocation(ip, metaModel.IsShowIpvn).CountryName,
                    Reason = reason
                };

                dataList.Add(item);
            }

            SortDetailData(dataList, metaModel.SortBy, metaModel.SortOrder);

            return new SortableReportResultModel<AbnormalBetDetail>
            {
                DataList = dataList
            };
        }

        private string GetLeagueName(int leagueId, int sportType = 0)
        {
            var leagueName = sportType == SportType.BitCoin ? _cacheRefAdapter.GetLeagueName3rd(leagueId) : _cacheRefAdapter.GetLeagueName(leagueId);
            return leagueName;
        }

        private static string GetChoiceName(int betType, string choice)
        {
            int[] HandicapBetTypeList = new[] { 1, 7 };
            int[] OverUnderBetTypeList = new[] { 3, 8 };

            if (HandicapBetTypeList.Contains(betType) && string.Equals(choice, "h", StringComparison.OrdinalIgnoreCase))
            {
                return ViewResources.Home;
            }
            else if (HandicapBetTypeList.Contains(betType) && string.Equals(choice, "a", StringComparison.OrdinalIgnoreCase))
            {
                return ViewResources.Away;
            }
            else if (OverUnderBetTypeList.Contains(betType) && string.Equals(choice, "h", StringComparison.OrdinalIgnoreCase))
            {
                return ViewResources.Over;
            }
            else if (OverUnderBetTypeList.Contains(betType) && string.Equals(choice, "a", StringComparison.OrdinalIgnoreCase))
            {
                return ViewResources.Under;
            }
            else
            {
                return choice;
            }
        }

        private static void SortDetailData(SortableList<AbnormalBetDetail> dataList, string sortBy, string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                var property = typeof(AbnormalBetDetail).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    dataList.Sort(property.Name, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(AbnormalBetDetail.Stake), true);
                }
            }
        }

        private string GetSportName(int sport, int bettype)
        {
            if (bettype == ParlayBettype)
            {
                return ViewResources.Parlay;
            }

            if (sport > 0)
            {
                return _cacheRefAdapter.GetSportName(sport);
            }
            else
            {
                var sabaVirtualSports = SportType.GetSabaVirtualSports();
                var sabaSport = sabaVirtualSports.Find(_ => _.Item1 == sport);

                var sabaSportName = sabaSport.Item2 != 0
                    ? _cacheRefAdapter.GetSportName(sabaSport.Item2)
                    : ViewResources.ResourceManager.GetString(sabaSport.Item3);

                return ViewResources.Saba + (sabaSportName);
            }
        }

        private bool ShouldReturnEmptyResult(AbnormalBetMetaModel metaModel)
           => !string.IsNullOrWhiteSpace(metaModel.Ip) && !metaModel.IsShowIpvn && _ipService.IsVNIP(metaModel.Ip);
    }
}