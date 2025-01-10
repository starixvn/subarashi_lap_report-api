namespace Fanex.Athena.Report.BySite.Repositories
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Common;
    using Fanex.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    public class DailyBetDetailsRepository : IDailyBetDetailsRepository
    {
        public async Task<ResultModel<DailyBetDetails>> GetDataAsync(DailyBetDetailsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            const string UspName = "Lrf_Rpt_Audit_DailyBets_Detail";
            var data = new List<DailyBetDetails>();
            var totalRows = 0;

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        data.Add(ReadMainDataItem(reader, auditCurrencyList));
                    }
                }

                totalRows = Converter.ToInt(db.GetParameterValue("TotalRow"));
            }

            return new ResultModel<DailyBetDetails>
            {
                DataList = data,
                TotalRows = totalRows
            };
        }

        private static DailyBetDetails ReadMainDataItem(IDataReader reader, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            var currencyName = reader["CurrencyCode"].ToString();
            var hardCodeCurrency = auditCurrencyList.FirstOrDefault(_ => String.Equals(_.Name, currencyName, StringComparison.InvariantCultureIgnoreCase))?.HardCodeName;

            return new DailyBetDetails
            {
                CurrencyName = !string.IsNullOrEmpty(hardCodeCurrency) ? hardCodeCurrency : currencyName,
                RefNo = reader["RefNo"].ToString(),
                SettlementDate = Convert.ToDateTime(reader["SettlementDate"]),
                BrandUrl = reader["BrandUrl"].ToString(),
                GameType = reader["GameType"].ToString(),
                DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
                BetAmount = Convert.ToDecimal(reader["Turnover"]),
                NetAmount = -Convert.ToDecimal(reader["WinLoss"]),
                RIdx = reader["rIdx"].ToString()
            };
        }

        private static void UpdateUspParams(IObjectDb db, DailyBetDetailsMetaModel metaModel)
        {
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("CurrencyList", CurrencyListToXml(metaModel));
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("PageSize", metaModel.PageSize);
            db.SetParameterValue("TotalRow", null);

            if (metaModel.RefNo > 0 && string.Equals(metaModel.SiteName, "IvoryBet", StringComparison.InvariantCultureIgnoreCase))
            {
                db.SetParameterValue("SearchRefNo", metaModel.RefNo);
            }
        }

        private static string CurrencyListToXml(DailyBetDetailsMetaModel metaModel)
        {
            var selectedCurrencies = metaModel.CurrencyIdList
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => int.Parse(_));
            return $"<ROOT>{string.Join("", selectedCurrencies.Select(id => $"<CR Id=\"{id}\" Code=\"{metaModel.CurrencyList[id]}\" />"))}</ROOT>";
        }
    }
}