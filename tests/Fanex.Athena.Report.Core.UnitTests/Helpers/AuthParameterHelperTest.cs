using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Core.Utils;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Helpers
{
    public class AuthParameterHelperTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void GenerateAgentListXml_WithCurrencyAndAgentList_ReturnsValidXml()
        {
            // Arrange
            var currencyList = new Dictionary<int, string>
            {
                { 1, "USD" },
                { 2, "EUR" }
            };
            var currencyAgentList = new Dictionary<int, IEnumerable<int>>
            {
                { 1, new[] { 101, 102 } },
                { 2, new[] { 201, 202 } }
            };

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyList, currencyAgentList);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"102\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"201\" Currency=\"2\"", result);
            Assert.Contains("AgentId=\"202\" Currency=\"2\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithEmptyLists_ReturnsEmptyRootXml()
        {
            // Arrange
            var currencyList = new Dictionary<int, string>();
            var currencyAgentList = new Dictionary<int, IEnumerable<int>>();

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyList, currencyAgentList);

            // Assert
            Assert.Equal("<ROOT></ROOT>", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithNonMatchingCurrencies_ReturnsEmptyRootXml()
        {
            // Arrange
            var currencyList = new Dictionary<int, string>
            {
                { 1, "USD" }
            };
            var currencyAgentList = new Dictionary<int, IEnumerable<int>>
            {
                { 2, new[] { 201, 202 } }
            };

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyList, currencyAgentList);

            // Assert
            Assert.Equal("<ROOT></ROOT>", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithSingleCurrencyId_ReturnsValidXml()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101, 102 } },
                { 2, new[] { 201, 202 } }
            };
            const int currencyId = 1;

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyId);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"102\" Currency=\"1\"", result);
            Assert.DoesNotContain("Currency=\"2\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithNegativeCurrencyId_TreatsAsAllCurrencies()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { 2, new[] { 201 } }
            };
            const int negativeCurrencyId = -1;

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, negativeCurrencyId);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"201\" Currency=\"2\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithAllCurrencyId_ReturnsAllCurrencies()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { 2, new[] { 201 } },
                { 3, new[] { 301 } }
            };
            var currencyIdList = new[] { 0 }; // 0 means all currencies

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"201\" Currency=\"2\"", result);
            Assert.Contains("AgentId=\"301\" Currency=\"3\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithSpecificCurrencyIdList_ReturnsOnlySpecifiedCurrencies()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { 2, new[] { 201 } },
                { 3, new[] { 301 } }
            };
            var currencyIdList = new[] { 1, 3 };

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, false);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"301\" Currency=\"3\"", result);
            Assert.DoesNotContain("Currency=\"2\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithExcludeTestDataTrue_ExcludesUusCurrency()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { CurrencyType.Uus, new[] { 999 } }, // Test currency
                { 3, new[] { 301 } }
            };
            var currencyIdList = new[] { 0 }; // All currencies
            const bool isExcludeTestData = true;

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, isExcludeTestData);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"301\" Currency=\"3\"", result);
            Assert.DoesNotContain($"Currency=\"{CurrencyType.Uus}\"", result);
            Assert.DoesNotContain("AgentId=\"999\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithExcludeTestDataFalse_IncludesUusCurrency()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { CurrencyType.Uus, new[] { 999 } }, // Test currency
                { 3, new[] { 301 } }
            };
            var currencyIdList = new[] { 0 }; // All currencies
            const bool isExcludeTestData = false;

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, isExcludeTestData);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"301\" Currency=\"3\"", result);
            Assert.Contains($"Currency=\"{CurrencyType.Uus}\"", result);
            Assert.Contains("AgentId=\"999\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithNonExistentCurrencyIds_IgnoresThemGracefully()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { 2, new[] { 201 } }
            };
            var currencyIdList = new[] { 1, 999, 2, 888 }; // 999 and 888 don't exist

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, false);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"201\" Currency=\"2\"", result);
            Assert.DoesNotContain("Currency=\"999\"", result);
            Assert.DoesNotContain("Currency=\"888\"", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithEmptyAgentList_ReturnsEmptyRootXml()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new int[0] }
            };
            var currencyIdList = new[] { 1 };

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, false);

            // Assert
            Assert.Equal("<ROOT></ROOT>", result);
        }

        [Fact]
        public void GenerateAgentListXml_WithLargeAgentList_HandlesAllAgents()
        {
            // Arrange
            var largeAgentList = Enumerable.Range(1, 100).ToArray();
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, largeAgentList }
            };
            var currencyIdList = new[] { 1 };

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, false);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            
            // Verify first and last agents are included
            Assert.Contains("AgentId=\"1\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"100\" Currency=\"1\"", result);
            
            // Count agent entries - should be 100
            var agentCount = result.Split("Agent ").Length - 1;
            Assert.Equal(100, agentCount);
        }

        [Fact]
        public void GenerateAgentListXml_WithMixedAllAndSpecificCurrencies_PrefersAllCurrencies()
        {
            // Arrange
            var currencyAgentList = new Dictionary<int, int[]>
            {
                { 1, new[] { 101 } },
                { 2, new[] { 201 } },
                { 3, new[] { 301 } }
            };
            var currencyIdList = new[] { 0, 1, 2 }; // Contains 0 (all) plus specific IDs

            // Act
            var result = AuthParameterHelper.GenerateAgentListXml(currencyAgentList, currencyIdList, false);

            // Assert
            Assert.StartsWith("<ROOT>", result);
            Assert.EndsWith("</ROOT>", result);
            
            // Should include all currencies since 0 is in the list
            Assert.Contains("AgentId=\"101\" Currency=\"1\"", result);
            Assert.Contains("AgentId=\"201\" Currency=\"2\"", result);
            Assert.Contains("AgentId=\"301\" Currency=\"3\"", result);
        }
    }
}