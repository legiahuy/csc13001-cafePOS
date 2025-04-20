using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class PaymentMethodDAO
    {
        private static PaymentMethodDAO? instance;
        public static PaymentMethodDAO Instance
        {
            get { if (instance == null) instance = new PaymentMethodDAO(); return instance; }
            private set { instance = value; }
        }

        private PaymentMethodDAO() { }

        public async Task<List<PaymentMethod>> GetAllActivePaymentMethodsAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllPaymentMethod.ExecuteAsync();
            var paymentMethods = result.Data?.AllPaymentMethods?.Edges?
                .Where(e => e.Node != null && e.Node.IsActive)
                .Select(e => new PaymentMethod(e.Node!))
                .ToList() ?? new List<PaymentMethod>();

            return paymentMethods;
        }

        public async Task<List<PaymentMethod>> GetAllPaymentMethodAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllPaymentMethod.ExecuteAsync();
            var paymentMethods = result.Data?.AllPaymentMethods?.Edges?
                .Where(e => e.Node != null)
                .Select(e => new PaymentMethod(e.Node!))
                .ToList() ?? new List<PaymentMethod>();
            return paymentMethods;
        }

        public async Task<bool> UpdatePaymentMethodAsync(int id, string name, string? description, bool isActive, string? iconUrl)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.UpdatePaymentMethod.ExecuteAsync(id, name, description, isActive, iconUrl);
            return result.Data?.UpdatePaymentMethodById?.PaymentMethod?.Id == id;
        }

        public async Task<bool> DeletePaymentMethodAsync(int id)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.DeletePaymentMethodById.ExecuteAsync(id);
            string deletedPaymentMethodId = result.Data?.DeletePaymentMethodById?.DeletedPaymentMethodId!;
            return !string.IsNullOrEmpty(deletedPaymentMethodId);
        }

        public async Task<int> InsertPaymentMethodAsync(string name, string? description, bool isActive, string? iconUrl)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.CreatePaymentMethod.ExecuteAsync(name, description, isActive, iconUrl);
            return result.Data?.CreatePaymentMethod?.PaymentMethod?.Id ?? -1;
        }
    }
}
