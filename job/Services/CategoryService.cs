using job.Data;
using job.Dtos;
using Microsoft.EntityFrameworkCore;

namespace job.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly JobPtitContext _context;

        public CategoryService(JobPtitContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryCardDto>> GetCategories(string? keyword)
        {
            return await _context.Categories
                .Include(c => c.Jobs)
                .Where(c => (keyword == null || c.Name.Contains(keyword)))
                .Select(c => new CategoryCardDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    TotalJobs = c.Jobs.Count(j => j.Status == 1)
                }).ToListAsync();
        }

        public async Task<List<FeaturedCategoryCardDto>> GetFeaturedCategories(int count = 6, int days = 30)
        {
            var growthThreshold = DateTime.UtcNow.AddDays(-days);

            return await _context.Categories
                .Select(c => new FeaturedCategoryCardDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    TotalJobs = c.Jobs.Count(j => j.Status == 1),

                    Growth = c.Jobs.Count(j => j.CreatedAt >= growthThreshold && j.Status == 1)
                })
                .OrderByDescending(c => c.TotalJobs)
                .Take(count)
                .ToListAsync();
        }
    }
}
