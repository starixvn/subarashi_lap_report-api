using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class TreeViewItem <TParent, TChildern>
    {
        public TParent Item { get; set; }

        public IEnumerable<TChildern> Childern { get; set; }
    }
}
