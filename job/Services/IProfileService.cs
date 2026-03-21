using job.Dtos;
using job.Models;

namespace job.Services
{
    public interface IProfileService
    {
        Task<FileResult?> GetCvAsync(string? userId);
        Task<ProfileDto?> GetProfileByUserIdAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<bool> UploadCvAsync(string userId, IFormFile cv);
    }
}
