using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Data;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Common.Extensions;

namespace Fanex.Athena.Report.BySite.Repositories
{
    public class TurnoverRepository : ITurnoverRepository
    {
        public async Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel)
        {
            var result = new List<TurnoverDto>();

            using (var db = ObjectDbFactory.CreateInstance("Lrf_Rpt_TurnOver"))
            {
                AddParams(db, metaModel);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var index = 1;
                    while (reader.Read())
                    {
                        var item = ReadDataItem(reader, index++);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private static TurnoverDto ReadDataItem(IDataReader reader, int index)
        {
            return new TurnoverDto
            {
                Index = index,
                Date = reader.ToDateTime("winlostdate").ToString("MM/dd/yyyy"),
                OpeningBalance = reader.ToDecimal("OpeningBalance"),
                Deposit = reader.ToDecimal("Deposit"),
                Withdrawal = reader.ToDecimal("Withdrawal"),
                AffiliateTransfer = reader.ToDecimal("AffiliateTransfer"),
                Bonus = reader.ToDecimal("Bonus"),
                Adjustment = reader.ToDecimal("Adjustment"),
                Promotion = reader.ToDecimal("Promotion"),
                Turnover = reader.ToDecimal("Turnover"),
                PendingFunds = reader.ToDecimal("PendingFund"),
                WinLoss = reader.ToDecimal("WinLoss"),
                ClosingBalance = reader.ToDecimal("ClosingBalance"),
                Transfer3rd = reader.ToDecimal("Transfer3rd"),
                DrawGame = reader.ToDecimal("DrawGame"),
                TurnoverBA = reader.ToDecimal("TurnoverBA"),
                CommBA = reader.ToDecimal("CommBA"),
                AmountDueToBA = reader.ToDecimal("AmountDueToBA"),
                NetTurnover = reader.ToDecimal("Turnover") - reader.ToDecimal("DrawGame"),
                PlayerComm = reader.ToDecimal("PlayerComm"),
            };
        }

        private static void AddParams(IObjectDb db, TurnoverMetaModel metaModel)
        {
            metaModel.ProductOptions.ForEach(item =>
            {
                if (item.Code is Product.BT or Product.CO)
                {
                    db.SetParameterValue("show" + item.Code, item.Status);
                }
            });

            db.SetParameterValue("ProductList", string.Join(",", metaModel.ProductOptions.ToSelectedProductCodeList(_ => _.Code != "BT" && _.Code != "CO")));
            db.SetParameterValue("fromdate", metaModel.FromDate);
            db.SetParameterValue("todate", metaModel.ToDate);
            db.SetParameterValue("username", metaModel.Username);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
        }
    }
}
