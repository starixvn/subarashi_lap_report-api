namespace Fanex.Athena.Report.BySite.MetaModels;

using Fanex.Core.Utils;

public class MatchMonitoringCenterGetMetaModel : ICurrencyIdMetaModel, IAgentMetaModel
{
    public string MatchIdList { get; set; }

    public int MatchStatus { get; set; }

    public int TriggeredAlert { get; set; }

    public int LicenseeId { get; set; }

    public int CurrencyId { get; set; }

    public string CurrencyName { get; set; }

    public string BaseCurrency { get; set; }

    public string BaseCurrencyName { get; set; }

    public string AgentListXml { get; set; }
}

