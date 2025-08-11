namespace Fanex.Athena.Report.ByPlayer.Extensions
{
    using System.Linq;
    using Data;
    using MetaModels;

    public static class CustomerBalanceMetaModelExtension
    {
        public static void InjectToDbParameter(this CustomerBalanceMetaModel metaModel, IObjectDb db)
        {
            db.SetParameterValue("fromdate", metaModel.FromDate);
            db.SetParameterValue("todate", metaModel.ToDate);
            db.SetParameterValue("topcount", metaModel.TopCount);
            db.SetParameterValue("orderitem", metaModel.OrderItem);
            db.SetParameterValue("pagesize", metaModel.PageSize);
            db.SetParameterValue("pageindex", metaModel.PageIndex);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("ProductList", string.Join(",", metaModel.ProductOptions.Where(_ => _.Status && _.Code != "BT" && _.Code != "CO").Select(_ => _.Code)));

            foreach (var item in metaModel.ProductOptions)
            {
                if (item.Code == "BT" || item.Code == "CO")
                {
                    db.SetParameterValue("show" + item.Code, item.Status);
                }
            }
        }
    }
}