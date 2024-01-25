using System.Collections.Generic;
using Fanex.Athena.Report.Common.Extensions;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Extensions
{
    public class DictionaryPropertyComparerTest
    {
        [Theory]
        [InlineData(10, -1)]
        [InlineData(11, 0)]
        [InlineData(12, 1)]
        public void Compare_AscendingDirection_ReturnCorrecvalue(int keyValue, int expected)
        {
            // Arrange
            var firstObject = new Person { Id = 1, Name = "Walter", Children = CreateData("Ariana", "Selena", "Kross") };
            var secondObject = new Person { Id = 2, Name = "Ralph", Children = CreateData("Kyle", "Selena", "Alex") };
            var comparer = new DictionaryPropertyComparer<Person>(keyValue, "Children", System.ComponentModel.ListSortDirection.Ascending);

            // Action
            var result = comparer.Compare(firstObject, secondObject);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(11, 0)]
        [InlineData(12, -1)]
        public void Compare_DescendingDirection_ReturnCorrecvalue(int keyValue, int expected)
        {
            // Arrange
            var firstObject = new Person { Id = 1, Name = "Walter", Children = CreateData("Ariana", "Selena", "Kross") };
            var secondObject = new Person { Id = 2, Name = "Ralph", Children = CreateData("Kyle", "Selena", "Alex") };
            var comparer = new DictionaryPropertyComparer<Person>(keyValue, "Children", System.ComponentModel.ListSortDirection.Descending);

            // Action
            var result = comparer.Compare(firstObject, secondObject);

            // Assert
            Assert.Equal(expected, result);
        }

        private static IDictionary<int, string> CreateData(params string[] names)
        {
            var dictionary = new Dictionary<int, string>
            {
                { 10, names[0] },
                { 11, names[1] },
                { 12, names[2] }
            };

            return dictionary;
        }

        internal class Person
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string NickName { get; set; }

            public IDictionary<int, string> Children { get; set; }
        }
    }
}