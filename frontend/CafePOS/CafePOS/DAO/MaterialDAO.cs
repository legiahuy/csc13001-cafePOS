using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    internal class MaterialDAO
    {
        private static MaterialDAO instance;
        public static MaterialDAO Instance
        {
            get { if (instance == null) instance = new MaterialDAO(); return MaterialDAO.instance; }
            private set { MaterialDAO.instance = value; }
        }
        private MaterialDAO() { }
        public async Task<List<Material>> GetListMaterialAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetListMaterials.ExecuteAsync();

            var materials = result.Data?.AllMaterials?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new Material(e.Node))
                .ToList() ?? new List<Material>();

            return materials;
        }

        public async Task<int> InsertMaterialAsync(String name, int currentStock, int minStock, string unit, double price, string ImageUrl)
        {
            if (ImageUrl.Equals("") || string.IsNullOrEmpty(ImageUrl)) ImageUrl = "/Assets/default-image.jpg";
            var client = DataProvider.Instance.Client;

            var result = await client.CreateMaterial.ExecuteAsync(name, null, price, minStock, unit, currentStock, ImageUrl);

            return result.Data?.CreateMaterial?.Material?.Id ?? -1;
        }

        public async Task<bool> UpdateMaterialAsync(int id, String name, int currentStock, int minStock, string unit, double price, string ImageUrl)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.UpdateMaterial.ExecuteAsync(id, name, price, minStock, unit, currentStock, ImageUrl, "");
            return result.Data?.UpdateMaterialById?.Material?.Id == id;
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.DeleteMaterialById.ExecuteAsync(id);
            string deletedMaterialId = result.Data?.DeleteMaterialById?.DeletedMaterialId!;
            return !string.IsNullOrEmpty(deletedMaterialId);
        }
    }
}
