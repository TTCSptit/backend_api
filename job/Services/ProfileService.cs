using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace job.Services
{
    public class ProfileService : IProfileService
    {
        private readonly JobPtitContext _context;

        public ProfileService(JobPtitContext context)
        {
            _context = context;
        }

        public async Task<FileResult?> GetCvAsync(string? userId)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);


            if (profile == null || profile.Cvurl == null)
                return null;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resumes", profile.Cvurl);

            if (!File.Exists(filePath))
                return null;

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

        public async Task<ProfileDto?> GetProfileByUserIdAsync(string userId)
        {
            var profile = await _context.CandidateProfiles
                .Include(c => c.Educations)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return profile == null ? null : new ProfileDto
            {
                Id = profile.Id,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Email = profile.Email,
                Location = profile.Location,
                AboutMe = profile.AboutMe,
                Cvurl = profile.Cvurl,
                Educations = profile.Educations.Select(e => new EducationDto
                {
                    Id = e.Id,
                    SchoolName = e.SchoolName,
                    Degree = e.Degree,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate
                }).ToList(),
                WorkExperiences = profile.WorkExperiences.Select(w => new WorkExperienceDto
                {
                    Id = w.Id,
                    CompanyName = w.CompanyName,
                    Position = w.Position,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    Description = w.Description
                }).ToList(),
                Skills = profile.Skills.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList()
            };

        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var profile = await _context.CandidateProfiles
                    .Include(c => c.Educations)
                    .Include(c => c.WorkExperiences)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return false;


            profile.FullName = dto.FullName;
            profile.Phone = dto.Phone;
            profile.Email = dto.Email;
            profile.Location = dto.Location;
            profile.AboutMe = dto.AboutMe;

            var existingEducationDict = profile.Educations.ToDictionary(e => e.Id);

            foreach (var education in dto.Educations)
            {
                if (education.Id > 0 && existingEducationDict.TryGetValue(education.Id.Value, out var existingEntity))
                {
                    _context.Entry(existingEntity).CurrentValues.SetValues(education);

                    existingEducationDict.Remove(education.Id.Value);
                }
                else
                {
                    profile.Educations.Add(new Education
                    {
                        SchoolName = education.SchoolName,
                        Degree = education.Degree,
                        StartDate = education.StartDate,
                        EndDate = education.EndDate
                    });
                }
            }

            _context.Educations.RemoveRange(existingEducationDict.Values);

            var existingWorkExperienceDict = profile.WorkExperiences.ToDictionary(e => e.Id);

            foreach (var workExperience in dto.WorkExperiences)
            {
                if (workExperience.Id > 0 && existingWorkExperienceDict.TryGetValue(workExperience.Id.Value, out var existingEntity))
                {
                    _context.Entry(existingEntity).CurrentValues.SetValues(workExperience);
                    existingWorkExperienceDict.Remove(workExperience.Id.Value);
                }
                else
                {
                    profile.WorkExperiences.Add(new WorkExperience
                    {
                        CompanyName = workExperience.CompanyName,
                        Position = workExperience.Position,
                        StartDate = workExperience.StartDate,
                        EndDate = workExperience.EndDate
                    });
                }
            }

            _context.WorkExperiences.RemoveRange(existingWorkExperienceDict.Values);

            var existingSkillDict = profile.Skills.ToDictionary(s => s.Id);

            foreach (var skill in dto.Skills)
            {
                if (skill.Id > 0 && existingSkillDict.TryGetValue(skill.Id.Value, out var existingEntity))
                {
                    _context.Entry(existingEntity).CurrentValues.SetValues(skill);
                    existingSkillDict.Remove(skill.Id.Value);
                }
                else
                {
                    profile.Skills.Add(new Skill
                    {
                        Name = skill.Name
                    });
                }
            }

            _context.Skills.RemoveRange(existingSkillDict.Values);

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }


        }

        public async Task<bool> UploadCvAsync(string userId, IFormFile cv)
        {
            var profile = _context.CandidateProfiles.FirstOrDefault(p => p.UserId == userId);
            if (profile == null)
                return false;

            if (profile.Cvurl != null)
            {
                var existingCvFile = Path.Combine(Directory.GetCurrentDirectory(), "Resumes", profile.Cvurl);

                if (File.Exists(existingCvFile))
                {
                    File.Delete(existingCvFile);
                }
            }

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Resumes");

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            string fileExtension = Path.GetExtension(cv.FileName);


            string newFileName = new StringBuilder()
                .Append($"{profile.FullName}-JobPtit-{DateTime.Now.ToString("ddMMyy.hhmmss")}")
                .Append(fileExtension).ToString();

            string filePath = Path.Combine(rootPath, newFileName);


            try
            {
                using (var Stream = new FileStream(filePath, FileMode.Create))
                {
                    await cv.CopyToAsync(Stream);
                }

                profile.Cvurl = newFileName;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                if (File.Exists(filePath)) File.Delete(filePath);

                return false;
            }
        }


    }
}
