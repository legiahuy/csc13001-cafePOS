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

        private BillInfoDAO() { }

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

    }
}
