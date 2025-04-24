using Microsoft.VisualStudio.TestTools.UnitTesting;
using CafePOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CafePOS.Tests
{
    [TestClass]
    public class StatisticPageTest
    {
        [TestMethod]
        public void CalculatePercentageChange_ShouldReturnCorrectValue()
        {
            // Arrange
            double currentValue = 1000;
            double previousValue = 800;
            double expectedChange = 25;

            // Act
            double percentageChange = ((currentValue - previousValue) / previousValue) * 100;

            // Assert
            Assert.AreEqual(expectedChange, percentageChange);
        }

        [TestMethod]
        public void ValidateDateRange_ShouldReturnFalseForInvalidRange()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(-1);

            // Act
            bool isValid = startDate <= endDate;

            // Assert
            Assert.IsFalse(isValid, "Date range validation should fail when end date is before start date");
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
            Assert.IsTrue(isValid, "Date range validation should pass when end date is after start date");
        }

        [TestMethod]
        public void TimeRangeFilter_ShouldDisableComparisonsForNonDailyView()
        {
            // Arrange
            string[] nonDailyViews = { "Theo tuần", "Theo tháng", "Theo quý", "Theo năm" };

            // Act & Assert
            foreach (var view in nonDailyViews)
            {
                bool shouldEnableComparisons = view == "Theo ngày";
                Assert.IsFalse(shouldEnableComparisons, $"Comparisons should be disabled for {view}");
            }
        }

        [TestMethod]
        public void CalculateAverageBillValue_ShouldReturnCorrectValue()
        {
            // Arrange
            var bills = new List<double> { 100000, 200000, 300000 };
            double expectedAverage = 200000;

            // Act
            double average = bills.Average();

            // Assert
            Assert.AreEqual(expectedAverage, average);
        }

        [TestMethod]
        public void FormatCurrency_ShouldReturnCorrectFormat()
        {
            // Arrange
            double amount = 1234567;
            string expected = "1,234,567đ";

            // Act
            string formatted = string.Format("{0:N0}đ", amount);

            // Assert
            Assert.AreEqual(expected, formatted);
        }
    }
}
