using job.Dtos;
using job.Models;

namespace job.Services
{
    public interface IJobService
    {
        Task<PagedResult<JobCardDto>> GetJobCardsAsync(JobFilterDto filter);
        Task<JobCardDto?> GetJobCardAsync(int id);
        Task<List<JobCardDto>> GetFeaturedJobsAsync(int count = 6);
        Task<List<ApplicationCardDto>> GetApplications(string userId);
    }
}
