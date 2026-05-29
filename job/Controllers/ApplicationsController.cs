using job.Dtos;
using job.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var applications = await _applicationService.GetSubmittedJobsAsync(userId);

            if (applications == null) return NotFound(ApiResponse<object>.FailureResponse("No applications found."));

            return Ok(ApiResponse<IEnumerable<ApplicationCardDto>>.SuccessResponse(applications));
        }

        [Authorize(Roles = "candidate, Candidate, CANDIDATE")]
        [HttpDelete("{applicationId:int}")]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _applicationService.DeleteApplicationAsync(applicationId, userId);

            if (!result) return BadRequest(ApiResponse<object>.FailureResponse("Failed to withdraw application."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Application withdrawn successfully."));
        }

        [Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
        [HttpGet("job/{jobId:int}")]
        public async Task<IActionResult> GetApplicants(int jobId)
        {
            var jobApplicantsDashboard = await _applicationService.GetJobApplicantsDashboardAsync(jobId);

            if (jobApplicantsDashboard == null)
                return NotFound(ApiResponse<object>.FailureResponse("No applicants found for this job."));

            return Ok(ApiResponse<JobApplicantsDashboardDto>.SuccessResponse(jobApplicantsDashboard));
        }

        [Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateApplicationStatusDto dto)
        {
            var result = await _applicationService.UpdateStatus(dto);

            if (!result)
                return BadRequest(ApiResponse<object>.FailureResponse("Failed to update status."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Status updated successfully."));
        }

        [HttpPut("update-ai-score")]
        public async Task<IActionResult> UpdateAiScore([FromBody] UpdateAiScoreDto dto)
        {
            var result = await _applicationService.UpdateAiScore(dto);

            if (!result)
                return BadRequest(ApiResponse<object>.FailureResponse("Failed to update AI score."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "AI Score updated successfully."));
        }

        [Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
        [HttpGet("{applicationId:int}/cv")]
        public async Task<IActionResult> GetApplicantCv(int applicationId)
        {
            var cvFile = await _applicationService.GetApplicantCvAsync(applicationId);
            if (cvFile == null)
                return NotFound(ApiResponse<object>.FailureResponse("CV not found for this application."));
            return File(cvFile.Data, cvFile.ContentType, cvFile.FileName);
        }
    }
}
