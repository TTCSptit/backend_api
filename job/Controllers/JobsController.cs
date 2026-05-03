using job.Dtos;
using job.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
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

        [Authorize(Roles = "Candidate, candidate, CANDIDATE")]
        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto dto)
        {
            var result = await _jobService.GetJobCardsAsync(dto);
            return Ok(ApiResponse<PagedResult<JobCardDto>>.SuccessResponse(result));
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJob([FromRoute] int id)
        {
            var job = await _jobService.GetJobCardAsync(id);

            if (job == null)
                return NotFound(ApiResponse<object>.FailureResponse($"Job with ID {id} not found."));

            return Ok(ApiResponse<JobCardDto>.SuccessResponse(job));
        }

        [Authorize(Roles = "Candidate, candidate, CANDIDATE")]
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedJobs([FromQuery] int count = 6)
        {
            var featuredJobs = await _jobService.GetFeaturedJobsAsync(count);
            return Ok(ApiResponse<List<JobCardDto>>.SuccessResponse(featuredJobs));
        }

        [Authorize(Roles = "Candidate, candidate, CANDIDATE")]
        [HttpPost("{id:int}/apply")]
        public async Task<IActionResult> ApplyJob([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var result = await _jobService.ApplyJobAsync(userId, id);

            if (!result)
                return BadRequest(ApiResponse<object>.FailureResponse("You have already applied or the job is no longer available."));

            return Ok(ApiResponse<object>.SuccessResponse("Application submitted successfully!"));
        }

        [Authorize(Roles = "Recruiter, recruiter, RECRUITER")]
        [HttpGet("manage-listings")]
        public async Task<IActionResult> GetEmployerJobsWithStats([FromQuery] string? keyword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var overviewData =  await _jobService.GetEmployerJobsWithStatsAsync(userId, keyword);

            if (overviewData is null)
                return NotFound(ApiResponse<object>.FailureResponse("You haven't posted any jobs yet."));

            return Ok(ApiResponse<EmployerJobOverviewDto>.SuccessResponse(overviewData));
        }

        [Authorize(Roles = "Recruiter, recruiter, RECRUITER")]
        [HttpGet("detailed-stats")]
        public async Task<IActionResult> GetEmployerDetailedStats([FromQuery] int days = 180)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = await _jobService.GetEmployerDetailedStatsAsync(userId, days);

            if (stats is null)
                return NotFound(ApiResponse<object>.FailureResponse("Company not found."));

            return Ok(ApiResponse<RecruiterDetailedStatsDto>.SuccessResponse(stats));
        }


        [Authorize(Roles = "Recruiter, recruiter, RECRUITER")]
        [HttpPost]
        public async Task<IActionResult> PostJob([FromBody] CreateJobDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _jobService.CreateJobAsync(dto, userId);

            if (!result)
            {
                return BadRequest(ApiResponse<object>.FailureResponse("Post job failed. Please try again."));
            }
            return Ok(ApiResponse<object>.SuccessResponse(null, "Job posted successfully"));
        }

        [Authorize(Roles = "Recruiter, recruiter, RECRUITER")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _jobService.UpdateJobAsync(id, dto, userId);

            if (!result)
            {
                return NotFound(ApiResponse<object>.FailureResponse("Job not found or permission denied."));
            }

            return NoContent();
        }

        [Authorize(Roles = "Recruiter, recruiter, RECRUITER")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _jobService.DeleteJobAsync(id, userId);

            if(!result)
                return NotFound(ApiResponse<object>.FailureResponse("Job not found or permission denied."));

            return NoContent();
        }
    }
}