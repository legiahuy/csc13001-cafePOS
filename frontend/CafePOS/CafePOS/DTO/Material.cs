using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Material
    {
        private string _name;
        private string _description;
        private int _id;
        private int _currentStock;
        private int _minStock;
        private float _price;
        private string _unit;
        private string _imageUrl;
        public Material() { }

        public Material(IGetListMaterials_AllMaterials_Edges_Node node) { 
            this.Name = node.Name;
            this.Description = node.Description!;
            this.Id = node.Id;
            this.Price = (float)node.Price;
            this.Unit = node.Unit;
            this.MinStock = (int)node.MinStock;
            this.CurrentStock = (int)node.CurrentStock;
            this.ImageUrl = node.ImageUrl!;
        }
        public bool IsLowStock => CurrentStock <= MinStock;

        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public int Id { get => _id; set => _id = value; }
        public int CurrentStock { get => _currentStock; set => _currentStock = value; }
        public int MinStock { get => _minStock; set => _minStock = value; }
        public float Price { get => _price; set => _price = value; }
        public string Unit { get => _unit; set => _unit = value; }
        public string ImageUrl { get => _imageUrl; set => _imageUrl = value; }
    }
}
