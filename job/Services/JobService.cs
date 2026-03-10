using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace job.Services
{
    public class JobService : IJobService
    {
        private readonly JobPtitContext _context;

        public JobService(JobPtitContext context)
        {
            _context = context;
        }

        private IQueryable<Job> GetJobsAvailable()
        {
            return _context.Jobs
                        .Where(j => (j.ExpiredAt == null || j.ExpiredAt > DateTime.UtcNow) && j.Status == 1)
                        .Include(j => j.Company)
                        .Include(j => j.Category)
                        .AsQueryable();
        }

        public async Task<PagedResult<JobCardDto>> GetJobCardsAsync(JobFilterDto filter)
        {
            var jobs = GetJobsAvailable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                jobs = jobs.Where(j =>
                    j.Title.Contains(filter.Keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                jobs = jobs.Where(j => j.Location == filter.Location);
            }

            if (filter.CategorySlug is not null)
            {
                jobs = jobs.Where(j => j.Category.Slug == filter.CategorySlug);
            }

            if (filter.JobType is not null)
            {
                jobs = jobs.Where(j => j.JobType == filter.JobType);
            }

            if (filter.MinSalary is not null)
            {
                jobs = jobs.Where(j =>
                    j.SalaryMin >= filter.MinSalary);
            }

            if (filter.MaxSalary is not null)
            {
                jobs = jobs.Where(j =>
                    j.SalaryMax <= filter.MaxSalary);
            }

            var totalCount = await jobs.CountAsync();

            var items = await jobs
                .OrderByDescending(j => j.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(j => new JobCardDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.Company.Name,
                    CompanyLogoUrl = j.Company.LogoUrl,
                    Location = j.Location,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    ExpiredAt = j.ExpiredAt
                })
                .ToListAsync();

            return new PagedResult<JobCardDto>
            {
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = items
            };
        }

        public async Task<JobCardDto?> GetJobCardAsync(int id)
        {
            var job = await GetJobsAvailable().SingleOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return null;

            job.ViewsCount += 1;
            await _context.SaveChangesAsync();

            return new JobCardDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.Company.Name,
                CompanyLogoUrl = job.Company.LogoUrl,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                ExpiredAt = job.ExpiredAt
            };
        }

        public async Task<List<JobCardDto>> GetFeaturedJobsAsync(int count = 6)
        {
            var jobs = GetJobsAvailable();
            return await jobs
                .OrderByDescending(j => j.ViewsCount)
                .ThenByDescending(j => j.CreatedAt)
                .Take(count)
                .Select(j => new JobCardDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.Company.Name,
                    CompanyLogoUrl = j.Company.LogoUrl,
                    Location = j.Location,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    ExpiredAt = j.ExpiredAt
                })
                .ToListAsync();
        }

        public async Task<List<ApplicationCardDto>> GetApplications(string userId)
        {
            return await _context.Applications
                .Include(a => a.Job).ThenInclude(j => j.Company)
                .Where(a => a.UserId == userId)
                .Select(a => new ApplicationCardDto
                {
                    JobCardDto = new JobCardDto
                    {
                        Id = a.JobId,
                        Title = a.Job.Title,
                        CompanyName = a.Job.Company.Name,
                        CompanyLogoUrl = a.Job.Company.LogoUrl,
                        Location = a.Job.Location,
                        SalaryMin = a.Job.SalaryMin,
                        SalaryMax = a.Job.SalaryMax,
                        JobType = a.Job.JobType,
                        IsNegotiable = a.Job.IsNegotiable
                    },
                    Status = a.Status,
                    AppliedAt = a.AppliedAt
                }).ToListAsync();
        }
    }
}
