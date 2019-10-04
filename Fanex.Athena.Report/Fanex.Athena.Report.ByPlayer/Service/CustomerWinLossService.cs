using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services;
using Fanex.Data;

namespace Fanex.Athena.Report.ByPlayer.Service
{
    public class CustomerWinLossService : ICustomerWinLossService
    {
        private const int PageSizeExcel = int.MaxValue - 1000;

        private readonly IBalanceUpService _balanceUpService;

        public CustomerWinLossService(
            IBalanceUpService balanceUpService)
        {
            _balanceUpService = balanceUpService;
        }

        public async Task<ResultModel<CustomerWinLoss>> GetDataList(CustomerWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLoss";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                await AddParametersAsync(db, metaModel).ConfigureAwait(false);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var dataList = new List<CustomerWinLoss>();
                    var index = ((metaModel.PageIndex - 1) * metaModel.PageSize) + 1;
                    while (reader.Read())
                    {
                        dataList.Add(ReadDataItem(reader, index++));
                    }

                    var result = new ResultModel<CustomerWinLoss>
                    {
                        DataList = dataList
                    };

                    if (dataList.Any() && reader.NextResult() && reader.Read())
                    {
                        result.TotalData = ReadTotalData(reader);
                        result.TotalRows = Convert.ToInt32(reader["TotalRows"]);
                        result.AdditionalInfo = Convert.ToString(reader["SumPromotionPrize"]);
                    }
                    else
                    {
                        result.TotalData = new CustomerWinLoss();
                    }

                    return result;
                }
            }
        }

        private static CustomerWinLoss ReadDataItem(IDataReader reader, int index)
        {
            return new CustomerWinLoss
            {
                Index = index,
                CustId = Convert.ToInt32(reader["CustID"]),
                ExternalId = Convert.ToString(reader["nickname"]),
                SystemId = Convert.ToString(reader["username"]),
                Turnover = Convert.ToDecimal(reader["turnover"]),
                TotalTransaction = Convert.ToInt32(reader["betcount"]),
                CustomerWL = Convert.ToDecimal(reader["memberwinlost"]),
                BetEffective = Convert.ToDecimal(reader["beteffective"]),
                PlayerComm = Convert.ToDecimal(reader["playercomm"]),
                CustomerTotalWL = Convert.ToDecimal(reader["customertotalwl"]),
                LicenseeWL = Convert.ToDecimal(reader["licenseewl"]),
                BAComm = Convert.ToDecimal(reader["commba"]),
                AmtDueToBA = Convert.ToDecimal(reader["amtduetoba"])
            };
        }

        private async Task AddParametersAsync(IObjectDb db, CustomerWinLossMetaModel metaModel)
        {
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            db.SetParameterValue("fromdate", metaModel.FromDate);
            db.SetParameterValue("todate", metaModel.ToDate);
            db.SetParameterValue("pagesize", metaModel.IsExportExcel ? PageSizeExcel : metaModel.PageSize);
            db.SetParameterValue("pageindex", metaModel.PageIndex);
            db.SetParameterValue("movebetsday", balanceUpInfo.MoveBetsDay);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("username", metaModel.Username);
            db.SetParameterValue("OrderItem", metaModel.OrderItem);
            db.SetParameterValue("IsAscSort", metaModel.IsAscSort);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            foreach (var item in metaModel.ProductOptions)
            {
                db.SetParameterValue("show" + item.Code, item.Status);
            }
        }

        private static CustomerWinLoss ReadTotalData(IDataReader reader)
        {
            return new CustomerWinLoss
            {
                Turnover = Convert.ToDecimal(reader["sumturnover"]),
                TotalTransaction = Convert.ToInt32(reader["sumbetcount"]),
                CustomerWL = Convert.ToDecimal(reader["summemberwinlost"]),
                PlayerComm = Convert.ToDecimal(reader["sumplayercomm"]),
                LicenseeWL = Convert.ToDecimal(reader["sumlicenseewl"]),
                BAComm = Convert.ToDecimal(reader["sumcommba"]),
                AmtDueToBA = Convert.ToDecimal(reader["sumamtduetoba"]),
                BetEffective = Convert.ToDecimal(reader["sumbeteffective"]),
                CustomerTotalWL = Convert.ToDecimal(reader["sumcustomertotalwl"])
            };
        }
    }
}