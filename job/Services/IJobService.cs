using job.Dtos;
using job.Models;

namespace job.Services
{
    public interface IJobService
    {
        Task<PagedResult<JobCardDto>> GetJobCardsAsync(JobFilterDto filter);
        Task<JobCardDto?> GetJobCardAsync(int id);
        Task<List<JobCardDto>> GetFeaturedJobsAsync(int count = 6);
        Task<bool> ApplyJobAsync(string userId, int jobId);
        Task<EmployerJobOverviewDto?> GetEmployerJobsWithStatsAsync(string userId, string? keyword);
        Task<RecruiterDetailedStatsDto?> GetEmployerDetailedStatsAsync(string userId, int days = 180);
        Task<bool> CreateJobAsync(CreateJobDto dto, string userId);
        Task<bool> UpdateJobAsync(int id, UpdateJobDto dto, string userId);
        Task<bool> DeleteJobAsync(int id, string userId);
    }
}
