using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class DataPermissionServiceTest
    {
        private readonly Mock<IContextService> _contextService;

        public DataPermissionServiceTest()
        {
            _contextService = new Mock<IContextService>();
        }

        [Fact]
        public void ClearDataByLimitPermission_WhenCall_ClearData()
        {
            //Arrange
            _contextService.Setup(x => x.IsSuperAdmin).Returns(false);
            _contextService.Setup(x => x.IsInRoles(FakeClassWithPermissions.RoleA)).Returns(true);
            _contextService.Setup(x => x.IsInRoles(FakeClassWithPermissions.RoleB)).Returns(false);
            var model = FakeHelper.Create<FakeClassWithPermissions>();
            var service = CreateService();

            //Action
            var result = service.ClearDataByLimitPermission(model);

            //Assert
            Assert.Null(result.RoleAData);
            Assert.Equal(model.RoleBData, result.RoleBData);
            Assert.Equal(model.DataWithNoPermissionLimit, result.DataWithNoPermissionLimit);
        }

        #region TestHelpers
        private IDataPermissionService CreateService()
        {
            return new DataPermissionService(_contextService.Object);
        }

        public class FakeClassWithPermissions
        {
            public const string RoleA = "RoleA";

            public const string RoleB = "RoleB";

            [DataPermission(Permission = RoleA)]
            public string RoleAData { get; set; }

            [DataPermission(Permission = RoleB)]
            public string RoleBData { get; set; }

            public string DataWithNoPermissionLimit { get; set; }
        }
        #endregion

    }


}
