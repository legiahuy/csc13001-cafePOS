using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class MenuDAO
    {
        private static MenuDAO? instance;
        public static MenuDAO Instance
        {
            get { if (instance == null) instance = new MenuDAO(); return instance; }
            private set { instance = value; }
        }

        private MenuDAO() { }

        public async Task<List<Menu>> GetListMenuByTableAsync(int id)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetListMenuByTable.ExecuteAsync(id);

            if (result.Data?.AllBills?.Nodes != null)
            {
                var listMenu = result.Data.AllBills.Nodes
                    .SelectMany(bill =>
                        bill!.BillInfosByIdBill.Nodes.Select(billInfo => new Menu
                        {
                            ProductName = billInfo!.ProductByIdProduct!.Name,
                            Count = billInfo.Count,
                            Price = billInfo.ProductByIdProduct.Price,
                            TotalPrice = billInfo.Count * billInfo.ProductByIdProduct.Price
                        }))
                    .ToList();

                Debug.WriteLine(result);

                foreach (Menu item in listMenu) {
                    Debug.WriteLine($"{item.ProductName}  --- {item.Price} --- {item.Count} ---- {item.TotalPrice}  ");
                }

                return listMenu;
            }
            return new List<Menu>();
        }
    }
}
