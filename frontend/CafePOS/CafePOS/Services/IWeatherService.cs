using System.Threading.Tasks;

namespace CafePOS.Services
{
    public interface IWeatherService
    {
        Task<double> GetCurrentTemperatureAsync();
        Task<string> GetCurrentWeatherConditionAsync();
    }
} 