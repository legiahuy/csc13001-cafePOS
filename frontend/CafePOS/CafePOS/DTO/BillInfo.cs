using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class BillInfo
    {

        public BillInfo(int id, int billID, int productID, int count) {
            this._billID = billID;
            this.Id = id;
            this.Count = count;
            this.ProductID = productID;
        }

        public BillInfo(IGetBillInfoByBillId_AllBillInfos_Edges_Node node)
        {
            this._billID = node.IdBill;
            this.Id = node.Id;
            this.Count = node.Count;
            this.ProductID = node.IdProduct;
        }

        private int _id;
        private int _billID;
        private int _count;
        private int _productID;

        public int Id { get => _id; set => _id = value; }
        public int BillId { get => _billID; set => _billID = value; }
        public int Count { get => _count; set => _count = value; }
        public int ProductID { get => _productID; set => _productID = value; }
    }
}
