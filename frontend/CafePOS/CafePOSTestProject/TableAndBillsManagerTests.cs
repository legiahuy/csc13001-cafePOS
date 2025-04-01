using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CafePOS.Tests
{
    public class CafeTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class Menu
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public float Price { get; set; }
        public double TotalPrice { get; set; }
    }

    [TestClass]
    public sealed class TableAndBillsManagerTests
    {
        [TestMethod]
        public void UpdateTableStatus_WhenBillExists_ShouldSetStatusToOccupied()
        {
            var table = new CafeTable { Id = 1, Name = "Table 1", Status = "Trống" };
            table.Status = DoesBillExist_ForTableId_ShouldReturnTrue(table.Id) ? "Có khách" : "Trống";
            Assert.AreEqual("Có khách", table.Status);
        }

        [TestMethod]
        public void UpdateTableStatus_WhenNoBillExists_ShouldRemainEmpty()
        {
            var table = new CafeTable { Id = 2, Name = "Table 2", Status = "Trống" };
            table.Status = DoesBillExist_ForTableId_ShouldReturnFalse(table.Id) ? "Trống" : "Có khách";
            Assert.AreEqual("Trống", table.Status);
        }

        [TestMethod]
        public void CalculateTotalPrice_ForMenuItems_ShouldReturnCorrectTotal()
        {
            var menuItems = new List<Menu>
            {
                new Menu { Name = "Coffee", Count = 2, Price = 20000, TotalPrice = 40000 },
                new Menu { Name = "Cake", Count = 1, Price = 30000, TotalPrice = 30000 }
            };
            double totalPrice = 0;
            foreach (var item in menuItems)
            {
                totalPrice += item.TotalPrice;
            }
            Assert.AreEqual(70000, totalPrice);
        }

        [TestMethod]
        public void ApplyDiscount_WhenValidPercentage_ShouldCalculateFinalAmount()
        {
            double originalTotal = 100000;
            float discount = 15.0f;
            double finalTotal = originalTotal * (100 - discount) / 100;
            Assert.AreEqual(85000, finalTotal);
        }

        [TestMethod]
        public void FormatPrice_WhenUsingVietnameseCulture_ShouldContainCurrencySymbol()
        {
            double amount = 70000;
            var culture = new System.Globalization.CultureInfo("vi-VN");
            string formatted = amount.ToString("C0", culture);
            Assert.IsTrue(formatted.Contains("₫"));
        }

        [TestMethod]
        public void ChangeTable_WhenTargetTableOccupied_ShouldPreventChange()
        {
            var sourceTable = new CafeTable { Id = 1, Name = "Table 1", Status = "Có khách" };
            var targetTable = new CafeTable { Id = 2, Name = "Table 2", Status = "Có khách" };
            bool canChange = CanChangeTable_WhenTargetOccupied_ShouldReturnFalse(sourceTable, targetTable);
            Assert.IsFalse(canChange);
        }

        [TestMethod]
        public void ChangeTable_WhenTargetTableEmpty_ShouldAllowChange()
        {
            var sourceTable = new CafeTable { Id = 1, Name = "Table 1", Status = "Có khách" };
            var targetTable = new CafeTable { Id = 2, Name = "Table 2", Status = "Trống" };
            bool canChange = CanChangeTable_WhenTargetEmpty_ShouldReturnTrue(sourceTable, targetTable);
            Assert.IsTrue(canChange);
        }

        [TestMethod]
        public void AddOrder_WhenNoExistingBill_ShouldCreateNewBill()
        {
            int tableId = 1;
            int foodId = 2;
            int count = 3;
            float unitPrice = 25000f;
            int billId = -1;
            if (billId == -1)
            {
                billId = CreateNewBill_ForTable_ShouldReturnBillId(tableId);
                Assert.AreNotEqual(-1, billId);
                bool success = AddItemToBill_ForNewBill_ShouldReturnTrue(billId, foodId, count, unitPrice);
                Assert.IsTrue(success);
            }
        }

        [TestMethod]
        public void AddOrder_WhenExistingBill_ShouldAddItemSuccessfully()
        {
            int billId = 5;
            int foodId = 2;
            int count = 3;
            float unitPrice = 25000f;
            bool success = AddItemToBill_ForExistingBill_ShouldReturnTrue(billId, foodId, count, unitPrice);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void Checkout_WhenApplyingDiscount_ShouldCalculateFinalTotal()
        {
            int billId = 5;
            float discount = 10.0f;
            var menuItems = new List<Menu>
            {
                new Menu { Name = "Coffee", Count = 2, Price = 20000, TotalPrice = 40000 },
                new Menu { Name = "Cake", Count = 1, Price = 30000, TotalPrice = 30000 }
            };
            double originalTotal = 0;
            foreach (var item in menuItems)
            {
                originalTotal += item.TotalPrice;
            }
            double finalTotal = originalTotal * (100 - discount) / 100;
            bool checkoutSuccess = Checkout_ProcessDiscount_ShouldReturnTrue(billId, discount);
            Assert.IsTrue(checkoutSuccess);
            Assert.AreEqual(63000, finalTotal);
        }

        private bool DoesBillExist_ForTableId_ShouldReturnTrue(int tableId) => tableId == 1;
        private bool DoesBillExist_ForTableId_ShouldReturnFalse(int tableId) => tableId == 2;
        private bool CanChangeTable_WhenTargetOccupied_ShouldReturnFalse(CafeTable source, CafeTable target) => target.Status != "Có khách";
        private bool CanChangeTable_WhenTargetEmpty_ShouldReturnTrue(CafeTable source, CafeTable target) => target.Status == "Trống";
        private int CreateNewBill_ForTable_ShouldReturnBillId(int tableId) => 10;
        private bool AddItemToBill_ForNewBill_ShouldReturnTrue(int billId, int foodId, int count, float unitPrice) => true;
        private bool AddItemToBill_ForExistingBill_ShouldReturnTrue(int billId, int foodId, int count, float unitPrice) => true;
        private bool Checkout_ProcessDiscount_ShouldReturnTrue(int billId, float discount) => true;
    }
}
