using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DTO;
using CafePOS.GraphQL;
using System;
using System.Diagnostics;

namespace CafePOS.DAO
{
    public class StaffDAO
    {
        private static StaffDAO? instance;
        public static StaffDAO Instance => instance ??= new StaffDAO();

        private StaffDAO() { }

        public async Task<List<Staff>> GetAllStaffAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllStaff.ExecuteAsync();

            var list = result.Data?.AllStaff?.Nodes?
                .Where(s => s != null)
                .Select(s => new Staff(s!))
                .ToList();

            return list ?? new List<Staff>();
        }

        public async Task<int> GetStaffIdByUserName(string userName)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetStaffIdByUserName.ExecuteAsync(userName);
           
            var staffId = result.Data?.AllStaff?.Edges?.FirstOrDefault()?.Node?.Id ?? 0;

            Debug.WriteLine($"staffId '{staffId}' session");
            return staffId;
        }

        public async Task<bool> AddStaffAsync(string name, string dob, string gender, string phone, string email, string position, float salary, string userName)
        {
            var client = DataProvider.Instance.Client;

            var result = await client.CreateStaff.ExecuteAsync(
                name,
                DateOnly.Parse(dob),
                gender,
                phone,
                email,
                position,
                salary, 
                userName
            );

            return result.Data?.CreateStaff?.Staff?.Id > 0;
        }

        public async Task<bool> UpdateStaffAsync(int id, string name, string dob, string gender, string phone, string email, string position, float salary)
        {
            var client = DataProvider.Instance.Client;

            var result = await client.UpdateStaff.ExecuteAsync(
                id,
                name,
                DateOnly.Parse(dob),
                gender,
                phone,
                email,
                position,
                salary
            );

            return result.Data?.UpdateStaffById?.Staff?.Id > 0;
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            var client = DataProvider.Instance.Client;

            var result = await client.DeleteStaff.ExecuteAsync(id);

            return result.Data?.DeleteStaffById?.DeletedStaffId != null;
        }
    }
}
