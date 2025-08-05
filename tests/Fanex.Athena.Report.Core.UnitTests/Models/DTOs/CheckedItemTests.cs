using Fanex.Athena.Report.Core.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    public class CheckedItemTests
    {
        [Fact]
        public void CheckedItem_Properties_CanBeSetAndRetrieved()
        {
            var childItem = new CheckedItem
            {
                Name = "Child Item",
                ID = 2,
                Value = "child_value",
                Checked = false
            };

            var checkedItem = new CheckedItem
            {
                Name = "Test Item",
                ID = 1,
                Value = "test_value",
                Checked = true,
                Children = new List<CheckedItem> { childItem }
            };

            Assert.Equal("Test Item", checkedItem.Name);
            Assert.Equal(1, checkedItem.ID);
            Assert.Equal("test_value", checkedItem.Value);
            Assert.True(checkedItem.Checked);
            Assert.NotNull(checkedItem.Children);
            Assert.Single(checkedItem.Children);
            Assert.Equal(childItem, checkedItem.Children.First());
        }

        [Fact]
        public void CheckedItem_DefaultValues_AreCorrect()
        {
            var checkedItem = new CheckedItem();

            Assert.Null(checkedItem.Name);
            Assert.Equal(0, checkedItem.ID);
            Assert.Null(checkedItem.Value);
            Assert.False(checkedItem.Checked);
            Assert.Null(checkedItem.Children);
        }

        [Fact]
        public void CheckedItem_WithNestedChildren_HandlesHierarchy()
        {
            var grandChildItem = new CheckedItem
            {
                Name = "Grand Child",
                ID = 3,
                Value = "grandchild_value",
                Checked = true
            };

            var childItem = new CheckedItem
            {
                Name = "Child Item",
                ID = 2,
                Value = "child_value",
                Checked = false,
                Children = new List<CheckedItem> { grandChildItem }
            };

            var parentItem = new CheckedItem
            {
                Name = "Parent Item",
                ID = 1,
                Value = "parent_value",
                Checked = true,
                Children = new List<CheckedItem> { childItem }
            };

            Assert.Single(parentItem.Children);
            var child = parentItem.Children.First();
            Assert.Equal("Child Item", child.Name);
            Assert.Single(child.Children);
            var grandChild = child.Children.First();
            Assert.Equal("Grand Child", grandChild.Name);
            Assert.True(grandChild.Checked);
        }

        [Fact]
        public void CheckedItem_WithEmptyChildren_HandlesCorrectly()
        {
            var checkedItem = new CheckedItem
            {
                Name = "Test Item",
                ID = 1,
                Value = "test_value",
                Checked = true,
                Children = new List<CheckedItem>()
            };

            Assert.NotNull(checkedItem.Children);
            Assert.Empty(checkedItem.Children);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CheckedItem_CheckedProperty_WorksCorrectly(bool isChecked)
        {
            var checkedItem = new CheckedItem
            {
                Checked = isChecked
            };

            Assert.Equal(isChecked, checkedItem.Checked);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void CheckedItem_IDProperty_AcceptsVariousValues(int id)
        {
            var checkedItem = new CheckedItem
            {
                ID = id
            };

            Assert.Equal(id, checkedItem.ID);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("simple")]
        [InlineData("complex_value_with_underscore")]
        [InlineData("Very Long Value With Spaces and Special Characters !@#$%")]
        public void CheckedItem_StringProperties_AcceptVariousValues(string value)
        {
            var checkedItem = new CheckedItem
            {
                Name = value,
                Value = value
            };

            Assert.Equal(value, checkedItem.Name);
            Assert.Equal(value, checkedItem.Value);
        }
    }
}