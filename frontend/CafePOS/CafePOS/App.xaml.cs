using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CafePOS.Services;
using CafePOS.GraphQL;
using CafePOS.DAO;
using Windows.Storage;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace CafePOS
{

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider _serviceProvider;
        private static IConfiguration _configuration;
        private Window m_window;

        public static Window MainAppWindow { get; set; }

        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            _serviceProvider = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IWeatherService, WeatherService>();
            services.AddSingleton<IFoodRecommendationService, FoodRecommendationService>();
            services.AddSingleton<IGeminiService>(_ => new GeminiService("AIzaSyCnAQZDxfKWruStV1i0MGOH6fHVuQKMNiM"));  // Replace with your Gemini API key
            services.AddCafePOSClient().ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost:5001/graphql"));

            return services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            MainAppWindow = m_window;
            m_window.Activate();
        }
    }
}