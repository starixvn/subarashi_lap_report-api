using Fanex.Athena.Report.Core.Attributes;
using System;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Attributes
{
    public class BearerAuthorizeAttributeTests
    {
        [Fact]
        public void BearerAuthorizeAttribute_WithNoRoles_SetsBearerAuthenticationScheme()
        {
            var attribute = new BearerAuthorizeAttribute();

            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Null(attribute.Roles);
        }

        [Fact]
        public void BearerAuthorizeAttribute_WithSingleRole_SetsRoleAndBearerScheme()
        {
            var attribute = new BearerAuthorizeAttribute("Admin");

            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin", attribute.Roles);
        }

        [Fact]
        public void BearerAuthorizeAttribute_WithMultipleRoles_JoinsRolesWithComma()
        {
            var attribute = new BearerAuthorizeAttribute("Admin", "User", "Manager");

            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin,User,Manager", attribute.Roles);
        }

        [Fact]
        public void BearerAuthorizeAttribute_WithEmptyRoleArray_SetsBearerSchemeOnly()
        {
            var attribute = new BearerAuthorizeAttribute(new string[0]);

            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Null(attribute.Roles);
        }

        [Fact]
        public void BearerAuthorizeAttribute_WithNullRoles_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BearerAuthorizeAttribute(null));
        }

        [Theory]
        [InlineData("Role1")]
        [InlineData("SuperAdmin")]
        [InlineData("ReadOnly")]
        public void BearerAuthorizeAttribute_WithSingleRole_PreservesRoleName(string roleName)
        {
            var attribute = new BearerAuthorizeAttribute(roleName);

            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal(roleName, attribute.Roles);
        }

        [Fact]
        public void BearerAuthorizeAttribute_AttributeUsage_IsConfiguredCorrectly()
        {
            var type = typeof(BearerAuthorizeAttribute);
            var attributeUsage = (System.AttributeUsageAttribute)type.GetCustomAttributes(typeof(System.AttributeUsageAttribute), false)[0];

            Assert.True(attributeUsage.AllowMultiple);
            Assert.True(attributeUsage.Inherited);
            Assert.Equal(System.AttributeTargets.Class | System.AttributeTargets.Method, attributeUsage.ValidOn);
        }
    }
}