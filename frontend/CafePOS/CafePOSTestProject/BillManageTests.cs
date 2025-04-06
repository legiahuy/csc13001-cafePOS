using System;
using System.Collections.Generic;
using System.Linq;
using CafePOS.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CafePOS.Tests
{
    [TestClass]
    public class BillManageTests
    {
        private List<Bill> mockBills;

        [TestInitialize]
        public void Setup()
        {
            mockBills = new List<Bill>
            {
                new Bill(1, new DateTime(2025, 4, 1), new DateTime(2025, 4, 2), 0),
                new Bill(2, new DateTime(2025, 4, 3), new DateTime(2025, 4, 4), 1),
                new Bill(3, new DateTime(2025, 4, 5), new DateTime(2025, 4, 6), 0),
                new Bill(4, null, null, 2)
            };
        }

        [TestMethod]
        public void FilterBills_ByValidDateRange_ReturnsCorrectBills()
        {
            DateTime checkIn = new DateTime(2025, 4, 2);
            DateTime checkOut = new DateTime(2025, 4, 4);

            var result = FilterBillsByDateRange(mockBills, checkIn, checkOut);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.First().Id);
        }

        [TestMethod]
        public void FilterBills_WithNullCheckInDates_ExcludesInvalidBills()
        {
            var result = FilterBillsByDateRange(mockBills, new DateTime(2025, 4, 1), new DateTime(2025, 4, 6));

            Assert.IsFalse(result.Any(b => b.Id == 4));
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void FilterBills_WithEmptyList_ReturnsEmpty()
        {
            var result = FilterBillsByDateRange(new List<Bill>(), DateTime.Today, DateTime.Today);
            Assert.AreEqual(0, result.Count);
        }

        private List<Bill> FilterBillsByDateRange(List<Bill> bills, DateTime checkIn, DateTime checkOut)
        {
            return bills
                .Where(bill => bill.DateCheckIn.HasValue &&
                               bill.DateCheckIn.Value.Date >= checkIn.Date &&
                               bill.DateCheckIn.Value.Date <= checkOut.Date)
                .ToList();
        }
    }
}
