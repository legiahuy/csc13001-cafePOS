using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class BillInfoDAO
    {
        private static BillInfoDAO? instance;
        public static BillInfoDAO Instance
        {
            get { if (instance == null) instance = new BillInfoDAO(); return instance; }
            private set { instance = value; }
        }

        public BillInfoDAO() { }

        public async Task<List<BillInfo>> GetListBillInfoAsync(int id)
        {

            //List<BillInfo> listBillInfo = new List<BillInfo>();

            var client = DataProvider.Instance.Client;
            var result = await client.GetBillInfoByBillId.ExecuteAsync(id);

            var listBillInfo = result.Data?.AllBillInfos?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new BillInfo(e.Node))
                .ToList() ?? new List<BillInfo>();

            return listBillInfo;
        }
        public async Task<int> InsertBillInfoAsync(int idBill, int idProduct, int count, float unitPrice, float totalPrice)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.InsertBillInfo.ExecuteAsync(idBill, idProduct, count, unitPrice, totalPrice);

            return result.Data?.CreateBillInfo?.BillInfo?.Id ?? -1;
        }

        public async Task<int> GetUncheckBillIDByTableIDAsync(int tableId)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetBillByTableIdAndStatus.ExecuteAsync(tableId, 0); // 0 = Unpaid

            return result.Data?.AllBills?.Edges?
                .Where(e => e.Node != null)
                .Select(e => e.Node.Id)
                .FirstOrDefault() ?? -1;
        }
        public async Task<List<BillInfo>> GetBillInfoByBillIdAsync(int billId)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var result = await client.GetBillInfoByBillId.ExecuteAsync(billId);

                var billInfos = result.Data?.AllBillInfos?.Edges?
                    .Where(e => e.Node != null)
                    .Select(e => new BillInfo(e.Node))
                    .ToList() ?? new List<BillInfo>();

                return billInfos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetBillInfoByBillIdAsync: {ex.Message}");
                return new List<BillInfo>();
            }
        }

    }
}
