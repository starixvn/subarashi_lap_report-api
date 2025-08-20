namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Common.Services.Interfaces;
    using Data;
    using DTOs;
    using Interfaces;
    using MetaModels;

    public class PlayerSabaCoinsRepository : IPlayerSabaCoinsRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        public PlayerSabaCoinsRepository(IBalanceUpService balanceUpService)
        {
            _balanceUpService = balanceUpService;
        }

        public async Task<IEnumerable<PlayerSabaCoinsMain>> GetMainData(
            PlayerSabaCoinsMetaModel metaModel)
        {
            const string UspName = "LAP_Rpt_SabaCoins_Performance";
            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("Username", metaModel.Username);

            using var reader = await db.ExecuteReaderAsync();
            return ReadMainData(reader);
        }

        private static IEnumerable<PlayerSabaCoinsMain> ReadMainData(
            IDataReader reader)
        {
            var dataList = new List<PlayerSabaCoinsMain>();

            while (reader.Read())
            {
                var item = new PlayerSabaCoinsMain
                {
                    CustId = Convert.ToInt32(reader["custId"]),
                    SabaId = Convert.ToInt64(reader["sabaId"]),
                    Username = reader["username"].ToString(),
                    ExternalId = reader["nickname"].ToString(),
                    OpeningBalance = Convert.ToDecimal(reader["OpeningBalance"]),
                    Turnover = Convert.ToDecimal(reader["Turnover"]),
                    Outstanding = Convert.ToDecimal(reader["Outstanding"]),
                    WinLoss = Convert.ToDecimal(reader["WinLoss"]),
                    ClosingBalance = Convert.ToDecimal(reader["ClosingBalance"])
                };

                dataList.Add(item);
            }

            return dataList;
        }

        public async Task<IEnumerable<PlayerSabaCoinsExtraInfo>> GetExtraInfoData(
            PlayerSabaCoinsMetaModel metaModel)
        {
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync();

            const string UspName = "LAP_Rpt_SabaCoins_Performance_ExtraInfo";
            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("Username", metaModel.Username);
            db.SetParameterValue("movebetsday", balanceUpInfo.MoveBetsDay);

            using var reader = await db.ExecuteReaderAsync();
            return ReadExtraInfoData(reader);
        }

        private static IEnumerable<PlayerSabaCoinsExtraInfo> ReadExtraInfoData(
            IDataReader reader)
        {
            var dataList = new List<PlayerSabaCoinsExtraInfo>();

            while (reader.Read())
            {
                var item = new PlayerSabaCoinsExtraInfo
                {
                    CustId = Convert.ToInt32(reader["custId"]),
                    SabaId = Convert.ToInt64(reader["sabaId"]),
                    RewardPromotionAmt = Convert.ToDecimal(reader["RewardPromotionAmt"]),
                    ExchangeAmt = Convert.ToDecimal(reader["ExchangeAmt"])
                };

                dataList.Add(item);
            }

            return dataList;
        }
    }
}