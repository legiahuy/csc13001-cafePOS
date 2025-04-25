using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DTO;
using CafePOS.GraphQL;

namespace CafePOS.Services
{
    public class FoodRecommendation
    {
        public string RecommendationReason { get; set; }
        public List<ProductDTO> RecommendedProducts { get; set; }
    }

    public class FoodRecommendationService : IFoodRecommendationService
    {
        private readonly IWeatherService _weatherService;
        private readonly ICafePOSClient _client;

        public FoodRecommendationService(IWeatherService weatherService, ICafePOSClient client)
        {
            _weatherService = weatherService;
            _client = client;
        }

        public async Task<List<ProductDTO>> GetRecommendedProductsAsync()
        {
            try
            {
                var temperature = await _weatherService.GetCurrentTemperatureAsync();
                var weatherCondition = await _weatherService.GetCurrentWeatherConditionAsync();

                var result = await _client.GetAllProducts.ExecuteAsync();
                if (result.Data?.AllProducts?.Edges == null)
                {
                    return new List<ProductDTO>();
                }

                var allProducts = result.Data.AllProducts.Edges
                    .Select(edge => new ProductDTO
                    {
                        Id = edge.Node.Id,
                        Name = edge.Node.Name,
                        Price = edge.Node.Price,
                        Description = edge.Node.Description,
                        ImageUrl = edge.Node.ImageUrl
                    })
                    .ToList();

                // Filter products based on weather conditions
                var recommendedProducts = FilterProductsByWeather(allProducts, temperature, weatherCondition);
                return recommendedProducts.Take(5).ToList(); // Return top 5 recommendations
            }
            catch (Exception)
            {
                return new List<ProductDTO>();
            }
        }

        private List<ProductDTO> FilterProductsByWeather(List<ProductDTO> products, double temperature, string weatherCondition)
        {
            var recommendedProducts = new List<ProductDTO>();

            // Keywords for hot weather
            var hotWeatherKeywords = new[] { "đá", "lạnh", "sinh tố", "nước ép" };
            // Keywords for cold weather
            var coldWeatherKeywords = new[] { "nóng", "ấm", "trà" };

            foreach (var product in products)
            {
                var name = product.Name.ToLower();
                var description = product.Description?.ToLower() ?? "";

                if (temperature > 30)
                {
                    if (hotWeatherKeywords.Any(keyword => name.Contains(keyword) || description.Contains(keyword)))
                    {
                        recommendedProducts.Add(product);
                    }
                }
                else // Cool/Cold weather
                {
                    if (coldWeatherKeywords.Any(keyword => name.Contains(keyword) || description.Contains(keyword)))
                    {
                        recommendedProducts.Add(product);
                    }
                }
            }

            return recommendedProducts;
        }
    }
} 