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
    }
}
