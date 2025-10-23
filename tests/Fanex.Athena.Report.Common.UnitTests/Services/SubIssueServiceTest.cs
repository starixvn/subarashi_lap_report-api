using AutoFixture;
using Fanex.Athena.Report.Common.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Services
{
    public class SubIssueServiceTest
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMemoryCache> _memoryCache;

        public SubIssueServiceTest()
        {
            _fixture = new Fixture();
            _memoryCache = new Mock<IMemoryCache>();
        }

        [Fact]
        public void GetSubIssueList_WithValidData_ReturnsSubIssueList()
        {
            // Arrange
            var expectedResult = _fixture.Create<IDictionary<int, string>>();
            object outValue = expectedResult;
            _memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var service = new SubIssueService(_memoryCache.Object);

            // Act
            var result = service.GetSubIssueList();

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}