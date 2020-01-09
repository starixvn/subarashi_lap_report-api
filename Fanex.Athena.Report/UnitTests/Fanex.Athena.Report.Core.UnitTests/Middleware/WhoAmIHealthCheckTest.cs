using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Middleware;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Middleware
{
    public class WhoAmIHealthCheckTest
    {
        [Fact]
        public async Task CheckHealthAsync_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var healthCheck = new WhoAmIHealthCheck();

            // Action
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            Assert.Equal("Server Info", result.Description);
            Assert.NotEmpty(result.Data);
        }
    }
}