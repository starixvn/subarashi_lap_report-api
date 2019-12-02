using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
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

        public async Task<ResultModel<CustomerWinLoss>> GetDataList(CustomerWinLossMetaModel metaModel, bool checkPromotionPrize = false)
        {
            const string UspName = "Lrf_Rpt_WinLoss";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                await AddParametersAsync(db, metaModel, checkPromotionPrize).ConfigureAwait(false);

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

                    var hasSummaryResult = reader.NextResult() && reader.Read();
                    result.AdditionalInfo = hasSummaryResult ? Convert.ToString(reader["PromotionPrize"]) : "0";

                    if (dataList.Any() && hasSummaryResult)
                    {
                        result.TotalData = ReadTotalData(reader);
                        result.TotalRows = Convert.ToInt32(reader["TotalRows"]);
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
                CustId = reader.GetVal<int>("CustID"),
                ExternalId = reader.GetVal<string>("nickname"),
                SystemId = reader.GetVal<string>("username"),
                Turnover = reader.GetVal<decimal>("turnover"),
                TotalTransaction = reader.GetVal<int>("betcount"),
                CustomerWL = reader.GetVal<decimal>("memberwinlost"),
                BetEffective = reader.GetVal<decimal>("beteffective"),
                PlayerComm = reader.GetVal<decimal>("playercomm"),
                CustomerTotalWL = reader.GetVal<decimal>("customertotalwl"),
                LicenseeWL = reader.GetVal<decimal>("licenseewl"),
                BAComm = reader.GetVal<decimal>("commba"),
                AmtDueToBA = reader.GetVal<decimal>("amtduetoba")
            };
        }

        private async Task AddParametersAsync(IObjectDb db, CustomerWinLossMetaModel metaModel, bool checkPromotionPrize)
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
            db.SetParameterValue("CheckPromotionPrize", checkPromotionPrize);
            foreach (var item in metaModel.ProductOptions)
            {
                db.SetParameterValue("show" + item.Code, item.Status);
            }
        }

        private static CustomerWinLoss ReadTotalData(IDataReader reader)
        {
            return new CustomerWinLoss
            {
                Turnover = reader.GetVal<decimal>("sumturnover"),
                TotalTransaction = reader.GetVal<int>("sumbetcount"),
                CustomerWL = reader.GetVal<decimal>("summemberwinlost"),
                PlayerComm = reader.GetVal<decimal>("sumplayercomm"),
                LicenseeWL = reader.GetVal<decimal>("sumlicenseewl"),
                BAComm = reader.GetVal<decimal>("sumcommba"),
                AmtDueToBA = reader.GetVal<decimal>("sumamtduetoba"),
                BetEffective = reader.GetVal<decimal>("sumbeteffective"),
                CustomerTotalWL = reader.GetVal<decimal>("sumcustomertotalwl")
            };
        }
    }
}