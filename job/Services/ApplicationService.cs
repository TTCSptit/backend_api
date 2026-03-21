using job.Controllers;
using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace job.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly JobPtitContext _context;

        public ApplicationService(JobPtitContext context)
        {
            _context = context;
        }

        public async Task<FileResult?> GetApplicantCvAsync(int applicationId)
        {
            var profile = await _context.Applications
                .Include(a => a.User)
                .ThenInclude(u => u.CandidateProfile)
                .Select(a => a.User.CandidateProfile)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (profile == null || string.IsNullOrEmpty(profile.Cvurl)) return null;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resumes", profile.Cvurl);
            if (!File.Exists(filePath)) return null;

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(profile.Cvurl, out var contentType))
            {
                contentType = "application/octet-stream"; 
            }

            return new FileResult
            {
                Data = await File.ReadAllBytesAsync(filePath),
                FileName = profile.Cvurl,
                ContentType = contentType
            };
        }

        public async Task<JobApplicantsDashboardDto?> GetJobApplicantsDashboardAsync(int jobId)
        {
            var isOwner = await _context.Jobs.AnyAsync(j => j.Id == jobId);
            if (!isOwner) return null;

            var applicants = await _context.Applications
                .Where(a => a.JobId == jobId)
                .Select(a => new
                {
                    App = a,
                    Profile = _context.CandidateProfiles
                        .Include(p => p.Skills)
                        .Include(p => p.Educations)
                        .Include(p => p.WorkExperiences)
                        .Include(p => p.Skills)
                        .FirstOrDefault(p => p.UserId == a.UserId)
                })
                .Select(x => new ApplicantCardDto
                {
                    Id = x.App.Id,
                    Status = x.App.Status,
                    AppliedAt = x.App.AppliedAt,
                    FullName = x.Profile != null ? x.Profile.FullName : "N/A",
                    Phone = x.Profile != null ? x.Profile.Phone : null,
                    Location = x.Profile != null ? x.Profile.Location : null,
                    AboutMe = x.Profile != null ? x.Profile.AboutMe : null,
                    Cvurl = x.Profile != null ? x.Profile.Cvurl : null,


                    Skills = x.Profile.Skills.Select(s => s.Name).ToList(),
                    WorkExperiences = x.Profile.WorkExperiences.Select(w => new WorkExperience
                    {
                        Id = w.Id,
                        CompanyName = w.CompanyName,
                        Position = w.Position,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate,
                        Description = w.Description
                    }).ToList(),

                })
                .ToListAsync();

            return new JobApplicantsDashboardDto
            {
                Total = applicants.Count,
                Pending = applicants.Count(a => a.Status == "Pending"),
                Interested = applicants.Count(a => a.Status == "Interested"),
                Rejected = applicants.Count(a => a.Status == "Rejected"),
                Applicants = applicants
            };
        }
        /// <summary>
        /// Asynchronously retrieves the collection of jobs submitted by the specified user.
        /// </summary>
        /// <remarks>Each application card includes job details and application status. The returned
        /// collection may be empty if the user has not submitted any job applications.</remarks>
        /// <param name="userId">The unique identifier of the user whose submitted job applications are to be retrieved. Cannot be null or
        /// empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
        /// application cards for the submitted jobs, or null if no applications are found.</returns>
        public async Task<IEnumerable<ApplicationCardDto>?> GetSubmittedJobsAsync(string userId)
        {
            var applications = _context.Applications.Where(a => a.UserId == userId);

            return !applications.Any() ? null : await applications.Include(a => a.Job).ThenInclude(j => j.Company).Select(a => new ApplicationCardDto
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
                    IsNegotiable = a.Job.IsNegotiable,
                    ExpiredAt = a.Job.ExpiredAt
                },
                Status = a.Status,
                AppliedAt = a.AppliedAt
            }).ToListAsync();


        }

        public async Task<bool> UpdateStatus(UpdateApplicationStatusDto dto)
        {
            var existingApplication = await _context.Applications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);

            existingApplication.Status = dto.NewStatus;

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
