using System;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class FullBill
    {
        public FullBill(IGetAllFullBills_AllBills_Edges_Node node)
        {
            Id = node.Id;
            IdTable = node.IdTable;
            DateCheckIn = DateTime.TryParse(node.DateCheckIn, out var checkIn) ? checkIn : (DateTime?)null;
            DateCheckOut = DateTime.TryParse(node.DateCheckOut, out var checkOut) ? checkOut : (DateTime?)null;
            Status = node.Status;
            TotalAmount = node.TotalAmount;
            Discount = node.Discount ?? 0.0;
            FinalAmount = node.FinalAmount;
            PaymentMethod = node.PaymentMethod ?? "Unknown";
        }

        public int Id { get; set; }
        public int IdTable { get; set; }
        public DateTime? DateCheckIn { get; set; }
        public DateTime? DateCheckOut { get; set; }
        public int Status { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double FinalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string StatusText => DateCheckOut == null ? "Chưa thanh toán" : "Đã thanh toán";
    }
}
