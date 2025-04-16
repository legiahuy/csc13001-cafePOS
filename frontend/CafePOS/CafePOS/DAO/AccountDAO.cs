using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class AccountDAO
    {
        private static AccountDAO? instance;
        public static AccountDAO Instance
        {
            get { if (instance == null) instance = new AccountDAO(); return instance; }
            private set { instance = value; }
        }

        private AccountDAO() { }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            try
            {
                var client = DataProvider.Instance.Client;
                var result = await client.Login.ExecuteAsync(userName, password);

                bool isAuthenticated = result.Data?.AllAccounts?.Edges?.Count > 0;

                if (isAuthenticated)
                {
                    Debug.WriteLine($"User '{userName}' logged in successfully");
                }

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }

        public async Task GetAllAccountsAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetallAccounts.ExecuteAsync();

            foreach (var edge in result.Data!.AllAccounts!.Edges)
            {
                var item = edge.Node;
                if (item != null)
                {
                    Debug.WriteLine($"{item.UserName} - {item.Password}");
                }
            }
        }

        public async Task<bool> CreateAccountAsync(string userName, string displayName, string password, int type)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.CreateAccount.ExecuteAsync(userName, displayName, password, type);
            return result.Data?.CreateAccount?.Account?.UserName != null;
        }

        public async Task<int> GetAccountTypeAsync(string userName)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAccountByUserName.ExecuteAsync(userName);
            return result.Data?.AccountByUserName?.Type ?? 0;
        }

        public async Task<Account> GetAccountByUserNameAsync(string userName)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAccountByUserName.ExecuteAsync(userName);
            var acc = result.Data?.AccountByUserName;

            return new Account
            {
                UserName = acc?.UserName ?? "",
                DisplayName = acc?.DisplayName ?? "",
                Type = acc?.Type ?? 0,
                Password = acc?.Password ?? ""
            };
        }
        public async Task<bool> UpdateDisplayNameAsync(string userName, string newDisplayName)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.UpdateAccountDisplayName.ExecuteAsync(userName, newDisplayName);
            return result.Data?.UpdateAccountByUserName?.Account?.DisplayName == newDisplayName;
        }

        public async Task<bool> DeleteAccountByUserNameAsync(string userName)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.DeleteAccountByUserName.ExecuteAsync(userName);
            return result.Data?.DeleteAccountByUserName?.Account?.UserName != null;
        }

    }
}
