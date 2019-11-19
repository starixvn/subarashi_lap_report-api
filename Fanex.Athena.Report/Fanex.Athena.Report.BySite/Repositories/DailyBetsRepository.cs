namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Extensions;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Data;

    public class DailyBetsRepository : IDailyBetsRepository
    {
        public async Task<(int totalRows, AuditTicket totalData)> GetTotalDataAsync(DailyBetsMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_Audit_DailyBets_Total";
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        var totalRows = Convert.ToInt32(reader["TotalRows"]);

                        var totalData = totalRows == 0 ? new AuditTicket() : new AuditTicket
                        {
                            Turnover = Convert.ToDecimal(reader["TotalTurnover"]),
                            WinLoss = Convert.ToDecimal(reader["TotalWinLoss"])
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
        }
    }
}