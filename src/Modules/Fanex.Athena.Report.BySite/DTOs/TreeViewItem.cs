namespace Fanex.Athena.Report.BySite.DTOs
{
    using System.Collections.Generic;

    public class TreeViewItem<TParent, TChildren>
    {
        public TParent KeyItem { get; set; }

        public IEnumerable<TChildren> Children { get; set; }
    }
}