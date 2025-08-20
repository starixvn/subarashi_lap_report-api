namespace Fanex.Athena.Report.Core.UnitTests.Models.DTOs
{
    using Core.Models.DTOs;
    using Xunit;

    public class CustomerClassCategoryTest
    {
        [Fact]
        public void CustomerClass_GetSet_WorksCorrectly()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const int expectedCustomerClass = 5;

            // Act
            category.CustomerClass = expectedCustomerClass;

            // Assert
            Assert.Equal(expectedCustomerClass, category.CustomerClass);
        }

        [Fact]
        public void Category_GetSet_WorksCorrectly()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const int expectedCategory = 2;

            // Act
            category.Category = expectedCategory;

            // Assert
            Assert.Equal(expectedCategory, category.Category);
        }

        [Fact]
        public void CategoryName_GetSet_WorksCorrectly()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const string expectedCategoryName = "High Risk";

            // Act
            category.CategoryName = expectedCategoryName;

            // Assert
            Assert.Equal(expectedCategoryName, category.CategoryName);
        }

        [Fact]
        public void CategoryNameCN_GetSet_WorksCorrectly()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const string expectedCategoryNameCN = "高风险";

            // Act
            category.CategoryNameCN = expectedCategoryNameCN;

            // Assert
            Assert.Equal(expectedCategoryNameCN, category.CategoryNameCN);
        }

        [Fact]
        public void CustomerClassName_GetSet_WorksCorrectly()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const string expectedCustomerClassName = "VIP Level 1";

            // Act
            category.CustomerClassName = expectedCustomerClassName;

            // Assert
            Assert.Equal(expectedCustomerClassName, category.CustomerClassName);
        }

        [Fact]
        public void SabaRiskLevel_GetSet_WorksCorrectly()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const string expectedSabaRiskLevel = "Dangerous";

            // Act
            category.SabaRiskLevel = expectedSabaRiskLevel;

            // Assert
            Assert.Equal(expectedSabaRiskLevel, category.SabaRiskLevel);
        }

        [Fact]
        public void DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var category = new CustomerClassCategory();

            // Assert
            Assert.Equal(0, category.CustomerClass);
            Assert.Equal(0, category.Category);
            Assert.Null(category.CategoryName);
            Assert.Null(category.CategoryNameCN);
            Assert.Null(category.CustomerClassName);
            Assert.Null(category.SabaRiskLevel);
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const int expectedCustomerClass = 10;
            const int expectedCategory = 3;
            const string expectedCategoryName = "Premium";
            const string expectedCategoryNameCN = "优质客户";
            const string expectedCustomerClassName = "Premium Customer";
            const string expectedSabaRiskLevel = "VIP";

            // Act
            category.CustomerClass = expectedCustomerClass;
            category.Category = expectedCategory;
            category.CategoryName = expectedCategoryName;
            category.CategoryNameCN = expectedCategoryNameCN;
            category.CustomerClassName = expectedCustomerClassName;
            category.SabaRiskLevel = expectedSabaRiskLevel;

            // Assert
            Assert.Equal(expectedCustomerClass, category.CustomerClass);
            Assert.Equal(expectedCategory, category.Category);
            Assert.Equal(expectedCategoryName, category.CategoryName);
            Assert.Equal(expectedCategoryNameCN, category.CategoryNameCN);
            Assert.Equal(expectedCustomerClassName, category.CustomerClassName);
            Assert.Equal(expectedSabaRiskLevel, category.SabaRiskLevel);
        }

        [Fact]
        public void Properties_CanHandleEmptyStrings()
        {
            // Arrange
            var category = new CustomerClassCategory();

            // Act
            category.CategoryName = "";
            category.CategoryNameCN = "";
            category.CustomerClassName = "";
            category.SabaRiskLevel = "";

            // Assert
            Assert.Equal("", category.CategoryName);
            Assert.Equal("", category.CategoryNameCN);
            Assert.Equal("", category.CustomerClassName);
            Assert.Equal("", category.SabaRiskLevel);
        }

        [Fact]
        public void Properties_HandleNegativeNumbers()
        {
            // Arrange
            var category = new CustomerClassCategory();
            const int negativeCustomerClass = -1;
            const int negativeCategory = -5;

            // Act
            category.CustomerClass = negativeCustomerClass;
            category.Category = negativeCategory;

            // Assert
            Assert.Equal(negativeCustomerClass, category.CustomerClass);
            Assert.Equal(negativeCategory, category.Category);
        }
    }
}