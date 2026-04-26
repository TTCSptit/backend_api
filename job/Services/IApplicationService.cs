using job.Dtos;
using Microsoft.AspNetCore.Mvc;


namespace job.Services
{
    public interface IApplicationService
    {
        Task<FileResult?> GetApplicantCvAsync(int applicationId);
        Task<JobApplicantsDashboardDto?> GetJobApplicantsDashboardAsync(int jobId);
        Task<IEnumerable<ApplicationCardDto>?> GetSubmittedJobsAsync(string userId);
        Task<bool> UpdateStatus(UpdateApplicationStatusDto dto);
    }
}
