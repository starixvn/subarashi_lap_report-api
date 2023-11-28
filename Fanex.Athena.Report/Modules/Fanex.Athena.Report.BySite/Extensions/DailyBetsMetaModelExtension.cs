namespace Fanex.Athena.Report.BySite.Extensions
{
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Resources;
    using Fanex.Common.Extensions;
    using Fanex.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DailyBetsMetaModelExtension
    {
        public static string CurrencyListToXml(this DailyBetsMetaModel item)
        {
            var selectedCurrencies = item.CurrencyIdList
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => int.Parse(_));
            return $"<ROOT>{string.Join("", selectedCurrencies.Select(id => $"<CR Id=\"{id}\" Code=\"{item.CurrencyList[id]}\" />"))}</ROOT>";
        }

        public static void InjectToDbParameter(this DailyBetsMetaModel metaModel, IObjectDb db)
        {
            db.SetParameterValue("LoginName", metaModel.LoginName);
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("ProductCodes", metaModel.ProductCodeList);
            db.SetParameterValue("GameCodes", metaModel.GameCodes);
            db.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml());
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("PageSize", metaModel.PageSize);

            if (!string.IsNullOrWhiteSpace(metaModel.SortBy) && !string.IsNullOrWhiteSpace(metaModel.SortOrder))
            {
                db.SetParameterValue("SortBy", metaModel.SortBy);
                db.SetParameterValue("SortOrder", metaModel.SortOrder);
            }
            if (metaModel.RefNo > 0 && string.Equals(metaModel.SiteName, "IvoryBet", StringComparison.InvariantCultureIgnoreCase))
            {
                db.SetParameterValue("SearchRefNo", metaModel.RefNo);
            }
        }

        public static IDictionary<string, Func<string>> GetProductName { get; set; } = new Dictionary<string, Func<string>>
        {
            { "SB", () => ViewResources.SB },
            { "NG", () => ViewResources.NG },
            { "CS", () => ViewResources.CS },
            { "LC", () => ViewResources.LC },
        };

        public static IDictionary<int, Func<string>> GetStatusName { get; set; } = new Dictionary<int, Func<string>>
        {
            { 0, () => ViewResources.Draw },
            { 1, () => ViewResources.Won },
            { -1, () => ViewResources.Lose }
        };
    }
}