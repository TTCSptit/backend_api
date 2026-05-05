using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.EntityFrameworkCore;

namespace job.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly JobPtitContext _context;

        public CompanyService(JobPtitContext context)
        {
            _context = context;
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
                Founded = existingCompany.Founded
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
                Founded = company.Founded
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

            try
            {
                int ret = await _context.SaveChangesAsync();

                return ret > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string?> UploadLogoAsync(int id, string userId, IFormFile logo)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == userId);
            if (company == null) return null;

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
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
