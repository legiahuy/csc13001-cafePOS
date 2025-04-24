using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace CafePOS.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private const string API_KEY = "a9285daa7c65bb69afd3eda4ea072d2b";
        private const string BASE_URL = "https://api.openweathermap.org/data/2.5/weather";
        private const string CITY = "Ho Chi Minh City";
        private const string COUNTRY_CODE = "VN";

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<double> GetCurrentTemperatureAsync()
        {
            try
            {
                var weatherData = await GetWeatherDataAsync();
                var jsonDocument = JsonDocument.Parse(weatherData);
                var root = jsonDocument.RootElement;

                return root.GetProperty("main").GetProperty("temp").GetDouble();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting temperature: {ex.Message}");
                return 30.0; // Fallback temperature if API call fails
            }
        }

        public async Task<string> GetCurrentWeatherConditionAsync()
        {
            try
            {
                var weatherData = await GetWeatherDataAsync();
                var jsonDocument = JsonDocument.Parse(weatherData);
                var root = jsonDocument.RootElement;

                return root.GetProperty("weather")[0].GetProperty("main").GetString().ToLower();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting weather condition: {ex.Message}");
                return "unknown"; // Fallback condition if API call fails
            }
        }

        private async Task<string> GetWeatherDataAsync()
        {
            try
            {
                var url = $"{BASE_URL}?q={CITY},{COUNTRY_CODE}&appid={API_KEY}&units=metric";
                return await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching weather data: {ex.Message}");
            }
        }
    }
} 