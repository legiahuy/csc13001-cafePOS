using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.DTO;
using CafePOS.GraphQL.State;

namespace CafePOS.DAO
{
    public class CategoryDAO
    {
        private static CategoryDAO instance;
        public static CategoryDAO Instance
        {
            get { if (instance == null) instance = new CategoryDAO(); return CategoryDAO.instance; }
            private set { CategoryDAO.instance = value; }
        }
        private CategoryDAO() { }
        public async Task<List<Category>> GetListCategoryAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetCategories.ExecuteAsync();

            var categories = result.Data?.AllCategories?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new Category(e.Node))
                .ToList() ?? new List<Category>();

            return categories;
        }
    }
}
