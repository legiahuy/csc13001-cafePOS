using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using CafePOS.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IWeatherService _weatherService;
        private readonly IFoodRecommendationService _foodRecommendationService;
        private Frame mainFrame;

        public MainWindow()
        {
            this.InitializeComponent();

            // Initialize the Frame
            mainFrame = new Frame();
            this.Content = mainFrame;

            // Get services from the App's service provider
            var app = Application.Current as App;
            _weatherService = app.ServiceProvider.GetRequiredService<IWeatherService>();
            _foodRecommendationService = app.ServiceProvider.GetRequiredService<IFoodRecommendationService>();

            App.MainAppWindow = this;
            this.Title = "CafePOS";
            mainFrame.Navigate(typeof(LoginPage));
        }
    }
}
