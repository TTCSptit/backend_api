using job.Dtos;
using job.Models;

namespace job.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryCardDto>> GetCategories(string? keyword);
        Task<List<FeaturedCategoryCardDto>> GetFeaturedCategories(int count = 6, int days = 30);
    }
}
