using job.Dtos;
using job.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace job.Services
{
    public interface ICompanyService
    {
        Task<CompanyDetailDto?> GetCompanyAsync(int id);
        Task<CompanyDetailDto?> GetMyCompanyAsync(string userId);
        Task<bool> UpdateAsync(int id,string userId, UpdateCompanyRequestDto dto);
        Task<string?> UploadLogoAsync(int id, string userId, IFormFile logo);
    }
}
