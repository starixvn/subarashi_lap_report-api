using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Models.DTOs
{
    public class CheckedItem
    {
        public string Name { get; set; }

        public int ID { get; set; }

        public string Value { get; set; }

        public bool Checked { get; set; }

        public IEnumerable<CheckedItem> Children { get; set; }
    }
}
