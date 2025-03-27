using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class CafeTableDAO
    {
        private static CafeTableDAO? instance;
        public static CafeTableDAO Instance
        {
            get { if (instance == null) instance = new CafeTableDAO(); return instance; }
            private set { instance = value; }
        }

        public static int tableWidth = 100;
        public static int tableHeight = 100;

        private CafeTableDAO() { }

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
    }
}
