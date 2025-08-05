using System;
using System.Collections.Generic;
using System.Linq;
using Fanex.Athena.Report.Core.Extensions;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Extensions
{
    public class GuardExtensionTest
    {
        [Fact]
        public void IsPresent_WithNonEmptyCollection_ReturnsTrue()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };

            // Act
            var result = collection.IsPresent();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPresent_WithEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new List<int>();

            // Act
            var result = collection.IsPresent();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPresent_WithNullCollection_ReturnsFalse()
        {
            // Arrange
            List<int> collection = null;

            // Act
            var result = collection.IsPresent();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPresent_WithPredicateAndMatchingItems_ReturnsTrue()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            Func<int, bool> predicate = x => x > 3;

            // Act
            var result = collection.IsPresent(predicate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPresent_WithPredicateAndNoMatchingItems_ReturnsFalse()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };
            Func<int, bool> predicate = x => x > 10;

            // Act
            var result = collection.IsPresent(predicate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPresent_WithPredicateAndEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new List<int>();
            Func<int, bool> predicate = x => x > 0;

            // Act
            var result = collection.IsPresent(predicate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPresent_WithPredicateAndNullCollection_ReturnsFalse()
        {
            // Arrange
            List<int> collection = null;
            Func<int, bool> predicate = x => x > 0;

            // Act
            var result = collection.IsPresent(predicate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPresent_WithStringCollection_ReturnsCorrectResult()
        {
            // Arrange
            var collection = new List<string> { "apple", "banana", "cherry" };
            Func<string, bool> predicate = s => s.StartsWith("b");

            // Act
            var result = collection.IsPresent(predicate);

            // Assert
            Assert.True(result);
        }
    }
}