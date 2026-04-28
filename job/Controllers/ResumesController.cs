using System.Security.Claims;
using job.Data;
using job.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace job.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ResumesController : ControllerBase
{
    private readonly JobPtitContext _context;
    private readonly string _resumeFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resumes");

    public ResumesController(JobPtitContext context)
    {
        _context = context;
        if (!Directory.Exists(_resumeFolder))
        {
            Directory.CreateDirectory(_resumeFolder);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyResumes()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resumes = await _context.UserResumes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();

        return Ok(resumes);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadResume(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File không hợp lệ");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Tạo tên file duy nhất
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_resumeFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var hasExistingResumes = await _context.UserResumes.AnyAsync(r => r.UserId == userId);

        var resume = new UserResume
        {
            UserId = userId!,
            FileName = file.FileName,
            FilePath = fileName,
            FileSize = (file.Length / 1024.0 / 1024.0).ToString("F2") + " MB",
            UploadedAt = DateTime.UtcNow,
            IsMain = !hasExistingResumes // Nếu là file đầu tiên thì đặt là CV chính
        };

        _context.UserResumes.Add(resume);
        
        // Cập nhật Cvurl trong CandidateProfile nếu đây là CV chính
        if (resume.IsMain)
        {
            var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                profile.Cvurl = fileName;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(resume);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResume(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resume = await _context.UserResumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (resume == null) return NotFound();

        // Xóa file vật lý
        var filePath = Path.Combine(_resumeFolder, resume.FilePath);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.UserResumes.Remove(resume);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id}/set-main")]
    public async Task<IActionResult> SetMainResume(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resumes = await _context.UserResumes.Where(r => r.UserId == userId).ToListAsync();
        var target = resumes.FirstOrDefault(r => r.Id == id);

        if (target == null) return NotFound();

        foreach (var r in resumes)
        {
            r.IsMain = (r.Id == id);
        }

        // Cập nhật vào profile
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.Cvurl = target.FilePath;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadResume(int id)
    {
        var resume = await _context.UserResumes.FirstOrDefaultAsync(r => r.Id == id);
        if (resume == null) return NotFound();

        var filePath = Path.Combine(_resumeFolder, resume.FilePath);
        if (!System.IO.File.Exists(filePath)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/octet-stream", resume.FileName);
    }
}
