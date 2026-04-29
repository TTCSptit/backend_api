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
            try
            {
                var growthThreshold = DateTime.UtcNow.AddDays(-days);
                var allSkills = await _context.Skills.Select(s => s.Name).ToListAsync();

                // Step 1: Fetch basic category data
                var categoriesData = await _context.Categories
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Slug,
                        TotalJobs = c.Jobs.Count(j => j.Status == 1)
                    })
                    .OrderByDescending(c => c.TotalJobs)
                    .Take(count)
                    .ToListAsync();

                var categoryIds = categoriesData.Select(c => c.Id).ToList();

                // Step 2: Fetch all relevant jobs for these categories to calculate stats in-memory
                var allJobs = await _context.Jobs
                    .Where(j => categoryIds.Contains(j.CategoryId) && j.Status == 1)
                    .Select(j => new
                    {
                        j.CategoryId,
                        j.SalaryMin,
                        j.SalaryMax,
                        j.CreatedAt,
                        AppCount = j.Applications.Count,
                        SearchText = (j.Title + " " + (j.Description ?? "")).ToLower()
                    })
                    .ToListAsync();

                var lowerSkills = allSkills.Select(s => s.ToLower()).ToList();
                var results = new List<FeaturedCategoryCardDto>();

                foreach (var cat in categoriesData)
                {
                    var catJobs = allJobs.Where(j => j.CategoryId == cat.Id).ToList();
                    
                    var dto = new FeaturedCategoryCardDto
                    {
                        Id = cat.Id,
                        Name = cat.Name,
                        Slug = cat.Slug,
                        TotalJobs = cat.TotalJobs,
                        Growth = catJobs.Count(j => j.CreatedAt >= growthThreshold),
                        SalaryMin = catJobs.Any(j => j.SalaryMin.HasValue) ? (decimal)catJobs.Where(j => j.SalaryMin.HasValue).Average(j => j.SalaryMin.Value) : 0,
                        SalaryMax = catJobs.Any(j => j.SalaryMax.HasValue) ? (decimal)catJobs.Where(j => j.SalaryMax.HasValue).Average(j => j.SalaryMax.Value) : 0,
                        CompetitionRatio = catJobs.Any() ? catJobs.Average(j => (double)j.AppCount) : 0
                    };

                    // Skill extraction
                    var combinedText = string.Join(" ", catJobs.Select(j => j.SearchText));
                    if (!string.IsNullOrEmpty(combinedText))
                    {
                        dto.TopSkills = allSkills
                            .Zip(lowerSkills, (name, lower) => new { Name = name, Lower = lower })
                            .Where(x => combinedText.Contains(x.Lower))
                            .Take(3)
                            .Select(x => x.Name)
                            .ToList();
                    }

                    if (dto.TopSkills == null || !dto.TopSkills.Any())
                        dto.TopSkills = new List<string> { "General", "Communication" };

                    results.Add(dto);
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetFeaturedCategories Error]: {ex.Message}");
                return new List<FeaturedCategoryCardDto>();
            }
        }
    }
}
