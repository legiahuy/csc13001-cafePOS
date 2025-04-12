using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using CafePOS.DTO;
using CafePOS.DAO;
using System.Linq;

namespace CafePOS.Tests
{
    [TestClass]
    public class StaffPageTests
    {
        private static string GenerateRandomUsername() =>
            "testuser_" + Guid.NewGuid().ToString("N").Substring(0, 6);

        private static string GenerateRandomEmail() =>
            $"test_{Guid.NewGuid().ToString("N").Substring(0, 6)}@example.com";

        [TestMethod]
        public async Task AddStaff_WithAccount_ShouldIncreaseCount()
        {
            var beforeList = await StaffDAO.Instance.GetAllStaffAsync();
            Assert.IsNotNull(beforeList, "Danh sách trước khi thêm bị null");

            string username = GenerateRandomUsername();
            string name = "Test User";
            string dob = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd");
            string gender = "Nam";
            string phone = "0123" + new Random().Next(1000000, 9999999);
            string email = GenerateRandomEmail();
            string position = "Tester";
            float salary = 5000000f;
            string password = "123456";

            bool accountCreated = await AccountDAO.Instance.CreateAccountAsync(username, name, password, 0);
            Assert.IsTrue(accountCreated, "Tạo tài khoản thất bại");

            bool staffAdded = await StaffDAO.Instance.AddStaffAsync(name, dob, gender, phone, email, position, salary, username);
            Assert.IsTrue(staffAdded, "Thêm nhân viên thất bại");

            var afterList = await StaffDAO.Instance.GetAllStaffAsync();
            var addedStaff = afterList.FirstOrDefault(s => s.Email == email);
            Assert.IsNotNull(addedStaff, "Không tìm thấy nhân viên vừa thêm");

            if (addedStaff != null)
            {
                await StaffDAO.Instance.DeleteStaffAsync(addedStaff.Id);
            }
            await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);
        }

        [TestMethod]
        public async Task UpdateStaff_ShouldChangeDetails()
        {
            string username = GenerateRandomUsername();
            string email = GenerateRandomEmail();

            bool acc = await AccountDAO.Instance.CreateAccountAsync(username, "Temp", "123", 0);
            Assert.IsTrue(acc);

            bool added = await StaffDAO.Instance.AddStaffAsync("Temp", "1995-01-01", "Nam", "0999999999", email, "NV", 4000000, username);
            Assert.IsTrue(added);

            var staffList = await StaffDAO.Instance.GetAllStaffAsync();
            var staff = staffList.FirstOrDefault(s => s.Email == email);
            Assert.IsNotNull(staff);

            string newName = "Updated Name";
            bool updated = await StaffDAO.Instance.UpdateStaffAsync(staff.Id, newName, staff.Dob, staff.Gender, staff.Phone, staff.Email, staff.Position, staff.Salary);

            var updatedStaff = (await StaffDAO.Instance.GetAllStaffAsync()).FirstOrDefault(s => s.Id == staff.Id);
            Assert.IsTrue(updated);
            Assert.AreEqual(newName, updatedStaff?.Name);

            await StaffDAO.Instance.DeleteStaffAsync(staff.Id);
            await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);
        }

        [TestMethod]
        public async Task DeleteStaff_ShouldRemoveFromList_AndDeleteAccount()
        {
            string username = GenerateRandomUsername();
            string email = GenerateRandomEmail();

            await AccountDAO.Instance.CreateAccountAsync(username, "ToDelete", "123", 0);
            await StaffDAO.Instance.AddStaffAsync("ToDelete", "1999-12-12", "Nữ", "0888888888", email, "NV", 3000000, username);

            var list = await StaffDAO.Instance.GetAllStaffAsync();
            var toDelete = list.FirstOrDefault(s => s.UserName == username);
            Assert.IsNotNull(toDelete);

            bool deleted = await StaffDAO.Instance.DeleteStaffAsync(toDelete.Id);
            await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);

            var listAfterDelete = await StaffDAO.Instance.GetAllStaffAsync();
            var stillExists = listAfterDelete.Any(s => s.Id == toDelete.Id);

            Assert.IsTrue(deleted);
            Assert.IsFalse(stillExists);
        }
    }
}
