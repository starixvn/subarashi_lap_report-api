namespace Fanex.Athena.Report.BySite.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Core.Repositories.Abstraction;
    using Data;
    using DTOs.WinlossAnalysis;
    using Interfaces;
    using MetaModels;

    public class WinlossAnalysisRepository : IWinlossAnalysisRepository
    {
        private const int SPTIMEOUT = 180;

        private readonly IDbFactory _dbFactory;

        public WinlossAnalysisRepository(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public Task<WinlossAnalysisAggregateDTO> GetDataBySettlementTimeCriteria(WinlossAnalysisMetaModel metaModel)
        {
            const string spName = "LAP_Rpt_WinlossAnalysis_SettleTimeSel";

            using var objectDb = _dbFactory.GetObjectDb(spName, SPTIMEOUT);
            AddCommonParameter(objectDb, metaModel);
            AddPlayerParameter(objectDb, metaModel);
            objectDb.SetParameterValue("HourParts", metaModel.HourParts);

            return ReadDataAsync(objectDb, metaModel);
        }

        public Task<WinlossAnalysisAggregateDTO> GetDataByAgencyCriteria(WinlossAnalysisMetaModel metaModel)
        {
            const string spName = "LAP_Rpt_WinlossAnalysis_AgencySel";

            using var objectDb = _dbFactory.GetObjectDb(spName, SPTIMEOUT);
            AddCommonParameter(objectDb, metaModel);

            return ReadDataAsync(objectDb, metaModel);
        }

        public Task<WinlossAnalysisAggregateDTO> GetDataByPlayerCriteria(WinlossAnalysisMetaModel metaModel)
        {
            const string spName = "LAP_Rpt_WinlossAnalysis_PlayerSel";

            using var objectDb = _dbFactory.GetObjectDb(spName, SPTIMEOUT);
            AddCommonParameter(objectDb, metaModel);
            AddPlayerParameter(objectDb, metaModel);

            return ReadDataAsync(objectDb, metaModel);
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

        private async Task<WinlossAnalysisAggregateDTO> ReadDataAsync(IObjectDb objectDb, WinlossAnalysisMetaModel metaModel)
        {
            var result = new WinlossAnalysisAggregateDTO();

            using (var reader = await objectDb.ExecuteReaderAsync())
            {
                result.MainData = ReadMainDataAsync(reader, metaModel);
                reader.NextResult();
                result.GroupSumData = ReadGroupSumDataAsync(reader);
                reader.NextResult();
                result.SumTotal = ReadSumTotalAsync(reader);
            }

            return result;
        }

        private static IEnumerable<WinlossAnalysisDTO> ReadMainDataAsync(IDataReader reader, WinlossAnalysisMetaModel metaModel)
        {
            var listMainData = new List<WinlossAnalysisDTO>();

            while (reader.Read())
            {
                var data = new WinlossAnalysisDTO
                {
                    RowIndex = reader.GetVal<int>("ridx"),
                    GroupBy = reader.GetVal<string>("GroupBy"),
                    BreakBy = reader.GetVal<string>("BreakBy")
                };
                data.DisplayRowIndex = data.RowIndex;
                ReadCommonField(reader, data);
                if (metaModel.IsShowInOrginalCurrency)
                {
                    ReadOriginalField(reader, data);
                }
                else
                {
                    ReadBaseField(reader, data);
                }

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
                    RowIndex = reader.GetVal<int>("ridx"),
                    GroupBy = reader.GetVal<string>("GroupBy")
                };
                data.DisplayRowIndex = data.RowIndex;
                ReadCommonField(reader, data);
                ReadBaseField(reader, data);
                listGroupSumData.Add(data);
            }

            return listGroupSumData;
        }

        private static SumTotalWinlossAnalysisDTO ReadSumTotalAsync(IDataReader reader)
        {
            reader.Read();
            var sumData = new SumTotalWinlossAnalysisDTO
            {
                TotalRow = reader.GetVal<int>("TotalRow")
            };
            ReadCommonField(reader, sumData);
            ReadBaseField(reader, sumData);

            return sumData;
        }

        private static void ReadCommonField(IDataReader reader, WinlossAnalysisDTO data)
        {
            data.BetCount = reader.GetVal<int>("BetCount");
            data.Margin = reader.GetVal<float>("Margin");
        }

        private static void ReadOriginalField(IDataReader reader, WinlossAnalysisDTO data)
        {
            data.Turnover = reader.GetVal<decimal>("Turnover");
            data.NetTurnover = reader.GetVal<decimal>("NetTurnover");
            data.PlayerWinloss = reader.GetVal<decimal>("MemWinloss");
            data.PlayerComm = reader.GetVal<decimal>("MemComm");
            data.LicWinloss = reader.GetVal<decimal>("LicWinloss");
            data.LicComm = reader.GetVal<decimal>("LicComm");
            data.BetEffective = reader.GetVal<decimal>("BetEffective");
        }

        private static void ReadBaseField(IDataReader reader, WinlossAnalysisDTO data)
        {
            data.Turnover = reader.GetVal<decimal>("BaseTurnover");
            data.NetTurnover = reader.GetVal<decimal>("BaseNetTurnover");
            data.PlayerWinloss = reader.GetVal<decimal>("BaseMemWinloss");
            data.PlayerComm = reader.GetVal<decimal>("BaseMemComm");
            data.LicWinloss = reader.GetVal<decimal>("BaseLicWinloss");
            data.LicComm = reader.GetVal<decimal>("BaseLicComm");
            data.BetEffective = reader.GetVal<decimal>("BaseBetEffective");
        }
    }
}