using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Windows.Media.Protection.PlayReady;
using CafePOS.GraphQL;


namespace CafePOS.DAO
{
    public class DataProvider
    {
        private static DataProvider instance;
        public static DataProvider Instance
        {
            get { if (instance == null) instance = new DataProvider(); return DataProvider.instance; }
            private set { DataProvider.instance = value; }
        }

        private string host = "http://localhost:5001/graphql";
        private ICafePOSClient _client;
        private IServiceProvider _services;
        private DataProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCafePOSClient().ConfigureHttpClient(
            client => client.BaseAddress = new Uri(host));
            _services = serviceCollection.BuildServiceProvider();

            _client = _services.GetRequiredService<ICafePOSClient>();
        }
        public void getAllAccounts()
        {
            Debug.WriteLine($"Hello from DAO");

            //var result = await _client.getallAccounts.ExecuteAsync();

            //foreach (var edge in result.Data!.AllAccounts!.Edges)
            //{
            //    var item = edge.Node;
            //    if (item != null)
            //    {
            //        Debug.WriteLine($"{item.UserName} - {item.Password}");
            //    }
            //}
        }

        public async Task<bool> AuthenticateUserAsync(string userName, string password)
        {
            try
            {
                var result = await _client.Login.ExecuteAsync(userName, password);

                bool isAuthenticated = result.Data?.AllAccounts?.Edges?.Count > 0;

                if (isAuthenticated)
                {
                    Debug.WriteLine($"User '{userName}' logged in successfully");
                }

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                // Log any exceptions during authentication
                Debug.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }
    }
}