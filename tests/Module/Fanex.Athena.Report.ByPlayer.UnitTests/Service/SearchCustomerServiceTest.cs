namespace Fanex.Athena.Report.ByPlayer.UnitTests.Service;

using System.Threading.Tasks;
using ByPlayer.Service;
using Data;
using Moq;
using Xunit;

public class SearchCustomerServiceTest
{
    private readonly Mock<IObjectDb> _objectDb;

    public SearchCustomerServiceTest()
    {
        DbSettingProviderManager
            .StartTestingSession()
            .Run();

        _objectDb = new Mock<IObjectDb>();
    }

    [Fact]
    public async Task SearchCustomers_WhenCall_ParametersPassedCorrectly()
    {
        // Arrange
        const int userId = 1;
        const string userName = "test";
        const string agenList = "agentList";
        const bool isIncludeDormant = true;

        var service = CreateService();

        // Action
        await service.SearchCustomers(userId, userName, agenList, isIncludeDormant);

        // Assert
        _objectDb.Verify(db => db.SetParameterValue("userid", userId), Times.Once);
        _objectDb.Verify(db => db.SetParameterValue("username", userName), Times.Once);
        _objectDb.Verify(db => db.SetParameterValue("AgentList", agenList), Times.Once);
        _objectDb.Verify(db => db.SetParameterValue("IsIncludeDormant", isIncludeDormant), Times.Once);
    }

    private SearchCustomerService CreateService()
    {
        SetupObjectDb();

        return new SearchCustomerService();
    }

    private void SetupObjectDb()
    {
        ObjectDbFactory.SwitchToTestingMode();
        ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
    }
}