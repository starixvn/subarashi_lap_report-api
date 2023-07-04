using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class TreeViewItem <TParent, TChildren>
    {
        public TParent KeyItem { get; set; }

        public IEnumerable<TChildren> Children { get; set; }
    }
}
