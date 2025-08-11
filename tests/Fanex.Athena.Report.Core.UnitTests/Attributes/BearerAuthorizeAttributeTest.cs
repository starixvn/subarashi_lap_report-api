namespace Fanex.Athena.Report.Core.UnitTests.Attributes
{
    using System;
    using Core.Attributes;
    using Xunit;

    public class BearerAuthorizeAttributeTest
    {
        [Fact]
        public void Constructor_WithoutRoles_SetsBearerAuthenticationScheme()
        {
            // Act
            var attribute = new BearerAuthorizeAttribute();

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Null(attribute.Roles);
        }

        [Fact]
        public void Constructor_WithSingleRole_SetsRoleAndBearerScheme()
        {
            // Arrange
            const string role = "Admin";

            // Act
            var attribute = new BearerAuthorizeAttribute(role);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal(role, attribute.Roles);
        }

        [Fact]
        public void Constructor_WithMultipleRoles_CombinesRolesWithCommas()
        {
            // Arrange
            var roles = new[] { "Admin", "User", "Manager" };

            // Act
            var attribute = new BearerAuthorizeAttribute(roles);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin,User,Manager", attribute.Roles);
        }

        [Fact]
        public void Constructor_WithEmptyRolesArray_DoesNotSetRoles()
        {
            // Arrange
            var roles = new string[0];

            // Act
            var attribute = new BearerAuthorizeAttribute(roles);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Null(attribute.Roles);
        }

        [Fact]
        public void Constructor_WithNullInRoles_HandlesGracefully()
        {
            // Arrange
            var roles = new[] { "Admin", null, "User" };

            // Act
            var attribute = new BearerAuthorizeAttribute(roles);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin,,User", attribute.Roles);
        }

        [Fact]
        public void Constructor_WithDuplicateRoles_IncludesDuplicates()
        {
            // Arrange
            var roles = new[] { "Admin", "Admin", "User" };

            // Act
            var attribute = new BearerAuthorizeAttribute(roles);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin,Admin,User", attribute.Roles);
        }

        [Fact]
        public void Constructor_WithWhitespaceRoles_PreservesWhitespace()
        {
            // Arrange
            var roles = new[] { "Admin", " User ", "Manager" };

            // Act
            var attribute = new BearerAuthorizeAttribute(roles);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin, User ,Manager", attribute.Roles);
        }

        [Fact]
        public void Attribute_HasCorrectAttributeUsage()
        {
            // Arrange
            var attributeType = typeof(BearerAuthorizeAttribute);

            // Act
            var attributes = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var attributeUsage = attributes[0] as AttributeUsageAttribute;

            // Assert
            Assert.NotNull(attributeUsage);
            Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Class));
            Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Method));
            Assert.True(attributeUsage.AllowMultiple);
            Assert.True(attributeUsage.Inherited);
        }

        [Fact]
        public void Attribute_IsSealed()
        {
            // Arrange & Act
            var attributeType = typeof(BearerAuthorizeAttribute);

            // Assert
            Assert.True(attributeType.IsSealed);
        }

        [Fact]
        public void Constructor_WithSpecialCharactersInRoles_HandlesCorrectly()
        {
            // Arrange
            var roles = new[] { "Admin-Level1", "User@Domain", "Manager.Team" };

            // Act
            var attribute = new BearerAuthorizeAttribute(roles);

            // Assert
            Assert.Equal("Bearer", attribute.AuthenticationSchemes);
            Assert.Equal("Admin-Level1,User@Domain,Manager.Team", attribute.Roles);
        }
    }
}