namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Data;
    using DTOs;
    using Fanex.Common.Extensions;
    using Interfaces;
    using MetaModels;
    using Resources.Constants;

    public class TurnoverRepository : ITurnoverRepository
    {
        public async Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel)
        {
            var result = new List<TurnoverDto>();

            using (var db = ObjectDbFactory.CreateInstance("Lrf_Rpt_TurnOver"))
            {
                AddParams(db, metaModel);

                using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
                var index = 1;
                while (reader.Read())
                {
                    var item = ReadDataItem(reader, index++);
                    result.Add(item);
                }
            }

            return result;
        }

        private static TurnoverDto ReadDataItem(IDataReader reader, int index)
        {
            return new TurnoverDto
            {
                Index = index,
                Date = reader.GetVal<DateTime>("winlostdate").ToString("MM/dd/yyyy"),
                OpeningBalance = reader.GetVal<decimal>("OpeningBalance"),
                Deposit = reader.GetVal<decimal>("Deposit"),
                Withdrawal = reader.GetVal<decimal>("Withdrawal"),
                AffiliateTransfer = reader.GetVal<decimal>("AffiliateTransfer"),
                Bonus = reader.GetVal<decimal>("Bonus"),
                Adjustment = reader.GetVal<decimal>("Adjustment"),
                Promotion = reader.GetVal<decimal>("Promotion"),
                Turnover = reader.GetVal<decimal>("Turnover"),
                PendingFunds = reader.GetVal<decimal>("PendingFund"),
                WinLoss = reader.GetVal<decimal>("WinLoss"),
                ClosingBalance = reader.GetVal<decimal>("ClosingBalance"),
                Transfer3rd = reader.GetVal<decimal>("Transfer3rd"),
                DrawGame = reader.GetVal<decimal>("DrawGame"),
                NetTurnover = reader.GetVal<decimal>("Turnover") - reader.GetVal<decimal>("DrawGame"),
                PlayerComm = reader.GetVal<decimal>("PlayerComm")
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