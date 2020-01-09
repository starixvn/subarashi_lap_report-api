using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Data;

namespace Fanex.Athena.Report.ByPlayer.Extensions
{
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

            foreach (var item in metaModel.ProductOptions)
            {
                db.SetParameterValue("show" + item.Code, item.Status);
            }
        }
    }
}