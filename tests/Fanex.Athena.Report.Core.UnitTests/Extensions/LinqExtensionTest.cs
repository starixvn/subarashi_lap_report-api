namespace Fanex.Athena.Report.Core.UnitTests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Extensions;
    using Xunit;

    public class LinqExtensionTest
    {
        [Fact]
        public void LeftOuterJoin_WithMatchingItems_ReturnsJoinedResults()
        {
            // Arrange
            var left = new List<Person>
            {
                new Person { Id = 1, Name = "John" },
                new Person { Id = 2, Name = "Jane" }
            };

            var right = new List<Address>
            {
                new Address { PersonId = 1, Street = "123 Main St" },
                new Address { PersonId = 2, Street = "456 Oak Ave" }
            };

            // Act
            var result = left.LeftOuterJoin(
                right,
                p => p.Id,
                a => a.PersonId,
                (person, address) => new { PersonName = person.Name, address?.Street }
            ).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].PersonName);
            Assert.Equal("123 Main St", result[0].Street);
            Assert.Equal("Jane", result[1].PersonName);
            Assert.Equal("456 Oak Ave", result[1].Street);
        }

        [Fact]
        public void LeftOuterJoin_WithUnmatchedLeftItems_ReturnsAllLeftItems()
        {
            // Arrange
            var left = new List<Person>
            {
                new Person { Id = 1, Name = "John" },
                new Person { Id = 2, Name = "Jane" },
                new Person { Id = 3, Name = "Bob" }
            };

            var right = new List<Address>
            {
                new Address { PersonId = 1, Street = "123 Main St" }
            };

            // Act
            var result = left.LeftOuterJoin(
                right,
                p => p.Id,
                a => a.PersonId,
                (person, address) => new { PersonName = person.Name, address?.Street }
            ).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("John", result[0].PersonName);
            Assert.Equal("123 Main St", result[0].Street);
            Assert.Equal("Jane", result[1].PersonName);
            Assert.Null(result[1].Street);
            Assert.Equal("Bob", result[2].PersonName);
            Assert.Null(result[2].Street);
        }

        [Fact]
        public void LeftOuterJoin_WithEmptyRightCollection_ReturnsLeftWithNullRightValues()
        {
            // Arrange
            var left = new List<Person>
            {
                new Person { Id = 1, Name = "John" }
            };

            var right = new List<Address>();

            // Act
            var result = left.LeftOuterJoin(
                right,
                p => p.Id,
                a => a.PersonId,
                (person, address) => new { PersonName = person.Name, address?.Street }
            ).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("John", result[0].PersonName);
            Assert.Null(result[0].Street);
        }

        [Fact]
        public void LeftOuterJoin_WithEmptyLeftCollection_ReturnsEmpty()
        {
            // Arrange
            var left = new List<Person>();
            var right = new List<Address>
            {
                new Address { PersonId = 1, Street = "123 Main St" }
            };

            // Act
            var result = left.LeftOuterJoin(
                right,
                p => p.Id,
                a => a.PersonId,
                (person, address) => new { PersonName = person.Name, address?.Street }
            ).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void LeftOuterJoin_WithMultipleMatchesInRight_ReturnsAllMatches()
        {
            // Arrange
            var left = new List<Person>
            {
                new Person { Id = 1, Name = "John" }
            };

            var right = new List<Address>
            {
                new Address { PersonId = 1, Street = "123 Main St" },
                new Address { PersonId = 1, Street = "456 Oak Ave" }
            };

            // Act
            var result = left.LeftOuterJoin(
                right,
                p => p.Id,
                a => a.PersonId,
                (person, address) => new { PersonName = person.Name, address?.Street }
            ).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("John", r.PersonName));
            Assert.Contains(result, r => r.Street == "123 Main St");
            Assert.Contains(result, r => r.Street == "456 Oak Ave");
        }

        private class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Address
        {
            public int PersonId { get; set; }
            public string Street { get; set; }
        }
    }
}