namespace Fanex.Athena.Report.BySite.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Extensions;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Data;

    public class DailyBetsService : IDailyBetsService
    {
        private const string UspName = "Lrf_Rpt_Audit_DailyBets";

        private readonly IDailyBetsRepository _repository;

        public DailyBetsService(IDailyBetsRepository dailyBetsRepository)
        {
            _repository = dailyBetsRepository;
        }

        public async Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            var getMainDataTask = GetMainDataAsync(metaModel, auditCurrencyList);
            var getTotalDataTask = _repository.GetTotalDataAsync(metaModel);

            await Task.WhenAll(getMainDataTask, getTotalDataTask).ConfigureAwait(false);

            var (totalRows, totalData) = await getTotalDataTask;
            return new ResultModel<AuditTicket>
            {
                DataList = await getMainDataTask,
                TotalRows = totalRows,
                TotalData = totalData
            };
        }

        private static async Task<IList<AuditTicket>> GetMainDataAsync(DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            var result = new List<AuditTicket>();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                metaModel.InjectToDbParameter(db);
                using (var reader = await db.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        result.Add(ReadMainDataItem(reader, auditCurrencyList));
                    }
                }
            }

            return result;
        }

        private static AuditTicket ReadMainDataItem(IDataReader reader, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            var currencyName = reader["CurrencyName"].ToString();
            var hardCodeCurrency = auditCurrencyList.FirstOrDefault(_ => String.Equals(_.Name, currencyName, StringComparison.InvariantCultureIgnoreCase))?.HardCodeName;

            return new AuditTicket
            {
                Index = Convert.ToInt32(reader["Index"]),
                PlayerName = reader["PlayerName"].ToString(),
                CurrencyName = !string.IsNullOrEmpty(hardCodeCurrency) ? hardCodeCurrency : currencyName,
                RefNo = reader["RefNo"].ToString(),
                TransTime = Convert.ToDateTime(reader["TransTime"]),
                ProductName = DailyBetsMetaModelExtension.GetProductName[reader["ProductCode"].ToString()](),
                SportName = reader["SportName"].ToString(),
                BetType = reader["BetType"].ToString(),
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                WinLoss = Convert.ToDecimal(reader["WinLoss"]),
                TurnoverBased = Convert.ToDecimal(reader["TurnoverBased"]),
                WinLossBased = Convert.ToDecimal(reader["WinLossBased"]),
                Status = DailyBetsMetaModelExtension.GetStatusName[Convert.ToInt16(reader["Status"])]()
            };
        }
    }
}