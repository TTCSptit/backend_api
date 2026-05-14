using job.Dtos;
using job.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace job.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _profileService.UpdateProfileAsync(userID, dto);

            if (result)
                return Ok(ApiResponse<string>.SuccessResponse("Profile updated successfully"));

            else
                return BadRequest(ApiResponse<string>.FailureResponse("Failed to update profile"));

        }

        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _profileService.GetProfileByUserIdAsync(userId);

            return Ok(ApiResponse<ProfileDto>.SuccessResponse(profile));
        }

        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpPost("upload-cv")]
        public async Task<IActionResult> PostCv([FromForm] FileUploadDto dto)
        { 
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _profileService.UploadCvAsync(userId, dto.Cv);

            if (result)
                return Ok(ApiResponse<string>.SuccessResponse("CV uploaded successfully."));
            return BadRequest(ApiResponse<string>.FailureResponse("Failed to upload CV."));

        }

        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpGet("my-cv")]
        public async Task<IActionResult> DownloadMyCv()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cv = await _profileService.GetCvAsync(userId);

            if (cv == null)
                return NotFound(ApiResponse<string>.FailureResponse("CV not found."));
            return File(cv.Data, cv.ContentType, cv.FileName);
        }
        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var avatarUrl = await _profileService.UploadAvatarAsync(userId, avatar);

            if (avatarUrl != null)
                return Ok(ApiResponse<string>.SuccessResponse(avatarUrl));

            return BadRequest(ApiResponse<string>.FailureResponse("Failed to upload avatar."));
        }
    }
}