using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Common.Extensions;
using Fanex.Data;
using Fanex.Athena.Report.Core.Repositories.Abstraction;

namespace Fanex.Athena.Report.BySite.Repositories
{
    public class WinlossAnalysisRepository : IWinlossAnalysisRepository
    {
        private readonly IDbFactory _dbFactory;

        public WinlossAnalysisRepository(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public Task<WinlossAnalysisAggregateDTO> GetDataBySettlementTimeCriteria(WinlossAnalysisMetaModel metaModel)
        {
            const string spName = "LAP_Rpt_WinlossAnalysis_SettleTimeSel";
            using (var objectDb = _dbFactory.GetObjectDb(spName))
            {
                AddCommonParameter(objectDb, metaModel);
                AddPlayerParameter(objectDb, metaModel);
                objectDb.SetParameterValue("HourParts", metaModel.HourParts);
                return ReadDataAsync(objectDb);
            }
        }

        public Task<WinlossAnalysisAggregateDTO> GetDataByAgencyCriteria(WinlossAnalysisMetaModel metaModel)
        {
            const string spName = "LAP_Rpt_WinlossAnalysis_AgencySel";
            using (var objectDb = _dbFactory.GetObjectDb(spName))
            {
                AddCommonParameter(objectDb, metaModel);
                return ReadDataAsync(objectDb);
            }
        }

        public Task<WinlossAnalysisAggregateDTO> GetDataByPlayerCriteria(WinlossAnalysisMetaModel metaModel)
        {
            const string spName = "LAP_Rpt_WinlossAnalysis_PlayerSel";
            using (var objectDb = _dbFactory.GetObjectDb(spName))
            {
                AddCommonParameter(objectDb, metaModel);
                AddPlayerParameter(objectDb, metaModel);
                return ReadDataAsync(objectDb);
            }
        }

        private static void AddCommonParameter(IObjectDb objectDb, WinlossAnalysisMetaModel metaModel)
        {
            objectDb.SetParameterValue("FromDate", metaModel.FromDate);
            objectDb.SetParameterValue("ToDate", metaModel.ToDate);
            objectDb.SetParameterValue("MovebetDay", metaModel.MovebetDay);
            objectDb.SetParameterValue("AgentList", metaModel.AgentListXml);
            objectDb.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            objectDb.SetParameterValue("ProductCodes", metaModel.ProductCodes);
            objectDb.SetParameterValue("SportIds", metaModel.SportIds);
            objectDb.SetParameterValue("BettypeIds", metaModel.BetTypeIds);
            objectDb.SetParameterValue("StatusIds", metaModel.StatusIds);
            objectDb.SetParameterValue("PlatformIds", metaModel.PlatformIds);
            objectDb.SetParameterValue("LiveStatusIds", metaModel.LiveStatusIds);
            objectDb.SetParameterValue("CashOutMode", metaModel.CashOutMode);
            objectDb.SetParameterValue("GroupMode", metaModel.GroupMode);
            objectDb.SetParameterValue("BreakdownMode", metaModel.BreakdownMode);
            objectDb.SetParameterValue("PageIndex", metaModel.PageIndex);
            objectDb.SetParameterValue("PageSize", metaModel.PageSize);
            objectDb.SetParameterValue("SortKey", metaModel.SortBy);
            objectDb.SetParameterValue("SortType", metaModel.SortOrder);
            objectDb.SetParameterValue("SpecialPlatform", metaModel.SpecialPlatformType);
        }

        private static void AddPlayerParameter(IObjectDb objectDb, WinlossAnalysisMetaModel metaModel)
        {
            objectDb.SetParameterValue("IsVIPOnly", metaModel.IsVipOnly);
            objectDb.SetParameterValue("CustomerClass", metaModel.CustomerClass);
            objectDb.SetParameterValue("FullCustomerClass", metaModel.FullCustomerClass);
            objectDb.SetParameterValue("OddsGroup", metaModel.OddsGroup);
            objectDb.SetParameterValue("SearchMode", metaModel.SearchMode);

            if (!string.IsNullOrEmpty(metaModel.SearchUserName))
            {
                objectDb.SetParameterValue("SearchUserName", metaModel.SearchUserName);
            }
        }

        private static async Task<WinlossAnalysisAggregateDTO> ReadDataAsync(IObjectDb objectDb)
        {
            var result = new WinlossAnalysisAggregateDTO();
            using (var reader = await objectDb.ExecuteReaderAsync())
            {
                result.MainData = ReadMainDataAsync(reader);
                reader.NextResult();
                result.GroupSumData = ReadGroupSumDataAsync(reader);
                reader.NextResult();
                result.SumTotal = ReadSumTotalAsync(reader);
            }
            return result;
        }

        private static IEnumerable<WinlossAnalysisDTO> ReadMainDataAsync(IDataReader reader)
        {
            var listMainData = new List<WinlossAnalysisDTO>();
            while (reader.Read())
            {
                var data = new WinlossAnalysisDTO
                {
                    RowIndex = reader.ToInt("ridx"),
                    GroupBy = reader.ToString("GroupBy"),
                    BreakBy = reader.ToString("BreakBy"),
                };
                ReadCommonField(reader, data);
                listMainData.Add(data);
            }
            return listMainData;
        }

        private static IEnumerable<WinlossAnalysisDTO> ReadGroupSumDataAsync(IDataReader reader)
        {
            var listGroupSumData = new List<WinlossAnalysisDTO>();
            while (reader.Read())
            {
                var data = new WinlossAnalysisDTO
                {
                    RowIndex = reader.ToInt("ridx"),
                    GroupBy = reader.ToString("GroupBy"),
                };
                ReadCommonField(reader, data);
                listGroupSumData.Add(data);
            }

            return listGroupSumData;
        }

        private static SumTotalWinlossAnalysisDTO ReadSumTotalAsync(IDataReader reader)
        {
            reader.Read();
            var sumData = new SumTotalWinlossAnalysisDTO
            {
                TotalRow = reader.ToInt("TotalRow")
            };
            ReadCommonField(reader, sumData);

            return sumData;
        }

        private static void ReadCommonField(IDataReader reader, WinlossAnalysisDTO data)
        {
            data.BetCount = reader.ToInt("BetCount");
            data.Turnover = reader.ToDecimal("Turnover");
            data.NetTurnover = reader.ToDecimal("NetTurnover");
            data.MemWinloss = reader.ToDecimal("MemWinloss");
            data.MemComm = reader.ToDecimal("MemComm");
            data.LicWinloss = reader.ToDecimal("LicWinloss");
            data.LicComm = reader.ToDecimal("LicComm");
            data.AmtDueToBA = reader.ToDecimal("AmtDueToBA");
            data.BetEffective = reader.ToDecimal("BetEffective");
            data.Margin = reader.ToFloat("Margin");
            data.BaseTurnover = reader.ToDecimal("BaseTurnover");
            data.BaseNetTurnover = reader.ToDecimal("BaseNetTurnover");
            data.BaseMemWinloss = reader.ToDecimal("BaseMemWinloss");
            data.BaseMemComm = reader.ToDecimal("BaseMemComm");
            data.BaseLicWinloss = reader.ToDecimal("BaseLicWinloss");
            data.BaseLicComm = reader.ToDecimal("BaseLicComm");
            data.BaseAmtDueToBA = reader.ToDecimal("BaseAmtDueToBA");
            data.BaseBetEffective = reader.ToDecimal("BaseBetEffective");
        }
    }
}