using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Bill
    {
        public Bill(int id, DateTime? dateCheckIn, DateTime? dateCheckOut, int status) {
            this.Id = id;
            this.DateCheckIn = dateCheckIn;
            this.DateCheckOut = dateCheckOut;
            this.Status = status;
        }

        public Bill(IGetBillByTableIdAndStatus_AllBills_Edges_Node node)
        {
            this.Id = node.Id;
            this.DateCheckIn = DateTime.TryParse(node.DateCheckIn, out DateTime checkIn) ? checkIn : (DateTime?)null;
            this.DateCheckOut = DateTime.TryParse(node.DateCheckOut, out DateTime checkOut) ? checkOut : (DateTime?)null;
            this.Status = node.Status;
        }

        private DateTime? _dateCheckIn;
        private DateTime? _dateCheckOut;
        private int _id;
        private int _status;

        public DateTime? DateCheckIn { get => _dateCheckIn; set => _dateCheckIn = value; }
        public int Id { get => _id; set => _id = value; }
        public DateTime? DateCheckOut { get => _dateCheckOut; set => _dateCheckOut = value; }
        public int Status { get => _status; set => _status = value; }

        public string StatusText => DateCheckOut == null ? "Chưa thanh toán" : "Đã thanh toán";
    }
}
