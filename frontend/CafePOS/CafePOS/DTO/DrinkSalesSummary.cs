using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class DrinkSalesSummary
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public int QuantitySold { get; set; }
        public float TotalRevenue => Price * QuantitySold;
        public double QuantityPercentage { get; set; }
        public double RevenuePercentage { get; set; }
    }
}
