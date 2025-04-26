using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DTO;
using CafePOS.GraphQL;
using Windows.Media.Protection.PlayReady;

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

        private BillInfoDAO() { }

        public async Task<List<BillInfo>> GetListBillInfoAsync(int id)
        {

            //List<BillInfo> listBillInfo = new List<BillInfo>();

            var client = DataProvider.Instance.Client;
            var result = await client.GetBillInfoByBillId.ExecuteAsync(id);

            var listBillInfo = result.Data?.AllBillInfos?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new BillInfo(e.Node!))
                .ToList() ?? new List<BillInfo>();

            return listBillInfo;
        }
        //public async Task<int> InsertBillInfoAsync(int idBill, int idProduct, int count, float unitPrice, float totalPrice)
        //{
        //    var client = DataProvider.Instance.Client;
        //    var result = await client.InsertBillInfo.ExecuteAsync(idBill, idProduct, count, unitPrice, totalPrice);

        //    return result.Data?.CreateBillInfo?.BillInfo?.Id ?? -1;
        //}

        // Trong BillInfoDAO
        public async Task<bool> InsertBillInfoAsync(int idBill, int foodID, int count, float unitPrice, float totalPrice)
        {
            try
            {
                // Kiểm tra xem món đã tồn tại trong hóa đơn chưa
                var existingBillInfo = await GetExistingBillInfoAsync(idBill, foodID);
                if (existingBillInfo != null && existingBillInfo.Nodes.Count > 0)
                {
                    // Nếu đã tồn tại, cập nhật số lượng và tổng giá
                    int billInfoId = existingBillInfo.Nodes[0]!.Id;
                    int newCount = existingBillInfo.Nodes[0].Count + count;
                    float newTotalPrice = unitPrice * newCount;
                    return await UpdateBillInfoAsync(billInfoId, newCount, newTotalPrice);
                }
                else
                {
                    // Nếu chưa tồn tại, thêm mới
                    var client = DataProvider.Instance.Client;
                    var result = await client.InsertBillInfo.ExecuteAsync(
                        idBill,
                        foodID,
                        count,
                        unitPrice,
                        totalPrice);

                    return result.Data?.CreateBillInfo?.BillInfo != null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InsertBillInfoAsync: {ex.Message}");
                return false;
            }
        }

        // Hàm để lấy BillInfo hiện có
        private async Task<IFindBillInfo_AllBillInfos> GetExistingBillInfoAsync(int idBill, int foodID)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var result = await client.FindBillInfo.ExecuteAsync(idBill, foodID);
                return result.Data?.AllBillInfos!;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetExistingBillInfoAsync: {ex.Message}");
                return null!;
            }
        }

        // Thêm hàm cập nhật BillInfo
        private async Task<bool> UpdateBillInfoAsync(int id, int count, float totalPrice)
        {
            try
            {
                // Tạo mutation để cập nhật BillInfo
                var client = DataProvider.Instance.Client;
                var result = await client.UpdateBillInfo.ExecuteAsync(
                    id,
                    count,
                    totalPrice);

                return result.Data?.UpdateBillInfoById?.BillInfo != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateBillInfoAsync: {ex.Message}");
                return false;
            }
        }

        // Sửa hàm DeleteBillInfoAsync để xóa theo ID
        public async Task<bool> DeleteBillInfoByIdAsync(int billInfoId)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var result = await client.DeleteBillInfoById.ExecuteAsync(billInfoId);
                return result.Data?.DeleteBillInfoById?.BillInfo != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteBillInfoByIdAsync: {ex.Message}");
                return false;
            }
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

        public async Task<bool> DeleteBillInfoAsync(int billId, int productId)
        {
            try
            {
                // Step 1: Query to find the BillInfo id
                var client = DataProvider.Instance.Client;
                var findResult = await client.FindBillInfo.ExecuteAsync(billId, productId);

                // Check for errors
                if (findResult.Errors?.Any() == true)
                {
                    Debug.WriteLine("GraphQL Error finding BillInfo: " +
                        string.Join(", ", findResult.Errors.Select(e => e.Message)));
                    return false;
                }

                // Get the BillInfo id
                var billInfoNode = findResult.Data?.AllBillInfos?.Nodes?.FirstOrDefault();
                if (billInfoNode == null)
                {
                    Debug.WriteLine($"BillInfo with billId={billId} and productId={productId} not found");
                    return false;
                }

                int billInfoId = billInfoNode.Id;

                // Step 2: Delete the BillInfo
                var deleteResult = await client.DeleteBillInfoById.ExecuteAsync(billInfoId);

                // Check for errors
                if (deleteResult.Errors?.Any() == true)
                {
                    Debug.WriteLine("GraphQL Error deleting BillInfo: " +
                        string.Join(", ", deleteResult.Errors.Select(e => e.Message)));
                    return false;
                }

                // Check if deletion was successful
                return deleteResult.Data?.DeleteBillInfoById?.BillInfo != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteBillInfoAsync: {ex.Message}");
                return false;
            }
        }
    }
}
