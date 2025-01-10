using Fanex.Athena.Report.Core.Models.MetaModels;
using Fanex.Core.Utils;
using System;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class PlayerSabaCoinsMetaModel : ISubsidiarySearchMetaModel, ICurrencyIdListMetaModel, IAgentMetaModel, IDataTestableMetaModel
    {
        private string _username;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string CurrencyIdList { get; set; }

        public string AgentListXml { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public bool IsExcludedTestData { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public string Username
        {
            get
            {
                return string.IsNullOrEmpty(_username) ? string.Empty : _username?.Trim();
            }

            set
            {
                _username = value;
            }
        }
    }
}