using Microsoft.Extensions.DependencyInjection;
using System;
using CafePOS.GraphQL;

namespace CafePOS.DAO
{
    public class DataProvider
    {

        private static DataProvider? instance;
        public static DataProvider Instance
        {
            get { if (instance == null) instance = new DataProvider(); return instance; }
            private set { instance = value; }
        }

        private readonly string host = "http://localhost:5001/graphql";
        private IServiceProvider _services;
        private ICafePOSClient _client;
        public ICafePOSClient Client => _client;

        private DataProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCafePOSClient().ConfigureHttpClient(
                client => client.BaseAddress = new Uri(host));
            _services = serviceCollection.BuildServiceProvider();

            _client = _services.GetRequiredService<ICafePOSClient>();
        }
    }
}
