using System;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? IconUrl { get; set; }
        public double? ProcessingFee { get; set; } = 0;
        public bool? RequiresVerification { get; set; } = false;

        public PaymentMethod() { }

        public PaymentMethod(IGetAllPaymentMethod_AllPaymentMethods_Edges_Node node) {
            this.Id = node.Id;
            this.Name = node.Name;
            this.Description = node.Description;
            this.IsActive = node.IsActive;
            this.IconUrl = node.IconUrl;
            this.ProcessingFee = node.ProcessingFee;
            this.RequiresVerification = node.RequiresVerification;
        }
    }
}
