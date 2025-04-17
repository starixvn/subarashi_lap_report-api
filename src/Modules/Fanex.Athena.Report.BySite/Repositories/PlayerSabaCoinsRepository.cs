namespace Fanex.Athena.Report.BySite.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Common.Extensions;
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

        private static IEnumerable<PlayerSabaCoins> ReadMainData(
            IDataReader reader)
        {
            var dataList = new List<PlayerSabaCoins>();

            while (reader.Read())
            {
                var item = new PlayerSabaCoins
                {
                    CustId = reader.GetVal<string>("custId"),
                    Username = reader.GetVal<string>("username"),
                    ExternalId = reader.GetVal<string>("nickname"),
                    OpeningBalance = reader.GetVal<decimal>("OpeningBalance"),
                    Turnover = reader.GetVal<decimal>("Turnover"),
                    Outstanding = reader.GetVal<decimal>("Outstanding"),
                    WinLoss = reader.GetVal<decimal>("WinLoss"),
                    ClosingBalance = reader.GetVal<decimal>("ClosingBalance")
                };

                dataList.Add(item);
            }

            return dataList;
        }

        private static IEnumerable<PlayerSabaCoins> ReadExtraInfoData(
            IDataReader reader)
        {
            var dataList = new List<PlayerSabaCoins>();

            while (reader.Read())
            {
                var item = new PlayerSabaCoins
                {
                    CustId = reader.GetVal<string>("custId"),
                    RewardPromotionAmt = reader.GetVal<decimal>("RewardPromotionAmt"),
                    ExchangeAmt = reader.GetVal<decimal>("ExchangeAmt")
                };

                dataList.Add(item);
            }

            return dataList;
        }
    }
}