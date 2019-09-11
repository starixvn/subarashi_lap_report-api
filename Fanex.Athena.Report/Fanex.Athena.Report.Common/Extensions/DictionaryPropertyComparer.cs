namespace Fanex.Athena.Report.Common.Extensions
{
    using System.Collections;
    using System.ComponentModel;

    public class DictionaryPropertyComparer<T> : PropertyComparer<T>
    {
        private readonly object _key;

        public DictionaryPropertyComparer(object key, string property, ListSortDirection direction)
            : base(property, direction)
        {
            this._key = key;
        }

        public DictionaryPropertyComparer(
            object key,
            string dicProperty,
            ListSortDirection dicDirection,
            string propertyName,
            ListSortDirection propDirection)
            : base(dicProperty, dicDirection, propertyName, propDirection)
        {
            this._key = key;
        }

        #region IComparer<T>

        public override int Compare(T x, T y)
        {
            var firstValue = (GetPropertyValue(x, FirstPropertyName) as IDictionary)[this._key];
            var secondValue = (GetPropertyValue(y, FirstPropertyName) as IDictionary)[this._key];

            var result = CompareValues(firstValue, secondValue, FirstDirection);

            if (result != 0 || string.IsNullOrEmpty(SecondPropertyName))
            {
                return result;
            }

            firstValue = GetPropertyValue(x, SecondPropertyName);
            secondValue = GetPropertyValue(y, SecondPropertyName);

            return CompareValues(firstValue, secondValue, SecondDirection);
        }

        #endregion IComparer<T>
    }
}
