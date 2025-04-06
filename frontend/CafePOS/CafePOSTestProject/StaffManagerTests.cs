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
        [TestMethod]
        public async Task AddStaff_ShouldIncreaseCount()
        {
            var beforeList = await StaffDAO.Instance.GetAllStaffAsync();
            Assert.IsNotNull(beforeList, "Danh sách trước khi thêm bị null");

            string name = "Test User " + Guid.NewGuid().ToString("N").Substring(0, 5);
            string dob = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd");
            string gender = "Nam";
            string phone = "0123" + new Random().Next(1000000, 9999999);
            string email = $"test_{Guid.NewGuid().ToString("N").Substring(0, 5)}@example.com";
            string position = "Tester";
            float salary = 5000000f;

            bool added = await StaffDAO.Instance.AddStaffAsync(name, dob, gender, phone, email, position, salary);
            Console.WriteLine($"Added = {added}");

            var afterList = await StaffDAO.Instance.GetAllStaffAsync();
            var addedStaff = afterList.FirstOrDefault(s => s.Email == email);
            Console.WriteLine($"Added Staff ID = {addedStaff?.Id}");

            Assert.IsTrue(added, "Thêm nhân viên thất bại (CreateStaff trả về null hoặc ID <= 0)");
            Assert.IsNotNull(addedStaff, "Không tìm thấy nhân viên vừa thêm");

            if (addedStaff != null)
            {
                await StaffDAO.Instance.DeleteStaffAsync(addedStaff.Id);
            }
        }


        [TestMethod]
        public async Task UpdateStaff_ShouldChangeDetails()
        {
            var added = await StaffDAO.Instance.AddStaffAsync("Temp", "1995-01-01", "Nam", "0999999999", "temp@test.com", "NV", 4000000);
            var staffList = await StaffDAO.Instance.GetAllStaffAsync();
            var staff = staffList.FirstOrDefault(s => s.Name == "Temp");

            Assert.IsNotNull(staff);

            string newName = "Updated Name";
            bool updated = await StaffDAO.Instance.UpdateStaffAsync(staff.Id, newName, staff.Dob, staff.Gender, staff.Phone, staff.Email, staff.Position, staff.Salary);

            var updatedStaff = (await StaffDAO.Instance.GetAllStaffAsync()).FirstOrDefault(s => s.Id == staff.Id);
            Assert.IsTrue(updated);
            Assert.AreEqual(newName, updatedStaff?.Name);

            await StaffDAO.Instance.DeleteStaffAsync(staff.Id);
        }

        [TestMethod]
        public async Task DeleteStaff_ShouldRemoveFromList()
        {
            await StaffDAO.Instance.AddStaffAsync("ToDelete", "1999-12-12", "Nữ", "0888888888", "delete@test.com", "NV", 3000000);
            var list = await StaffDAO.Instance.GetAllStaffAsync();
            var toDelete = list.FirstOrDefault(s => s.Name == "ToDelete");

            Assert.IsNotNull(toDelete);

            bool deleted = await StaffDAO.Instance.DeleteStaffAsync(toDelete.Id);
            var listAfterDelete = await StaffDAO.Instance.GetAllStaffAsync();
            var stillExists = listAfterDelete.Any(s => s.Id == toDelete.Id);

            Assert.IsTrue(deleted);
            Assert.IsFalse(stillExists);
        }
    }
}
