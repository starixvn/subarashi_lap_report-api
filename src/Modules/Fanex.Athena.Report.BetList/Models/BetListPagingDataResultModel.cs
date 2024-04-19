using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BetList.Models
{
    public class BetListPagingDataResultModel
    {
        public int Page { get; set; }

        public int Total { get; set; }

        public int Records { get; set; }

        public List<object> Rows { get; set; }

        [JsonProperty(PropertyName = "userdata")]
        public object UserData { get; set; }
    }
}
