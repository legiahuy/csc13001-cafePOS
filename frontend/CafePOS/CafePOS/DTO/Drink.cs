using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Drink
    {
        public Drink() { }
        public Drink(int id, string name, float price)
        {
            this.ID = id;
            this.Name = name;
            this.Price = price;
        }
        public Drink(IGetDrinksByCategory_AllProducts_Edges_Node node)
        {
            this.ID = node.Id;
            this.Name = node.Name;
            this.Price = (float)node.Price;
            this.imageUrl = node.ImageUrl!;
        }

        public Drink(IGetListProducts_AllProducts_Edges_Node node)
        {
            this.ID = node.Id;
            this.Name = node.Name;
            this.Price = (float)node.Price;
            this.categoryId = node.IdCategory;
            this.imageUrl = node.ImageUrl!;
        }

        private float price;
        public float Price
        {
            get { return price; }
            set { price = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int iD;
        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }

        public int CategoryId { get => categoryId; set => categoryId = value; }
        public string ImageUrl { get => imageUrl; set => imageUrl = value; }

        private int categoryId;

        private string imageUrl;
    }
}
