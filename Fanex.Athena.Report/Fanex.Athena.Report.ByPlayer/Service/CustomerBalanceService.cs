using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Extensions;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Data;

namespace Fanex.Athena.Report.ByPlayer.Service
{
    public class CustomerBalanceService : ICustomerBalanceService
    {
        public async Task<ResultModel<CustomerBalance>> GetDataList(CustomerBalanceMetaModel metaModel)
        {
            using (var db = ObjectDbFactory.CreateInstance("Lrf_Rpt_CustomerBalance"))
            {
                metaModel.InjectToDbParameter(db);

                var dataList = new List<CustomerBalance>();

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var index = ((metaModel.PageIndex - 1) * metaModel.PageSize) + 1;
                    while (reader.Read())
                    {
                        dataList.Add(ReadDataItem(reader, index));
                        index++;
                    }

                    var result = new ResultModel<CustomerBalance>
                    {
                        DataList = dataList
                    };

                    if (dataList.Any() && reader.NextResult() && reader.Read())
                    {
                        result.TotalData = ReadTotalData(reader);
                        result.TotalRows = Convert.ToInt32(reader["TotalRows"]);
                    }
                    else
                    {
                        result.TotalData = new CustomerBalance();
                    }

                    return result;
                }
            }
        }

        private static CustomerBalance ReadDataItem(IDataReader reader, int index)
        {
            return new CustomerBalance
            {
                Index = index,
                ExternalId = reader.GetVal<string>("nickname"),
                SystemId = reader.GetVal<string>("username"),
                OpeningBalance = reader.GetVal<decimal>("openingbalance"),
                Deposit = reader.GetVal<decimal>("deposit"),
                Withdrawal = reader.GetVal<decimal>("withdrawal"),
                Bonus = reader.GetVal<decimal>("bonus"),
                Adjustment = reader.GetVal<decimal>("adjustment"),
                Promotion = reader.GetVal<decimal>("promotion"),
                Turnover = reader.GetVal<decimal>("turnover"),
                DrawGame = reader.GetVal<decimal>("drawgame"),
                NetTurnover = reader.GetVal<decimal>("turnover") - reader.GetVal<decimal>("drawgame"),
                PendingFunds = reader.GetVal<decimal>("outstanding"),
                WinLoss = reader.GetVal<decimal>("winloss"),
                PlayerComm = reader.GetVal<decimal>("playercomm"),
                TotalWL = reader.GetVal<decimal>("winloss") + reader.GetVal<decimal>("playercomm"),
                AffiliateTransfer = reader.GetVal<decimal>("affiliatetransfer"),
                ClosingBalance = reader.GetVal<decimal>("closingbalance"),
                TurnoverBA = reader.GetVal<decimal>("TurnOverBA"),
                CommBA = reader.GetVal<decimal>("CommBA"),
                AmountDueToBA = reader.GetVal<decimal>("AmountDueToBA")
            };
        }

        private static CustomerBalance ReadTotalData(IDataReader reader)
        {
            return new CustomerBalance
            {
                OpeningBalance = reader.GetVal<decimal>("sumopeningbalance"),
                Deposit = reader.GetVal<decimal>("sumdeposit"),
                Withdrawal = reader.GetVal<decimal>("sumwithdrawal"),
                Bonus = reader.GetVal<decimal>("sumbonus"),
                Adjustment = reader.GetVal<decimal>("sumadjustment"),
                Promotion = reader.GetVal<decimal>("sumpromotion"),
                Turnover = reader.GetVal<decimal>("sumturnover"),
                DrawGame = reader.GetVal<decimal>("sumdrawgame"),
                NetTurnover = reader.GetVal<decimal>("sumturnover") - reader.GetVal<decimal>("sumdrawgame"),
                PendingFunds = reader.GetVal<decimal>("sumoutstanding"),
                WinLoss = reader.GetVal<decimal>("sumwinloss"),
                AffiliateTransfer = reader.GetVal<decimal>("sumaffiliatetransfer"),
                ClosingBalance = reader.GetVal<decimal>("sumclosingbalance"),
                TurnoverBA = reader.GetVal<decimal>("sumturnoverba"),
                CommBA = reader.GetVal<decimal>("sumcommba"),
                AmountDueToBA = reader.GetVal<decimal>("sumamountduetoba"),
                PlayerComm = reader.GetVal<decimal>("sumplayercomm"),
                TotalWL = reader.GetVal<decimal>("sumwinloss") + reader.GetVal<decimal>("sumplayercomm")
            };
        }
    }
}