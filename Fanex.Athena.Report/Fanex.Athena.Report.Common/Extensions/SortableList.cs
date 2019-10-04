namespace Fanex.Athena.Report.Common.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class SortableList<T> : List<T>, ISortableList<T>
    {
        private string _propertyName;
        private bool _ascending;
        public SortableList()
        {

        }
        public SortableList(IEnumerable<T> collection) : base(collection)
        {

        }

        public void SortByDictionary(object key, string propertyName, bool ascending)
        {
            UpdateSortingData(propertyName, ascending);

            // Apply and set the sort, if items to sort
            var comparer = new DictionaryPropertyComparer<T>(key, _propertyName, ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.Sort(comparer);
        }

        public void SortByDictionary(object key, string dicPropertyName, bool dicAscending, string propName, bool propAscending)
        {
            UpdateSortingData(dicPropertyName, dicAscending);

            var comparer = new DictionaryPropertyComparer<T>(
                key,
                _propertyName,
                dicAscending ? ListSortDirection.Ascending : ListSortDirection.Descending,
                propName,
                propAscending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.Sort(comparer);
        }

        /// <summary>
        /// Sort list by property name.
        /// </summary>
        /// <param name="propertyName">Property name to sort.</param>
        /// <param name="ascending">Sort ascending or descending.</param>
        public void Sort(string propertyName, bool ascending)
        {
            UpdateSortingData(propertyName, ascending);

            // Apply and set the sort, if items to sort
            var comparer = new PropertyComparer<T>(propertyName, _ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.Sort(comparer);
        }

        public void Sort(string firstPropertyName, bool firstAscending, string secondPropertyName, bool secondAscending)
        {
            UpdateSortingData(firstPropertyName, firstAscending);

            var comparer = new PropertyComparer<T>(
                firstPropertyName,
                _ascending ? ListSortDirection.Ascending : ListSortDirection.Descending,
                secondPropertyName,
                secondAscending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.Sort(comparer);
        }

        private void UpdateSortingData(string propertyName, bool ascending)
        {
            // Flip the properties if the parameters are the same.
            if (_propertyName == propertyName && _ascending == ascending)
            {
                _ascending = !ascending;
            }
            else
            {
                // Else, new properties are set with the new values.
                _propertyName = propertyName;
                _ascending = ascending;
            }
        }
    }
}
