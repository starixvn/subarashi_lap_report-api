namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;

    public class DailyBetsRepository : IDailyBetsRepository
    {
        public async Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel)
        {
            var result = new ResultModel<AuditTicket>();

            const string UspName = "Lrf_Rpt_Audit_DailyBets";
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    result.DataList = ReadMainData(reader);
                    
                    if (reader.NextResult() && reader.Read())
                    {
                        result.TotalData = ReadTotalData(reader);
                        result.TotalRows = Convert.ToInt32(reader["TotalRows"]);
                    }
                }
            }

            return result;
        }

        private static IEnumerable<AuditTicket> ReadMainData(IDataReader reader)
        {
            while (reader.Read())
            {
                var status = Convert.ToInt16(reader["Status"]);
                var productCode = reader["ProductCode"].ToString();
                var item = new AuditTicket
                {
                    Index = Convert.ToInt32(reader["Index"]),
                    PlayerName = reader["PlayerName"].ToString(),
                    CurrencyName = reader["CurrencyName"].ToString(),
                    RefNo = Convert.ToInt64(reader["RefNo"]),
                    TransTime = Convert.ToDateTime(reader["TransTime"]),
                    ProductCode = ViewResources.ResourceManager.GetString(productCode),
                    BetType = reader["BetType"].ToString(),
                    Turnover = Convert.ToDecimal(reader["Turnover"]),
                    WinLoss = Convert.ToDecimal(reader["WinLoss"]),
                    Status = GetStatusText(status)
                };

                yield return item;
            }
        }

        private static AuditTicket ReadTotalData(IDataReader reader)
        {
            return new AuditTicket
            {
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                WinLoss = Convert.ToDecimal(reader["WinLoss"])
            };
        }

        private static string GetStatusText(int status)
        {
            if (status == 0)
            {
                return ViewResources.Draw;
            } 
            
            return status > 0 ? ViewResources.Won : ViewResources.Lose;
        }

        private static void UpdateUspParams(IObjectDb db, DailyBetsMetaModel metaModel)
        {
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("ProductCodes", metaModel.ProductCodes);
            db.SetParameterValue("CurrencyList", metaModel.CurrencyList);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("PageSize", metaModel.PageSize);

            if (!string.IsNullOrWhiteSpace(metaModel.SortBy) && !string.IsNullOrWhiteSpace(metaModel.SortOrder))
            {
                db.SetParameterValue("SortBy", metaModel.SortBy);
                db.SetParameterValue("SortOrder", metaModel.SortOrder);
            }
        }
    }
}
