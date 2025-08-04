using Fanex.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fanex.Athena.Report.Core.Helpers
{
    public static class AuthParameterHelper
    {
        public static string GenerateAgentListXml(IReadOnlyDictionary<int, string> currencyList, IDictionary<int, IEnumerable<int>> currencyAgentList)
        {
            var result = new StringBuilder("<ROOT>");
            var offeredCurrency = currencyAgentList.Where(e => currencyList.ContainsKey(e.Key));
            foreach (var item in offeredCurrency)
            {
                result.Append(GenerateAgentXmlBody(item.Key, item.Value));
            }

            result.Append("</ROOT>");
            return result.ToString();
        }

        public static string GenerateAgentListXml(IReadOnlyDictionary<int, int[]> currencyAgentList, int currencyId = 0, bool isExcludeTestData = false)
        {
            return GenerateAgentListXml(currencyAgentList, new[] { currencyId < 0 ? 0 : currencyId, }, isExcludeTestData);
        }

        public static string GenerateAgentListXml(IReadOnlyDictionary<int, int[]> currencyAgentList, IEnumerable<int> currencyIdList, bool isExcludeTestData = false)
        {
            const int AllCurrencyId = 0;
            var result = new StringBuilder("<ROOT>");

            if (isExcludeTestData && currencyAgentList.ContainsKey(CurrencyType.Uus))
            {
                currencyAgentList = currencyAgentList.Where(x => x.Key != CurrencyType.Uus)
                                                     .ToDictionary(x => x.Key, x => x.Value);
            }

            if (currencyIdList.Contains(AllCurrencyId))
            {
                foreach (var currency in currencyAgentList)
                {
                    result.Append(GenerateAgentXmlBody(currency.Key, currency.Value));
                }
            }
            else
            {
                var agentList = currencyIdList
                               .Where(currencyId => currencyAgentList.ContainsKey(currencyId))
                               .Select(currencyId => (currencyId, agents: currencyAgentList[currencyId]))
                               .Select(tuple => GenerateAgentXmlBody(tuple.currencyId, tuple.agents));

                result.Append(string.Concat(agentList));
            }

            result.Append("</ROOT>");
            return result.ToString();
        }



        private static StringBuilder GenerateAgentXmlBody(int currencyId, IEnumerable<int> agentList)
        {
            var result = new StringBuilder();

            foreach (var agentId in agentList)
            {
                result.AppendFormat("<Agent AgentId=\"{0}\" Currency=\"{1}\"/>", agentId, currencyId);
            }

            return result;
        }
    }
}
