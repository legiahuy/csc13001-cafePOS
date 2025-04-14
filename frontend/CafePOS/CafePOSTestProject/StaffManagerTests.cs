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
        public async Task AddStaff_ShouldSucceedAndAppearInList()
        {
            var beforeList = await StaffDAO.Instance.GetAllStaffAsync();
            Assert.IsNotNull(beforeList, "Danh sách nhân viên trước khi thêm là null");

            string username = GenerateRandomUsername();
            string email = GenerateRandomEmail();

            bool accCreated = await AccountDAO.Instance.CreateAccountAsync(username, "Test Display", "123456", 0);
            Assert.IsTrue(accCreated, "Tạo tài khoản thất bại");

            bool added = await StaffDAO.Instance.AddStaffAsync(
                name: "Nguyễn Văn A",
                dob: "2000-01-01",
                gender: "Nam",
                phone: "0987654321",
                email: email,
                position: "Nhân viên",
                salary: 6000000,
                userName: username
            );
            Assert.IsTrue(added, "Thêm nhân viên thất bại");

            var afterList = await StaffDAO.Instance.GetAllStaffAsync();
            var addedStaff = afterList.FirstOrDefault(s => s.UserName == username);
            Assert.IsNotNull(addedStaff, "Không tìm thấy nhân viên vừa thêm");

            await StaffDAO.Instance.DeleteStaffAsync(addedStaff.Id);
            await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);
        }

        [TestMethod]
        public async Task UpdateStaff_ShouldReflectNewData()
        {
            string username = GenerateRandomUsername();
            string email = GenerateRandomEmail();

            await AccountDAO.Instance.CreateAccountAsync(username, "Display", "123", 0);
            await StaffDAO.Instance.AddStaffAsync("Tên cũ", "1990-05-05", "Nam", "0123456789", email, "NV", 4500000, username);

            var staff = (await StaffDAO.Instance.GetAllStaffAsync()).FirstOrDefault(s => s.UserName == username);
            Assert.IsNotNull(staff, "Không tìm thấy nhân viên để cập nhật");

            string newName = "Tên mới";
            string newPhone = "0988123123";

            bool updated = await StaffDAO.Instance.UpdateStaffAsync(
                staff.Id, newName, staff.Dob, staff.Gender, newPhone, staff.Email, staff.Position, staff.Salary);
            Assert.IsTrue(updated, "Cập nhật nhân viên thất bại");

            var afterUpdate = (await StaffDAO.Instance.GetAllStaffAsync()).FirstOrDefault(s => s.Id == staff.Id);
            Assert.AreEqual(newName, afterUpdate?.Name, "Tên không được cập nhật đúng");
            Assert.AreEqual(newPhone, afterUpdate?.Phone, "SĐT không được cập nhật đúng");

            await StaffDAO.Instance.DeleteStaffAsync(staff.Id);
            await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);
        }

        [TestMethod]
        public async Task DeleteStaff_ShouldAlsoRemoveAccount()
        {
            string username = GenerateRandomUsername();
            string email = GenerateRandomEmail();

            await AccountDAO.Instance.CreateAccountAsync(username, "ToDelete", "123", 0);
            await StaffDAO.Instance.AddStaffAsync("ToDelete", "1999-12-12", "Nữ", "0888888888", email, "NV", 3000000, username);

            var staff = (await StaffDAO.Instance.GetAllStaffAsync()).FirstOrDefault(s => s.UserName == username);
            Assert.IsNotNull(staff, "Không tìm thấy nhân viên để xoá");

            bool deletedStaff = await StaffDAO.Instance.DeleteStaffAsync(staff.Id);
            Assert.IsTrue(deletedStaff, "Xoá nhân viên thất bại");

            await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);

            var stillExists = (await StaffDAO.Instance.GetAllStaffAsync()).Any(s => s.Id == staff.Id);
            Assert.IsFalse(stillExists, "Nhân viên vẫn còn tồn tại sau khi xoá");
        }
    }
}
