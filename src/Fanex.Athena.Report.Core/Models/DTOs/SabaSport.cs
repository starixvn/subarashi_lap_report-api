using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Models.DTOs
{
    public class SabaSport
    {
        public int Id { get; set; }

        public int SportTypeId { get; set; }

        public string Name { get; set; }

        public IEnumerable<int> LeagueGroupIds { get; set; }
    }
}
