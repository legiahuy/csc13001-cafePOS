using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafePOS.Helpers
{
    public static class WeatherClassifier
    {
        public static string ClassifyWeather(string mainWeather, double temperatureCelsius)
        {
            mainWeather = mainWeather.ToLower();

            if (temperatureCelsius >= 30)
                return "nóng";
            if (temperatureCelsius <= 20)
                return "lạnh";

            return "mát";
        }

        public static string ToVietnamese(string type)
        {
            return type switch
            {
                "nóng" => "nắng nóng",
                "lạnh" => "lạnh",
                "mát" => "mát mẻ",
                _ => "không xác định"
            };
        }
    }
}