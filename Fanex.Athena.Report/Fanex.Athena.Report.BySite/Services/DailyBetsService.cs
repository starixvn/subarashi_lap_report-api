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
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;

    public class DailyBetsService : IDailyBetsService
    {
        private const string UspName = "Lrf_Rpt_Audit_DailyBets";

        private readonly IDailyBetsRepository _repository;

        public DailyBetsService(IDailyBetsRepository dailyBetsRepository)
        {
            _repository = dailyBetsRepository;
        }

        public async Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel)
        {
            var getMainDataTask = GetMainDataAsync(metaModel);
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

        public static async Task<IList<AuditTicket>> GetMainDataAsync(DailyBetsMetaModel metaModel)
        {
            var productNames = GetProductName(metaModel.ProductCodeList);
            var statusNames = GetStatusName();

            var result = new List<AuditTicket>();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        result.Add(ReadMainDataItem(reader, statusNames, productNames));
                    }
                }
            }

            return result;
        }

        private static AuditTicket ReadMainDataItem(
            IDataReader reader,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            return new AuditTicket
            {
                Index = Convert.ToInt32(reader["Index"]),
                PlayerName = reader["PlayerName"].ToString(),
                CurrencyName = reader["CurrencyName"].ToString(),
                RefNo = Convert.ToInt64(reader["RefNo"]),
                TransTime = Convert.ToDateTime(reader["TransTime"]),
                ProductName = productNames[reader["ProductCode"].ToString()],
                BetType = reader["BetType"].ToString(),
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                WinLoss = Convert.ToDecimal(reader["WinLoss"]),
                Status = statusNames[Convert.ToInt16(reader["Status"])]
            };
        }

        private static void UpdateUspParams(IObjectDb db, DailyBetsMetaModel metaModel)
        {
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("ProductCodes", metaModel.ProductCodeList);
            db.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml());
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("PageSize", metaModel.PageSize);

            if (!string.IsNullOrWhiteSpace(metaModel.SortBy) && !string.IsNullOrWhiteSpace(metaModel.SortOrder))
            {
                db.SetParameterValue("SortBy", metaModel.SortBy);
                db.SetParameterValue("SortOrder", metaModel.SortOrder);
            }
        }

        private static IDictionary<string, string> GetProductName(string productCodeString)
        {
            var productCodes = productCodeString.Split(",");

            return productCodes.ToDictionary(key => key, value => ViewResources.ResourceManager.GetString(value));
        }

        private static IDictionary<int, string> GetStatusName()
        {
            return new Dictionary<int, string>
            {
                { 0, ViewResources.Draw },
                { 1, ViewResources.Won },
                { -1, ViewResources.Lose }
            };
        }
    }
}