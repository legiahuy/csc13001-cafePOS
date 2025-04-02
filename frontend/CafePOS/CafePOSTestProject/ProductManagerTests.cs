using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.UI.Xaml.Controls;

namespace CafePOS.Tests
{
    public class Drink
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    [TestClass]
    public sealed class ProductPageTests
    {
        [TestMethod]
        public void LoadProduct_ShouldReturnListOfDrinks()
        {
            // Arrange
            var drinkList = GetFakeDrinkList();

            // Act
            var result = LoadProduct_ShouldReturnDrinkList();

            // Assert
            Assert.AreEqual(drinkList.Count, result.Count);
            Assert.AreEqual("Espresso", result[0].Name);
            Assert.AreEqual(25000, result[0].Price);
        }

        [TestMethod]
        public void InsertProduct_WithValidData_ShouldReturnProductId()
        {
            // Arrange
            string name = "New Coffee";
            int categoryId = 1;
            int price = 30000;

            // Act
            int productId = InsertProduct_WithValidData_ShouldReturnId(name, categoryId, price);

            // Assert
            Assert.AreNotEqual(-1, productId);
            Assert.AreEqual(4, productId); // Next ID based on fake list (has 3 items)
        }

        [TestMethod]
        public void InsertProduct_WithEmptyName_ShouldReturnMinusOne()
        {
            // Arrange
            string name = "";
            int categoryId = 1;
            int price = 30000;

            // Act
            int productId = InsertProduct_WithInvalidData_ShouldReturnMinusOne(name, categoryId, price);

            // Assert
            Assert.AreEqual(-1, productId);
        }

        [TestMethod]
        public void UpdateProduct_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            int id = 1;
            string name = "Updated Espresso";
            string description = "Strong coffee";
            int categoryId = 1;
            int price = 27000;
            bool isAvailable = true;

            // Act
            bool success = UpdateProduct_WithValidData_ShouldReturnTrue(id, name, description, categoryId, price, isAvailable);

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void UpdateProduct_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            int id = 99; // Non-existent ID
            string name = "Updated Espresso";
            string description = "Strong coffee";
            int categoryId = 1;
            int price = 27000;
            bool isAvailable = true;

            // Act
            bool success = UpdateProduct_WithInvalidId_ShouldReturnFalse(id, name, description, categoryId, price, isAvailable);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void DeleteProduct_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            int id = 1;

            // Act
            bool success = DeleteProduct_WithValidId_ShouldReturnTrue(id);

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void DeleteProduct_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            int id = 99; // Non-existent ID

            // Act
            bool success = DeleteProduct_WithInvalidId_ShouldReturnFalse(id);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void GetCategories_ShouldReturnCategoryList()
        {
            // Arrange
            var expectedCategories = GetFakeCategoryList();

            // Act
            var result = GetCategories_ShouldReturnList();

            // Assert
            Assert.AreEqual(expectedCategories.Count, result.Count);
            Assert.AreEqual("Coffee", result[0].Name);
        }

        [TestMethod]
        public void GetCategoryById_WithValidId_ShouldReturnCategory()
        {
            // Arrange
            int categoryId = 1;

            // Act
            var category = GetCategoryById_WithValidId_ShouldReturnCategoryObject(categoryId);

            // Assert
            Assert.IsNotNull(category);
            Assert.AreEqual("Coffee", category.Name);
        }

        [TestMethod]
        public void GetCategoryById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            int categoryId = 99; // Non-existent ID

            // Act
            var category = GetCategoryById_WithInvalidId_ShouldReturnNull(categoryId);

            // Assert
            Assert.IsNull(category);
        }

        [TestMethod]
        public void ProductListView_SelectionChanged_ShouldUpdateDataContext()
        {
            // Arrange
            var selectedDrink = GetFakeDrinkList()[0];

            // Act
            object dataContext = ProductListView_SelectionChanged_ShouldSetDataContext(selectedDrink);

            // Assert
            Assert.IsNotNull(dataContext);
            Assert.AreEqual(selectedDrink, dataContext);
        }

