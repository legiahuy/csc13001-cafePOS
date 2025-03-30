using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class BillDAO
    {
        private static BillDAO? instance;
        public static BillDAO Instance
        {
            get { if (instance == null) instance = new BillDAO(); return instance; }
            private set { instance = value; }
        }

        private BillDAO() { }

        /// <summary>
        /// Success: bill ID
        /// Fail: -1
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<int> GetUncheckBillIDByTableID(int id, int status)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var result = await client.GetBillByTableIdAndStatus.ExecuteAsync(id, status);
                var billNode = result.Data?.AllBills?.Edges?.FirstOrDefault()?.Node;

                if (billNode != null)
                {
                    Bill bill = new Bill(billNode);
                    return bill.Id;
                }

                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error GetUnCheckBillIDByID: {ex.Message}");
                return -1;
            }
        }

        public async Task<List<CafeTable>> GetAllCafeTablesAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllCafeTables.ExecuteAsync();

            var tableList = result.Data?.AllCafeTables?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new CafeTable(e.Node))
                .ToList() ?? new List<CafeTable>();

            return tableList;
        }
        public async Task<int> InsertBillAsync(int idTable)
        {
            var client = DataProvider.Instance.Client;
            var dateCheckIn = DateTime.UtcNow.ToString("o");

            var result = await client.CreateBill.ExecuteAsync(idTable, 0, dateCheckIn);

            return result.Data?.CreateBill?.Bill?.Id ?? -1;
        }

        public async Task<int> GetMaxIDBillAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllBills.ExecuteAsync();

            var maxId = result.Data?.AllBills?.Edges?
                .Where(e => e.Node != null)  
                .Select(e => e.Node.Id)     
                .DefaultIfEmpty(1)          
                .Max() ?? 1;                      

            return maxId;
        }
    }
}
