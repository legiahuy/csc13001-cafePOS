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

        //    public async Task<bool> CreateAccountAsync(string userName, string displayName, string password, int type)
        //    {
        //        var client = DataProvider.Instance.Client;
        //        var result = await client.CreateAccount.ExecuteAsync(userName, displayName, password, type);
        //        return result.Data?.CreateAccount?.Account?.UserName != null;
        //    }

        //    public async Task<int> GetAccountTypeAsync(string userName)
        //    {
        //        var client = DataProvider.Instance.Client;
        //        var result = await client.GetAccountByUserName.ExecuteAsync(userName);
        //        return result.Data?.AccountByUserName?.Type ?? 0;
        //    }

        //    public async Task<Account> GetAccountByUserNameAsync(string userName)
        //    {
        //        var client = DataProvider.Instance.Client;
        //        var result = await client.GetAccountByUserName.ExecuteAsync(userName);
        //        var acc = result.Data?.AccountByUserName;

        //        return new Account
        //        {
        //            UserName = acc?.UserName ?? "",
        //            DisplayName = acc?.DisplayName ?? "",
        //            Type = acc?.Type ?? 0,
        //            Password = acc?.Password ?? ""
        //        };
        //    }
        //    public async Task<bool> UpdateDisplayNameAsync(string userName, string newDisplayName)
        //    {
        //        var client = DataProvider.Instance.Client;
        //        var result = await client.UpdateAccountDisplayName.ExecuteAsync(userName, newDisplayName);
        //        return result.Data?.UpdateAccountByUserName?.Account?.DisplayName == newDisplayName;
        //    }

        //    public async Task<bool> DeleteAccountByUserNameAsync(string userName)
        //    {
        //        var client = DataProvider.Instance.Client;
        //        var result = await client.DeleteAccountByUserName.ExecuteAsync(userName);
        //        return result.Data?.DeleteAccountByUserName?.Account?.UserName != null;
        //    }

        //}
    }
}
