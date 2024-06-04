using System;
using System.Globalization;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Models;
using Fanex.Athena.Report.Core.Models.MetaModels;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BetList.MetaModels
{
    public class BetListMetaModel : IValidationMetaModel, IPlayerComMetaModel, IMetaModelProductCodeList,
                                    IAgentMetaModel, ISubsidiarySearchMetaModel, ICurrencyIdMetaModel,
                                    ICurrencyIdListMetaModel, ISpecialPlatformMetaModel
    {
        public BetListMetaModel()
        {
            FromDate = DateTimeHelper.Now.Date;
            Date = DateTimeHelper.Now.Date;
            ToDate = DateTimeHelper.Now.Date;
            PageIndex = 1;
            PageSize = int.MaxValue - 1;
        }

        public string Title { get; set; }

        public int LicenseeId { get; set; }

        public int UserId { get; set; }

        public string UspKey { get; set; }

        public bool IsServerPaging { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public DateTime FromDate { get; set; }

        public string FromDateString => FromDate.ToString(CultureInfo.InvariantCulture);

        public DateTime ToDate { get; set; }

        public string ToDateString => ToDate.ToString(CultureInfo.InvariantCulture);

        public DateTime Date { get; set; }

        public string DateString => Date.ToString(CultureInfo.InvariantCulture);

        public int CustId { get; set; }

        public string SystemId { get; set; }

        public string ExternalId { get; set; }

        public DateTime MoveBetsDay { get; set; }

        public DateTime BalanceUpDay { get; set; }

        public int MatchId { get; set; }

        public int HomeId { get; set; }

        public int AwayId { get; set; }

        public int LeagueId { get; set; }

        public int BetType { get; set; }

        public int? LiveIndicator { get; set; }

        public long TransId { get; set; }

        public bool IsFakeLicensee { get; set; }

        public bool IsShowIpvn { get; set; }

        public bool IsShowIp { get; set; }

        public string CurrencyName { get; set; }

        public string BaseCurrency { get; set; }

        public bool IsExportExcel { get; set; }

        public virtual bool IsConvert => !string.IsNullOrEmpty(BaseCurrency);

        public string AgentListXml { get; set; }

        public string CurrencyIdList { get; set; }

        public int CurrencyId { get; set; }

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public bool HasPlayerCom { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public int SpecialPlatform { get; set; }

        public bool IsHideSaba { get; }

        public bool IsValid => FromDate <= ToDate;

    }
}