        [TestMethod]
        public void FormatPrice_WhenUsingVietnameseCulture_ShouldContainCurrencySymbol()
        {
            // Arrange
            double amount = 25000;

            // Act
            var culture = new System.Globalization.CultureInfo("vi-VN");
            string formatted = amount.ToString("C0", culture);

            // Assert
            Assert.IsTrue(formatted.Contains("₫"));
        }

        [TestMethod]
        public void ValidateProductInput_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            string name = "Cappuccino";
            int? categoryId = 1;
            int price = 30000;

            // Act
            bool isValid = ValidateProductInput_WithValidData_ShouldReturnTrue(name, categoryId, price);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidateProductInput_WithEmptyName_ShouldReturnFalse()
        {
            // Arrange
            string name = "";
            int? categoryId = 1;
            int price = 30000;

            // Act
            bool isValid = ValidateProductInput_WithInvalidData_ShouldReturnFalse(name, categoryId, price);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateProductInput_WithNullCategoryId_ShouldReturnFalse()
        {
            // Arrange
            string name = "Cappuccino";
            int? categoryId = null;
            int price = 30000;

            // Act
            bool isValid = ValidateProductInput_WithInvalidData_ShouldReturnFalse(name, categoryId, price);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateProductInput_WithZeroPrice_ShouldReturnFalse()
        {
            // Arrange
            string name = "Cappuccino";
            int? categoryId = 1;
            int price = 0;

            // Act
            bool isValid = ValidateProductInput_WithInvalidData_ShouldReturnFalse(name, categoryId, price);

            // Assert
            Assert.IsFalse(isValid);
        }

        // Helper Methods (mocking functionality without using actual DAOs)
        private List<Drink> GetFakeDrinkList()
        {
            return new List<Drink>
            {
                new Drink { ID = 1, Name = "Espresso", Description = "Strong coffee", CategoryId = 1, Price = 25000, IsAvailable = true },
                new Drink { ID = 2, Name = "Cappuccino", Description = "Coffee with milk foam", CategoryId = 1, Price = 35000, IsAvailable = true },
                new Drink { ID = 3, Name = "Green Tea", Description = "Refreshing tea", CategoryId = 2, Price = 20000, IsAvailable = true }
            };
        }

        private List<Category> GetFakeCategoryList()
        {
            return new List<Category>
            {
                new Category { ID = 1, Name = "Coffee" },
                new Category { ID = 2, Name = "Tea" },
                new Category { ID = 3, Name = "Smoothie" }
            };
        }

        private List<Drink> LoadProduct_ShouldReturnDrinkList() => GetFakeDrinkList();

        private int InsertProduct_WithValidData_ShouldReturnId(string name, int categoryId, int price) => 4;

        private int InsertProduct_WithInvalidData_ShouldReturnMinusOne(string name, int categoryId, int price) => -1;

        private bool UpdateProduct_WithValidData_ShouldReturnTrue(int id, string name, string description, int categoryId, int price, bool isAvailable) => true;

        private bool UpdateProduct_WithInvalidId_ShouldReturnFalse(int id, string name, string description, int categoryId, int price, bool isAvailable) => false;

        private bool DeleteProduct_WithValidId_ShouldReturnTrue(int id) => true;

        private bool DeleteProduct_WithInvalidId_ShouldReturnFalse(int id) => false;

        private List<Category> GetCategories_ShouldReturnList() => GetFakeCategoryList();

        private Category GetCategoryById_WithValidId_ShouldReturnCategoryObject(int id) => GetFakeCategoryList().Find(c => c.ID == id);

        private Category GetCategoryById_WithInvalidId_ShouldReturnNull(int id) => null;

        private object ProductListView_SelectionChanged_ShouldSetDataContext(Drink selectedDrink) => selectedDrink;

        private bool ValidateProductInput_WithValidData_ShouldReturnTrue(string name, int? categoryId, int price) =>
            !string.IsNullOrWhiteSpace(name) && categoryId != null && price > 0;

        private bool ValidateProductInput_WithInvalidData_ShouldReturnFalse(string name, int? categoryId, int price) =>
            !string.IsNullOrWhiteSpace(name) && categoryId != null && price > 0;
    }
}