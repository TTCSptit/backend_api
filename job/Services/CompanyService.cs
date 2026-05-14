using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace job.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly JobPtitContext _context;
        private readonly IWebHostEnvironment _environment;

        public CompanyService(JobPtitContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<CompanyDetailDto?> GetCompanyAsync(int id)
        {
            var existingCompany = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

            if (existingCompany is null)
                return null;
            return new CompanyDetailDto
            {
                Id = existingCompany.Id,
                Name = existingCompany.Name,
                Location = existingCompany.Location,
                WebsiteUrl = existingCompany.WebsiteUrl,
                Email = existingCompany.Email,
                PhoneNumber = existingCompany.PhoneNumber,
                LogoUrl = existingCompany.LogoUrl,
                Description = existingCompany.Description,
                IsVerified = existingCompany.IsVerified,
                Industry = existingCompany.Industry,
                Size = existingCompany.Size,
                Founded = existingCompany.Founded,
                Benefits = string.IsNullOrEmpty(existingCompany.Benefits) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(existingCompany.Benefits)
            };
        }

        public async Task<CompanyDetailDto?> GetMyCompanyAsync(string userId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == userId);

            if (company is null)
                return null;

            return new CompanyDetailDto
            {
                Id = company.Id,
                Name = company.Name,
                Location = company.Location,
                WebsiteUrl = company.WebsiteUrl,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber,
                LogoUrl = company.LogoUrl,
                Description = company.Description,
                IsVerified = company.IsVerified,
                Industry = company.Industry,
                Size = company.Size,
                Founded = company.Founded,
                Benefits = string.IsNullOrEmpty(company.Benefits) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(company.Benefits)
            };
        }

        public async Task<bool> UpdateAsync(int id, string userId, UpdateCompanyRequestDto dto)
        {
            var existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == userId);

            if (existingCompany is null) return false;

            existingCompany.Name = dto.Name;
            existingCompany.Location = dto.Location;
            existingCompany.WebsiteUrl = dto.WebsiteUrl;
            existingCompany.Email = dto.Email;
            existingCompany.PhoneNumber = dto.PhoneNumber;
            existingCompany.LogoUrl = dto.LogoUrl;
            existingCompany.Description = dto.Description;
            existingCompany.Industry = dto.Industry;
            existingCompany.Size = dto.Size;
            existingCompany.Founded = dto.Founded;
            existingCompany.Benefits = dto.Benefits != null 
                ? JsonSerializer.Serialize(dto.Benefits) 
                : null;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating company: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> UploadLogoAsync(int id, string userId, IFormFile logo)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == userId);
            if (company == null) return null;

            string wwwRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(wwwRootPath))
            {
                wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            string rootPath = Path.Combine(wwwRootPath, "uploads", "logos");
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            string fileExtension = Path.GetExtension(logo.FileName);
            string newFileName = $"logo-{id}-{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            string filePath = Path.Combine(rootPath, newFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }

                // Delete old logo if it was local
                if (!string.IsNullOrEmpty(company.LogoUrl) && company.LogoUrl.Contains("/uploads/logos/"))
                {
                    string oldFileName = Path.GetFileName(company.LogoUrl);
                    string oldFilePath = Path.Combine(rootPath, oldFileName);
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }

                string logoUrl = $"/uploads/logos/{newFileName}";
                company.LogoUrl = logoUrl;
                await _context.SaveChangesAsync();
                return logoUrl;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
