using System;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.Core.Models.MetaModels
{
    public class LeagueMetaModel : IAgentMetaModel, ICurrencyIdMetaModel, IMetaModelUserId
    {
        public string AgentListXml { get; set; }

        public int CurrencyId { get; set; }

        public int UserId { get; set; }

        public int SportId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool IsExcludedTestData { get; set; }
    }
}
