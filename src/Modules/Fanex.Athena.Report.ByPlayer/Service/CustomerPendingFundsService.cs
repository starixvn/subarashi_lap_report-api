namespace Fanex.Athena.Report.ByPlayer.Service
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Model.DTO;
    using Common.Model.ViewModel;
    using Data;
    using DTO;
    using Fanex.Common.Extensions;
    using Interfaces;
    using MetaModels;

    public class CustomerPendingFundsService : ICustomerPendingFundsService
    {
        public async Task<ReportResultModel<CustPendingFunds>> GetDataList(CustomerPendingFundsMetaModel metaModel)
        {
            const string UspName = "Shard_Lrf_Rpt_Outstanding";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                AddParameters(db, metaModel);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var dataList = new List<CustPendingFunds>();
                    var index = (metaModel.PageIndex - 1) * metaModel.PageSize + 1;
                    while (reader.Read())
                    {
                        dataList.Add(ReadData(reader, index++));
                    }

                    var result = new ReportResultModel<CustPendingFunds>
                    {
                        DataList = dataList
                    };

                    if (dataList.Any() && reader.NextResult() && reader.Read())
                    {
                        result.TotalData = new CustPendingFunds
                        {
                            BetCounts = reader.ToInt("SumBetCounts"),
                            PendingFunds = reader.ToDecimal("SumOutstanding")
                        };
                        result.TotalRows = reader.ToInt("TotalRows");
                    }
                    else
                    {
                        result.TotalData = new CustPendingFunds();
                    }

                    return result;
                }
            }
        }

        private static void AddParameters(IObjectDb db, CustomerPendingFundsMetaModel metaModel)
        {
            db.SetParameterValue("pagesize", metaModel.PageSize);
            db.SetParameterValue("pageindex", metaModel.PageIndex);
            db.SetParameterValue("OrderItem", metaModel.OrderItem);
            db.SetParameterValue("IsAscSort", metaModel.IsAscSort);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);

            AddProductCodeParameters(db, metaModel.ProductOptions.ToList());
        }

        private static void AddProductCodeParameters(IObjectDb db, ICollection<ProductOption> productOptions)
        {
            var productBa = productOptions.FirstOrDefault(_ => _.Code == "BA");
            if (productBa != null)
            {
                db.SetParameterValue("ShowBA", productBa.Status);
                productOptions.Remove(productBa);
            }

            db.SetParameterValue("ProductList", productOptions.ToSelectedProductCodeList());
        }

        private static CustPendingFunds ReadData(IDataReader reader, int index)
        {
            return new CustPendingFunds
            {
                Index = index,
                CustId = reader.ToInt("CustID"),
                ExternalId = reader.ToString("nickname"),
                SystemId = reader.ToString("username"),
                BetCounts = reader.ToInt("BetCounts"),
                PendingFunds = reader.ToDecimal("Outstanding")
            };
        }
    }
}