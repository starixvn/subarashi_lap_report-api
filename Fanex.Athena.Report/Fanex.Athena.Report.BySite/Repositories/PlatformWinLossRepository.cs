namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Resources.Helpers;
    using Fanex.Data;

    public class PlatformWinLossRepository : IPlatformWinLossRepository
    {
        private const int OneHunderPercent = 100;

        public async Task<ResultModel<PlatformWinLossSummary>> GetSummaryDataAsync(PlatformWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_PlatformWinloss";
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return ReadSummaryList(reader);
                }
            }
        }

        public async Task<ResultModel<PlatformWinLossMember>> GetMemberDataAsync(PlatformWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_PlatformWinlossDetail";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return ReadMemberList(reader);
                }
            }
        }

        private void UpdateUspParams(IObjectDb db, PlatformWinLossMetaModel metaModel)
        {
            db.SetParameterValue("PlatformType", metaModel.PlatformType);
            db.SetParameterValue("movebetsday", metaModel.MoveBetsDay);
            db.SetParameterValue("DrawFilter", metaModel.DrawFilter);
            db.SetParameterValue("FromDate", metaModel.FromDate);
            db.SetParameterValue("ToDate", metaModel.ToDate);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);

            if (!(metaModel.Products is null))
            {
                foreach (var product in metaModel.Products)
                {
                    db.SetParameterValue("show" + product, true);
                }
            }
        }

        private static ResultModel<PlatformWinLossSummary> ReadSummaryList(IDataReader reader)
        {
            var dataList = new List<PlatformWinLossSummary>();
            var totalData = new PlatformWinLossSummary();

            var parentRows = new Dictionary<int, PlatformWinLossSummary>();
            while (reader.Read())
            {
                var platformType = Convert.ToInt32(reader["PlatformType"]);

                PlatformWinLossSummary parent;
                if (!parentRows.ContainsKey(platformType))
                {
                    parent = new PlatformWinLossSummary
                    {
                        PlatformType = platformType,
                        Name = DataHelper.GetPlatformName(platformType),
                        ProductCode = string.Empty
                    };
                    parentRows.Add(platformType, parent);
                    dataList.Add(parent);
                }
                else
                {
                    parent = parentRows[platformType];
                }

                var child = ReadSummaryItem(reader, platformType);

                dataList.Add(child);
                AddTotalSummaryData(parent, child);
                AddTotalSummaryData(totalData, child);
            }

            CalculateTurnoverPercent(dataList, totalData);

            return new ResultModel<PlatformWinLossSummary>
            {
                DataList = dataList,
                TotalData = totalData
            };
        }

        private static PlatformWinLossSummary ReadSummaryItem(IDataReader reader, int platformType, byte level = 1, bool isLeaf = true)
        {
            var productCode = reader["productprefix"].ToString().ToUpperInvariant();

            return new PlatformWinLossSummary
            {
                PlatformType = platformType,
                IsLeaf = isLeaf,
                Level = level,
                Name = DataHelper.GetProductName(productCode),
                TotalTransactions = Convert.ToInt32(reader["Betcount"]),
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                CustomerWinLoss = Convert.ToDecimal(reader["Winlost"]),
                CustomerComm = Convert.ToDecimal(reader["PlayerComm"]),
                LicenseeWL = Convert.ToDecimal(reader["LicenseeWL"]),
                ProductCode = productCode
            };
        }

        private ResultModel<PlatformWinLossMember> ReadMemberList(IDataReader reader)
        {
            var dataList = new List<PlatformWinLossMember>();
            var totalData = new PlatformWinLossMember();

            var index = 1;
            while (reader.Read())
            {
                var item = new PlatformWinLossMember
                {
                    Index = index,
                    TotalTransactions = Convert.ToInt32(reader["Betcount"]),
                    Turnover = Convert.ToDecimal(reader["Turnover"]),
                    CustomerWinLoss = Convert.ToDecimal(reader["Winlost"]),
                    CustomerComm = Convert.ToDecimal(reader["PlayerComm"]),
                    LicenseeWL = Convert.ToDecimal(reader["LicenseeWL"]),
                    BACommission = Convert.ToDecimal(reader["CommBA"]),
                    AmtDueToBA = Convert.ToDecimal(reader["AmtDueToBA"]),
                    CurrencyId = Convert.ToInt32(reader["Currency"]),
                    SystemId = reader["username"].ToString(),
                    CustomerId = Convert.ToInt32(reader["custid"]),
                    ExternalId = reader["nickname"].ToString()
                };

                dataList.Add(item);
                AddTotalMemberData(totalData, item);
                index++;             
            }

            return new ResultModel<PlatformWinLossMember>
            {
                DataList = dataList,
                TotalData = totalData
            };
        }

        private static void CalculateTurnoverPercent(List<PlatformWinLossSummary> dataList, PlatformWinLossSummary totalData)
        {
            totalData.TurnoverPercent = totalData.Turnover > 0 ? OneHunderPercent : 0;

            if (totalData.Turnover > 0)
            {
                foreach (var item in dataList)
                {
                    item.TurnoverPercent = (item.Turnover / totalData.Turnover) * OneHunderPercent;
                }
            }
        }

        private static void AddTotalSummaryData(PlatformWinLossSummary totalData, PlatformWinLossSummary child)
        {
            totalData.TotalTransactions += child.TotalTransactions;
            totalData.Turnover += child.Turnover;
            totalData.CustomerWinLoss += child.CustomerWinLoss;
            totalData.CustomerComm += child.CustomerComm;
            totalData.LicenseeWL += child.LicenseeWL;
        }

        private static void AddTotalMemberData(PlatformWinLossMember totalData, PlatformWinLossMember item)
        {
            totalData.TotalTransactions += item.TotalTransactions;
            totalData.Turnover += item.Turnover;
            totalData.CustomerWinLoss += item.CustomerWinLoss;
            totalData.CustomerComm += item.CustomerComm;
            totalData.LicenseeWL += item.LicenseeWL;
            totalData.BACommission += item.BACommission;
            totalData.AmtDueToBA += item.AmtDueToBA;
        }
    }
}
