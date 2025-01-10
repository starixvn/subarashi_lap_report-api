using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BetList.Models.BetList
{
    public class BetListDataResultModel
    {
        [JsonProperty(PropertyName = "LeagueName")]
        public string LeagueName { get; set; }

        [JsonProperty(PropertyName = "Main")]
        public List<object> Main { get; set; }

        [JsonProperty(PropertyName = "Total")]
        public object Total { get; set; }
    }
}
