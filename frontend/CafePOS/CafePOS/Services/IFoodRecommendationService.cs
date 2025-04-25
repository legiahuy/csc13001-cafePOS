using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.DTO;

namespace CafePOS.Services
{
    public interface IFoodRecommendationService
    {
        Task<List<ProductDTO>> GetRecommendedProductsAsync();
    }
} 