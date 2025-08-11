namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using DTOs;
    using Extensions;
    using Interfaces;
    using MetaModels;

    public class DailyBetsRepository : IDailyBetsRepository
    {
        public async Task<(int totalRows, AuditTicket totalData)> GetTotalDataAsync(DailyBetsMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_Audit_DailyBets_Total";
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        var totalRows = Convert.ToInt32(reader["TotalRows"]);

                        var totalData = totalRows == 0 ? new AuditTicket() : new AuditTicket
                        {
                            TurnoverBased = Convert.ToDecimal(reader["TotalTurnover"]),
                            WinLossBased = Convert.ToDecimal(reader["TotalWinLoss"]),
                            PayoutBased = Convert.ToDecimal(reader["TotalPayout"]),
                        };

                        return (totalRows, totalData);
                    }
                }
            }

            return (0, new AuditTicket());
        }

        private static void UpdateUspParams(IObjectDb db, DailyBetsMetaModel metaModel)
        {
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("ProductCodes", metaModel.ProductCodeList);
            db.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml());
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("GameCodes", metaModel.GameCodes);

            if (metaModel.RefNo > 0 && string.Equals(metaModel.SiteName, "IvoryBet", StringComparison.InvariantCultureIgnoreCase))
            {
                db.SetParameterValue("SearchRefNo", metaModel.RefNo);
            }
        }
    }
}