using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
    }
}
