namespace Fanex.Athena.Report.BySite.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;

    public static class DailyBetsMetaModelExtension
    {
        public static string CurrencyListToXml(this DailyBetsMetaModel item)
        {
            return $"<ROOT>{string.Join("", item.CurrencyList.Select(i => $"<CR Id=\"{i.ID}\" Code=\"{i.Name}\" />"))}</ROOT>";

        }

        public static void InjectToDbParameter(this DailyBetsMetaModel metaModel, IObjectDb db)
        {
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("ProductCodes", metaModel.ProductCodeList);
            db.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml());
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("PageSize", metaModel.PageSize);

            if (!string.IsNullOrWhiteSpace(metaModel.SortBy) && !string.IsNullOrWhiteSpace(metaModel.SortOrder))
            {
                db.SetParameterValue("SortBy", metaModel.SortBy);
                db.SetParameterValue("SortOrder", metaModel.SortOrder);
            }
        }

        public static readonly IDictionary<string, Func<string>> GetProductName = new Dictionary<string, Func<string>>
            {
                { "SB", () => ViewResources.SB },
                { "NG", () => ViewResources.NG },
                { "CS", () => ViewResources.CS },
                { "LC", () => ViewResources.LC },
            };

        public static readonly IDictionary<int, Func<string>> GetStatusName = new Dictionary<int, Func<string>>
            {
                { 0, () => ViewResources.Draw },
                { 1, () => ViewResources.Won },
                { -1, () => ViewResources.Lose }
            };
    }
}
