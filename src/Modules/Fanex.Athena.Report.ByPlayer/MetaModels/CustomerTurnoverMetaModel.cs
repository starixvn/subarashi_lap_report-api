using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Core.Utils;
using System;

namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    public class CustomerTurnoverMetaModel : IValidationMetaModel, IAgentMetaModel, ICurrencyIdMetaModel
    {
        public CustomerTurnoverMetaModel()
        {
            FromDate = DateTimeHelper.Now;
            ToDate = DateTimeHelper.Now;
            ReportBy = ReportByType.Daily;
            ShowWin = true;
            ShowLoss = true;
            ShowDraw = true;
            Odds = 0;
        }

        public string ProductIds { get; set; }

        public bool IsExportExcel { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Bettypes { get; set; }

        public string Username { get; set; }

        public decimal Odds { get; set; }

        public int ReportBy { get; set; }

        public bool ShowWin { get; set; }

        public bool ShowLoss { get; set; }

        public bool ShowDraw { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string AgentListXml { get; set; }

        public int CurrencyId { get; set; }

        public bool IsValid
        {
            get
            {
                /*Only suport filter by all customer for sungame1 licensee (licenseeId=8).*/
                var isShow = ShowWin || ShowLoss || ShowDraw;

                return isShow && FromDate <= ToDate && !string.IsNullOrEmpty(ProductIds) &&
                       !string.IsNullOrEmpty(Bettypes);
            }
        }
    }
}
