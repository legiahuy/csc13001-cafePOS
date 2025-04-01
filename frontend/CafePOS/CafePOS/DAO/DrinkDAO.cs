using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    internal class DrinkDAO
    {
        private static DrinkDAO instance;
        public static DrinkDAO Instance
        {
            get { if (instance == null) instance = new DrinkDAO(); return DrinkDAO.instance; }
            private set { DrinkDAO.instance = value; }
        }
        private DrinkDAO() { }
        public async Task<List<Drink>> GetDrinkByCategoryIDAsync(int idCategory)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetDrinksByCategory.ExecuteAsync(idCategory);

            var drinks = result.Data?.AllProducts?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new Drink(e.Node))
                .ToList() ?? new List<Drink>();

            return drinks;
        }

        public async Task<List<Drink>> GetListDrinkAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetListProducts.ExecuteAsync();

            var products = result.Data?.AllProducts?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new Drink(e.Node))
                .ToList() ?? new List<Drink>();

            return products;
        }

        public async Task<int> InsertProductAsync(String name, int categoryId, int price)
        {
            var client = DataProvider.Instance.Client;

            var result = await client.CreateProduct.ExecuteAsync(name, "", categoryId, price, true);

            return result.Data?.CreateProduct?.Product?.Id ?? -1;
        }

        public async Task<bool> UpdateProductAsync(int id, string name, string description, int categoryId, int price, bool isAvailable)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.UpdateProduct.ExecuteAsync(id, name, description, categoryId, price, isAvailable);
            return result.Data?.UpdateProductById?.Product?.Id == id;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.DeleteProductById.ExecuteAsync(id);
            string deletedProductId = result.Data?.DeleteProductById?.DeletedProductId!;
            return !string.IsNullOrEmpty(deletedProductId);
        }
    }
}
