using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Xml;

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

            if (filter.CategoryName is not null)
            {
                jobs = jobs.Where(j => j.Category.Name == filter.CategoryName);
            }

            if (filter.CompanyId is not null)
            {
                jobs = jobs.Where(j => j.CompanyId == filter.CompanyId);
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
                    CompanyId = j.CompanyId,
                    CompanyLogoUrl = j.Company.LogoUrl,
                    Location = j.Location,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    ExpiredAt = j.ExpiredAt,
                    RecruiterId = j.Company.OwnerUserId
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
                CompanyId = job.CompanyId,
                CompanyLogoUrl = job.Company.LogoUrl,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Description = job.Description,
                CategoryId = job.CategoryId,
                JobType = job.JobType,
                IsNegotiable = job.IsNegotiable,
                ExpiredAt = job.ExpiredAt,
                RecruiterId = job.Company.OwnerUserId
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
                    CompanyId = j.CompanyId,
                    CompanyLogoUrl = j.Company.LogoUrl,
                    Location = j.Location,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    ExpiredAt = j.ExpiredAt,
                    RecruiterId = j.Company.OwnerUserId
                })
                .ToListAsync();
        }

        public async Task<List<ApplicationCardDto>> GetApplicationsAsync(string userId)
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
                        CompanyId = a.Job.CompanyId,
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

        public async Task<bool> ApplyJobAsync(string userId, int jobId)
        {
            var application = new Application
            {
                JobId = jobId,
                UserId = userId
            };

            await _context.Applications.AddAsync(application);

            try
            {
                return (await _context.SaveChangesAsync()) > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<EmployerJobOverviewDto?> GetEmployerJobsWithStatsAsync(string userId, string? keyword)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == userId);

            if (company is null) return null;

            var jobs = _context.Jobs.Where(j => j.CompanyId == company.Id);

            if (!string.IsNullOrEmpty(keyword))
                jobs = jobs.Where(j => j.Title.ToLower().Contains(keyword.ToLower()));

            var jobList = await jobs
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new EmployerJobItemDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    JobType = j.JobType,
                    IsNegotiable = j.IsNegotiable,
                    ExpiredAt = j.ExpiredAt,
                    CandidateCount = _context.Applications.Count(a => a.JobId == j.Id),
                    ViewCount = j.ViewsCount
                }).ToListAsync();

            return new EmployerJobOverviewDto
            {
                TotalPostedJobs = await _context.Jobs.CountAsync(j => j.CompanyId == company.Id),
                ActiveJobsCount = await GetJobsAvailable().CountAsync(j => j.CompanyId == company.Id),
                TotalViews = await _context.Jobs.Where(j => j.CompanyId == company.Id).SumAsync(j => j.ViewsCount),
                TotalApplications = await _context.Applications.Where(a => a.Job.CompanyId == company.Id).CountAsync(),
                Jobs = jobList
            };
        }

        public async Task<RecruiterDetailedStatsDto?> GetEmployerDetailedStatsAsync(string userId, int days = 180)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == userId);
            if (company is null) return null;

            var startDate = DateTime.UtcNow.AddDays(-days);

            var applications = await _context.Applications
                .Where(a => a.Job.CompanyId == company.Id && a.AppliedAt >= startDate)
                .ToListAsync();

            var result = new RecruiterDetailedStatsDto();

            // 1. Performance Chart (Applications grouped by month)
            // Group by Month/Year
            var groupedApps = applications
                .GroupBy(a => new { a.AppliedAt.Year, a.AppliedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToList();

            foreach (var group in groupedApps)
            {
                result.PerformanceChart.Labels.Add($"Tháng {group.Key.Month}");
                result.PerformanceChart.ApplicationsData.Add(group.Count());
            }

            // Ensure we have at least some labels if data is sparse
            if (!result.PerformanceChart.Labels.Any())
            {
                result.PerformanceChart.Labels.Add($"Tháng {DateTime.UtcNow.Month}");
                result.PerformanceChart.ApplicationsData.Add(0);
            }

            // 2. Status Chart
            var statusCounts = applications
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Add all standard statuses to ensure chart consistency
            string[] standardStatuses = { "Pending", "Interviewing", "Accepted", "Rejected" };
            foreach (var status in standardStatuses)
            {
                result.StatusChart.Labels.Add(status);
                result.StatusChart.Data.Add(statusCounts.GetValueOrDefault(status, 0));
            }

            // Add any other statuses not in the standard list
            foreach (var kvp in statusCounts)
            {
                if (!standardStatuses.Contains(kvp.Key))
                {
                    result.StatusChart.Labels.Add(kvp.Key);
                    result.StatusChart.Data.Add(kvp.Value);
                }
            }

            return result;
        }

        public async Task<bool> CreateJobAsync(CreateJobDto dto, string userId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == userId);

            if (company == null)
                return false;

            var newJob = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                IsNegotiable = dto.IsNegotiable,
                JobType = dto.JobType,
                CategoryId = dto.CategoryId,
                ExpiredAt = dto.ExpiredAt,
                CompanyId = company.Id,
                Status = 1
            };

            await _context.Jobs.AddAsync(newJob);

            try
            {
                return (await _context.SaveChangesAsync()) > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateJobAsync(int id, UpdateJobDto dto, string userId)
        {
            var existingJob = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id && j.Company.OwnerUserId == userId);

            if (existingJob is null) return false;

            existingJob.Title = dto.Title;
            existingJob.Description = dto.Description;
            existingJob.Location = dto.Location;
            existingJob.SalaryMin = dto.SalaryMin;
            existingJob.SalaryMax = dto.SalaryMax;
            existingJob.IsNegotiable = dto.IsNegotiable;
            existingJob.JobType = dto.JobType;
            existingJob.CategoryId = dto.CategoryId;
            existingJob.ExpiredAt = dto.ExpiredAt;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteJobAsync(int id, string userId)
        {

            var existingJob = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id && j.Company.OwnerUserId == userId);

            if (existingJob is null) return false;

            try
            {
                existingJob.Status = 0;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
