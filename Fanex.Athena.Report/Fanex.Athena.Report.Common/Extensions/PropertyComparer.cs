namespace Fanex.Athena.Report.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class PropertyComparer<T> : IComparer<T>
    {
        public PropertyComparer(string property, ListSortDirection direction)
        {
            FirstPropertyName = property;
            FirstDirection = direction;
        }

        public PropertyComparer(string firstProperty, ListSortDirection firstDirection, string secondProperty, ListSortDirection secondDirection)
        {
            FirstPropertyName = firstProperty;
            FirstDirection = firstDirection;
            SecondPropertyName = secondProperty;
            SecondDirection = secondDirection;
        }

        protected string FirstPropertyName { get; set; }

        protected ListSortDirection FirstDirection { get; set; }

        protected string SecondPropertyName { get; set; }

        protected ListSortDirection SecondDirection { get; set; }

        #region IComparer<T>

        public virtual int Compare(T x, T y)
        {
            var firstValue = GetPropertyValue(x, FirstPropertyName);
            var secondValue = GetPropertyValue(y, FirstPropertyName);

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

        protected static int CompareAscending(object x, object y)
        {
            if (y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            var value = x as IComparable;

            if (value != null)
            {
                return value.CompareTo(y);
            }

            return x.Equals(y) ? 0 : string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        protected static int CompareDescending(object x, object y)
        {
            // Return result adjusted for ascending or descending sort order ie
            // multiplied by 1 for ascending or -1 for descending
            return CompareAscending(x, y) * -1;
        }

        protected static object GetPropertyValue(T value, string propertyName)
        {
            var property = value.GetType().GetProperty(propertyName);

            return property.GetValue(value, null);
        }

        protected static int CompareValues(object x, object y, ListSortDirection direction)
        {
            return direction == ListSortDirection.Ascending ?
                CompareAscending(x, y) :
                CompareDescending(x, y);
        }
    }
}
