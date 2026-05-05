using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace job.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "recruiter, Recruiter, RECRUITER")]
public class SavedCandidatesController : ControllerBase
{
    private readonly JobPtitContext _context;

    public SavedCandidatesController(JobPtitContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedCandidates()
    {
        var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

        var saved = await _context.SavedCandidates
            .Where(sc => sc.RecruiterId == recruiterId)
            .Include(sc => sc.Candidate)
                .ThenInclude(u => u.CandidateProfile)
            .OrderByDescending(sc => sc.SavedAt)
            .Select(sc => new SavedCandidateDto
            {
                Id = sc.Id,
                CandidateId = sc.CandidateId,
                FullName = sc.Candidate.FullName,
                Email = sc.Candidate.Email,
                SavedAt = sc.SavedAt,
                Note = sc.Note,
                Role = "Ứng viên từ hệ thống",
                Location = sc.Candidate.CandidateProfile != null ? sc.Candidate.CandidateProfile.Location : "Chưa cập nhật",
                CvUrl = sc.Candidate.CandidateProfile != null ? sc.Candidate.CandidateProfile.Cvurl : null
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<SavedCandidateDto>>.SuccessResponse(saved));
    }

    [HttpPost]
    public async Task<IActionResult> SaveCandidate([FromBody] CreateSavedCandidateDto dto)
    {
        var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

        // Check if already saved
        var existing = await _context.SavedCandidates
            .FirstOrDefaultAsync(sc => sc.RecruiterId == recruiterId && sc.CandidateId == dto.CandidateId);
        
        if (existing != null)
            return BadRequest(ApiResponse<object>.FailureResponse("Candidate already saved."));

        var savedCandidate = new SavedCandidate
        {
            RecruiterId = recruiterId,
            CandidateId = dto.CandidateId,
            Note = dto.Note,
            SavedAt = DateTime.UtcNow
        };

        _context.SavedCandidates.Add(savedCandidate);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(null, "Candidate saved successfully."));
    }

    [HttpDelete("{candidateId}")]
    public async Task<IActionResult> UnsaveCandidate(string candidateId)
    {
        var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

        var saved = await _context.SavedCandidates
            .FirstOrDefaultAsync(sc => sc.RecruiterId == recruiterId && sc.CandidateId == candidateId);

        if (saved == null)
            return NotFound(ApiResponse<object>.FailureResponse("Saved candidate not found."));

        _context.SavedCandidates.Remove(saved);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(null, "Candidate unsaved successfully."));
    }
}
