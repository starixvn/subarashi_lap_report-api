using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Extensions
{
    public interface ISortableList<out T> : IEnumerable<T>
    {
        void SortByDictionary(object key, string propertyName, bool ascending);


        void SortByDictionary(object key, string dicPropertyName, bool dicAscending, string propName, bool propAscending);

        void Sort(string propertyName, bool ascending);

        void Sort(string firstPropertyName, bool firstAscending, string secondPropertyName, bool secondAscending);

    }
}
