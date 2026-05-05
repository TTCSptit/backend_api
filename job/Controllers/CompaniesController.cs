using job.Dtos;
using job.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace job.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var company = await _companyService.GetCompanyAsync(id);

            if (company is null)
                return NotFound(ApiResponse<object>.FailureResponse(""));
            return Ok(ApiResponse<CompanyDetailDto>.SuccessResponse(company));
        }

        [Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
        [HttpGet("my-company")]
        public async Task<IActionResult> GetMyCompany()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var company = await _companyService.GetMyCompanyAsync(currentUserId);

            if (company is null)
                return NotFound(ApiResponse<object>.FailureResponse("Company not found."));

            return Ok(ApiResponse<CompanyDetailDto>.SuccessResponse(company));
        }

        [Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyRequestDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var result = await _companyService.UpdateAsync(id, currentUserId, dto);

            if (result)
                return NoContent();

            return NotFound(ApiResponse<object>.FailureResponse("Company not found or you do not have permission."));
        }

        [Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
        [HttpPost("{id:int}/upload-logo")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile logo)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var logoUrl = await _companyService.UploadLogoAsync(id, currentUserId, logo);

            if (logoUrl is not null)
                return Ok(ApiResponse<string>.SuccessResponse(logoUrl));

            return BadRequest(ApiResponse<object>.FailureResponse("Failed to upload logo."));
        }
    }
}
