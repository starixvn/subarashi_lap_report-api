namespace Fanex.Athena.Report.BySite.Repositories
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public class PlayerSabaCoinsRepository : IPlayerSabaCoinsRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        public PlayerSabaCoinsRepository(IBalanceUpService balanceUpService)
        {
            _balanceUpService = balanceUpService;
        }

        public async Task<IEnumerable<PlayerSabaCoins>> GetMainData(
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

        private static IEnumerable<PlayerSabaCoins> ReadMainData(
            IDataReader reader)
        {
            var dataList = new List<PlayerSabaCoins>();

            while (reader.Read())
            {
                var item = new PlayerSabaCoins
                {
                    CustId = reader["custId"].ToString(),
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

        public async Task<IEnumerable<PlayerSabaCoins>> GetExtraInfoData(
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

        private static IEnumerable<PlayerSabaCoins> ReadExtraInfoData(
            IDataReader reader)
        {
            var dataList = new List<PlayerSabaCoins>();

            while (reader.Read())
            {
                var item = new PlayerSabaCoins
                {
                    CustId = reader["custId"].ToString(),
                    RewardPromotionAmt = Convert.ToDecimal(reader["RewardPromotionAmt"]),
                    ExchangeAmt = Convert.ToDecimal(reader["ExchangeAmt"])
                };

                dataList.Add(item);
            }

            return dataList;
        }
    }
}