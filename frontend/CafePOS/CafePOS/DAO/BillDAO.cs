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

        public async Task<bool> CheckOutAsync(int id, double totalAmount, double finalAmount, int paymentMethod, int? guestId, string? paymentNotes, int idStaff)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var dateCheckOut = DateTime.UtcNow.ToString("o");

                // The discount is already calculated in the CheckoutWindow as discountByPoints
                // We just need to pass totalAmount - finalAmount as the discount amount
                double discount = totalAmount - finalAmount;

                var result = await client.UpdateBillStatus.ExecuteAsync(
                    id,
                    discount,  // This is now the discountByPoints value
                    paymentMethod,
                    guestId,
                    paymentNotes,
                    dateCheckOut,
                    totalAmount,
                    finalAmount,
                    idStaff   
                );

                var updatedBill = result.Data?.UpdateBillById?.Bill;
                return updatedBill?.Status == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CheckOutAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CancelAsync(int id)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var currentStaffId = Account.CurrentUserStaffId;
                Debug.WriteLine($"Cancelling bill {id} with staff ID {currentStaffId}");
                
                var result = await client.CancelBill.ExecuteAsync(
                    id,
                    DateTime.UtcNow.ToString("o"),  // dateCheckOut
                    currentStaffId   // Use current staff's ID
                );

                var updatedBill = result.Data?.UpdateBillById?.Bill;
                Debug.WriteLine($"Cancel result - Status: {updatedBill?.Status}");
                return updatedBill?.Status == 2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CancelAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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

        public async Task<int> InsertBillAsync(int idTable, int idStaff)
        {
            var client = DataProvider.Instance.Client;
            var dateCheckIn = DateTime.UtcNow.ToString("o");

            var result = await client.CreateBill.ExecuteAsync(idTable, 0, dateCheckIn, idStaff, null);

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
