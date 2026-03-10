using job.Dtos;
using job.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace job.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto dto)
        {
            var result = await _jobService.GetJobCardsAsync(dto);

            if (result.TotalCount == 0)
            {
                return NotFound(ApiResponse<object>.FailureResponse("No jobs found matching your criteria."));
            }

            return Ok(ApiResponse<PagedResult<JobCardDto>>.SuccessResponse(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJob([FromRoute] int id)
        {
            var job = await _jobService.GetJobCardAsync(id);

            if (job == null)
            {
                return NotFound(ApiResponse<object>.FailureResponse($"Job with ID {id} not found."));
            }

            return Ok(ApiResponse<JobCardDto>.SuccessResponse(job));
        }

        [HttpGet("Featured")]
        public async Task<IActionResult> GetFeaturedJobs([FromQuery] int count = 6)
        {
            var featuredJobs = await _jobService.GetFeaturedJobsAsync(count);
            return Ok(ApiResponse<List<JobCardDto>>.SuccessResponse(featuredJobs));
        }

        [Authorize(Roles = "candidate")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetSubmittedJobs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applications = await _jobService.GetApplications(userId);

            if (applications == null || !applications.Any())
            {
                return Ok(ApiResponse<List<ApplicationCardDto>>.SuccessResponse(new List<ApplicationCardDto>(), "You haven't applied for any jobs yet."));
            }

            return Ok(ApiResponse<List<ApplicationCardDto>>.SuccessResponse(applications));
        }
    }
}