using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Category
    {
        public Category(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public Category(IGetCategories_AllCategories_Edges_Node node)
        {
            ID = node.Id;
            Name = node.Name;
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
    }
}
