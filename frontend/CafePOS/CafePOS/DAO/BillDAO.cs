using System;
using System.Collections.Generic;
using System.Data;
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


        public async Task<bool> CheckOutAsync(int id, float discount, float totalPrice)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                string dateCheckout = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var result = await client.UpdateBillStatus.ExecuteAsync(id, discount,dateCheckout ,totalPrice);
                var updatedBill = result.Data?.UpdateBillById?.Bill;
                return updatedBill?.Status == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CheckOutAsync: {ex.Message}");
                return false;
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

        public async Task<List<Bill>> GetBillsByDateAsync(DateTime checkIn, DateTime checkOut)
        {
            var client = DataProvider.Instance.Client;
            var checkInString = checkIn.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
            var checkOutString = checkOut.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
            var result = await client.GetBillsByDate.ExecuteAsync(checkInString, checkOutString);

            var bills = new List<Bill>();

            if (result.Data?.AllBills?.Edges != null)
            {
                foreach (var edge in result.Data.AllBills.Edges)
                {
                    bills.Add(new Bill(
                        edge.Node.Id,
                        DateTime.TryParse(edge.Node.DateCheckIn, out DateTime dateIn) ? dateIn : (DateTime?)null,
                        DateTime.TryParse(edge.Node.DateCheckOut, out DateTime dateOut) ? dateOut : (DateTime?)null,
                        edge.Node.Status
                    ));
                }
            }

            return bills;
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

        public async Task<bool> ChangeTableAsync(int billId, int newTableId)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var result = await client.ChangeTable.ExecuteAsync(billId, newTableId);
                return result.Data?.UpdateBillById?.Bill?.IdTable == newTableId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ChangeTableAsync: {ex.Message}");
                return false;
            }
        }

    }
}


