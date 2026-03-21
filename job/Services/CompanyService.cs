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
    }
}
