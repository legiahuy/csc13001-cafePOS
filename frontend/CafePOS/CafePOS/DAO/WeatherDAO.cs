using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CafePOS.DTO;
using DocumentFormat.OpenXml.Bibliography;
using CafePOS.Helpers;


namespace CafePOS.DAO
{
    public class WeatherDAO
    {
        private static WeatherDAO? instance;
        public static WeatherDAO Instance => instance ??= new WeatherDAO();

        private WeatherDAO() { }

        public async Task<Weather?> GetCurrentWeatherAsync()
        {
            var position = await new LocationHelper().GetCurrentLocationAsync();
            if (position == null) return null;

            double lat = position.Coordinate.Point.Position.Latitude;
            double lon = position.Coordinate.Point.Position.Longitude;

            string apiKey = "5b8add793e2c481e973e28b42cdd483c";
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&units=metric";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            return new Weather
            {
                Main = json.RootElement.GetProperty("weather")[0].GetProperty("main").GetString() ?? "",
                TemperatureCelsius = json.RootElement.GetProperty("main").GetProperty("temp").GetDouble()
            };
        }

    }
}
