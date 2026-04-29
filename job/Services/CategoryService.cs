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
                    TotalJobs = _context.Jobs.Count(j => j.CategoryId == c.Id && (j.ExpiredAt == null || j.ExpiredAt > DateTime.UtcNow) && j.Status == 1)
                }).ToListAsync();
        }

        public async Task<List<FeaturedCategoryCardDto>> GetFeaturedCategories(int count = 6, int days = 30)
        {
            var growthThreshold = DateTime.UtcNow.AddDays(-days);
            var allSkills = await _context.Skills.Select(s => s.Name).ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.Jobs)
                .ThenInclude(j => j.Applications)
                .Select(c => new FeaturedCategoryCardDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    TotalJobs = c.Jobs.Count(j => j.Status == 1),
                    Growth = c.Jobs.Count(j => j.CreatedAt >= growthThreshold && j.Status == 1),
                    SalaryMin = (decimal?)c.Jobs.Where(j => j.Status == 1).Average(j => j.SalaryMin),
                    SalaryMax = (decimal?)c.Jobs.Where(j => j.Status == 1).Average(j => j.SalaryMax),
                    CompetitionRatio = c.Jobs.Any(j => j.Status == 1) 
                        ? (double)c.Jobs.Where(j => j.Status == 1).Sum(j => j.Applications.Count) / c.Jobs.Count(j => j.Status == 1)
                        : 0
                })
                .OrderByDescending(c => c.TotalJobs)
                .Take(count)
                .ToListAsync();

            // Populate TopSkills by analyzing descriptions (simplified approach)
            foreach (var cat in categories)
            {
                var categoryJobs = await _context.Jobs
                    .Where(j => j.CategoryId == cat.Id && j.Status == 1)
                    .Select(j => (j.Title + " " + j.Description).ToLower())
                    .ToListAsync();

                var skillCounts = allSkills
                    .Select(skill => new { Skill = skill, Count = categoryJobs.Count(j => j.Contains(skill.ToLower())) })
                    .Where(x => x.Count > 0)
                    .OrderByDescending(x => x.Count)
                    .Take(3)
                    .Select(x => x.Skill)
                    .ToList();

                cat.TopSkills = skillCounts.Any() ? skillCounts : new List<string> { "General", "Communication" };
            }

            return categories;
        }
    }
}
