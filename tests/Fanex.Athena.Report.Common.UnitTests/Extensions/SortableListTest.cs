using System.Collections.Generic;
using Fanex.Athena.Report.Common.Extensions;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Extensions
{
    public class SortableListTest
    {
        [Fact]
        public void SortByDictionary_AscendingDirection_ListSortedCorrectly()
        {
            // Arrange
            var firstObject = new Person { Id = 1, Name = "Walter", Children = CreateData("Ariana", "Selena", "Kross") };
            var secondObject = new Person { Id = 2, Name = "Ralph", Children = CreateData("Kyle", "Selena", "Alex") };
            var dataList = new SortableList<Person> { firstObject, secondObject };

            // Action
            dataList.SortByDictionary(10, "Children", true);

            // Assert
            Assert.Equal(firstObject, dataList[0]);
            Assert.Equal(secondObject, dataList[1]);
        }

        [Fact]
        public void SortByDictionary_DescendingDirection_ListSortedCorrectly()
        {
            // Arrange
            var firstObject = new Person { Id = 1, Name = "Walter", Children = CreateData("Ariana", "Selena", "Kross") };
            var secondObject = new Person { Id = 2, Name = "Ralph", Children = CreateData("Kyle", "Selena", "Alex") };
            var dataList = new SortableList<Person> { firstObject, secondObject };

            // Action
            dataList.SortByDictionary(10, "Children", false);

            // Assert
            Assert.Equal(firstObject, dataList[1]);
            Assert.Equal(secondObject, dataList[0]);
        }

        [Fact]
        public void Sort_AscendingDirection_ListSortedCorrectly()
        {
            // Arrange
            var firstObject = new Person { Id = 1, Name = "Walter" };
            var secondObject = new Person { Id = 2, Name = "Ralph" };
            var dataList = new SortableList<Person> { firstObject, secondObject };

            // Action
            dataList.Sort("Name", true);

            // Assert
            Assert.Equal(firstObject, dataList[1]);
            Assert.Equal(secondObject, dataList[0]);
        }

        [Fact]
        public void Sort_DescendingDirection_ListSortedCorrectly()
        {
            // Arrange
            var firstObject = new Person { Id = 1, Name = "Walter" };
            var secondObject = new Person { Id = 2, Name = "Ralph" };
            var dataList = new SortableList<Person> { firstObject, secondObject };

            // Action
            dataList.Sort("Name", false);

            // Assert
            Assert.Equal(firstObject, dataList[0]);
            Assert.Equal(secondObject, dataList[1]);
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