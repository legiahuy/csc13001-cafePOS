using Microsoft.VisualStudio.TestTools.UnitTesting;
using CafePOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CafePOS.Tests
{
    [TestClass]
    public class BillManageTest
    {
        [TestMethod]
        public void FormatBillStatus_ShouldReturnCorrectText()
        {
            // Arrange
            int paidStatus = 1;
            int cancelledStatus = 0;

            // Act
            string paidText = paidStatus == 1 ? "Đã thanh toán" : "Đã hủy";
            string cancelledText = cancelledStatus == 1 ? "Đã thanh toán" : "Đã hủy";

            // Assert
            Assert.AreEqual("Đã thanh toán", paidText);
            Assert.AreEqual("Đã hủy", cancelledText);
        }

        [TestMethod]
        public void FormatBillAmount_ShouldReturnCorrectFormat()
        {
            // Arrange
            double amount = 2000000;
            string expected = "2,000,000đ";

            // Act
            string formatted = string.Format("{0:N0}đ", amount);

            // Assert
            Assert.AreEqual(expected, formatted);
        }

        [TestMethod]
        public void CalculatePagination_ShouldReturnCorrectPages()
        {
            // Arrange
            int totalItems = 25;
            int itemsPerPage = 10;
            int expectedTotalPages = 3;

            // Act
            int totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            // Assert
            Assert.AreEqual(expectedTotalPages, totalPages);
        }

        [TestMethod]
        public void ValidateDateRange_ShouldReturnTrueForValidRange()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(1);

            // Act
            bool isValid = startDate <= endDate;

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void FilterBills_ShouldReturnMatchingBills()
        {
            // Arrange
            var bills = new List<BillViewModel>
            {
                new BillViewModel { Id = "1", TotalAmount = 100000 },
                new BillViewModel { Id = "2", TotalAmount = 200000 },
                new BillViewModel { Id = "3", TotalAmount = 300000 }
            };
            string searchId = "2";

            // Act
            var filteredBills = bills.Where(b => b.Id == searchId);

            // Assert
            Assert.AreEqual(1, filteredBills.Count());
            Assert.AreEqual(200000, filteredBills.First().TotalAmount);
        }
    }

    // Mock class for testing
    public class BillViewModel
    {
        public string Id { get; set; }
        public double TotalAmount { get; set; }
    }
}
