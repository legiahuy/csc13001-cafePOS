using System;
using System.Collections.Generic;
using System.Linq;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Menu
    {
        public Menu() { }
    
        public Menu(string productName, int count, double price, double totalPrice = 0)
        {
            this.ProductName = productName;
            this.Count = count;
            this.Price = price;
            this.TotalPrice = totalPrice;
        }

        public string ProductName { get; set; }
        public int Count { get; set; }
        public double TotalPrice { get; set; }
        public double Price { get; set; }
    }
}
