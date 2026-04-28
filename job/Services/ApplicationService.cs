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
                .Where(a => a.Id == applicationId)
                .Select(a => a.User.CandidateProfile)
                .FirstOrDefaultAsync();

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
            // Kiểm tra xem job có tồn tại không
            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);
            if (!jobExists) return null;

            var applicants = await _context.Applications
                .Where(a => a.JobId == jobId)
                .Include(a => a.User)
                    .ThenInclude(u => u.CandidateProfile)
                        .ThenInclude(p => p.Skills)
                .Include(a => a.User)
                    .ThenInclude(u => u.CandidateProfile)
                        .ThenInclude(p => p.Educations)
                .Include(a => a.User)
                    .ThenInclude(u => u.CandidateProfile)
                        .ThenInclude(p => p.WorkExperiences)
                .Select(a => new ApplicantCardDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Status = a.Status,
                    AppliedAt = a.AppliedAt,
                    FullName = a.User.CandidateProfile != null ? a.User.CandidateProfile.FullName : (a.User.FullName ?? "N/A"),
                    Phone = a.User.CandidateProfile != null ? a.User.CandidateProfile.Phone : null,
                    Location = a.User.CandidateProfile != null ? a.User.CandidateProfile.Location : null,
                    AboutMe = a.User.CandidateProfile != null ? a.User.CandidateProfile.AboutMe : null,
                    Cvurl = a.User.CandidateProfile != null ? a.User.CandidateProfile.Cvurl : null,
                    
                    Skills = a.User.CandidateProfile != null 
                        ? a.User.CandidateProfile.Skills.Select(s => s.Name).ToList() 
                        : new List<string>(),
                        
                    Educations = a.User.CandidateProfile != null 
                        ? a.User.CandidateProfile.Educations.ToList() 
                        : new List<Education>(),
                        
                    WorkExperiences = a.User.CandidateProfile != null 
                        ? a.User.CandidateProfile.WorkExperiences.ToList() 
                        : new List<WorkExperience>()
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

        public async Task<IEnumerable<ApplicationCardDto>?> GetSubmittedJobsAsync(string userId)
        {
            var applications = _context.Applications.Where(a => a.UserId == userId);

            return !applications.Any() ? null : await applications.Include(a => a.Job).ThenInclude(j => j.Company).Select(a => new ApplicationCardDto
            {
                Id = a.Id,
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
            if (existingApplication == null) return false;

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
