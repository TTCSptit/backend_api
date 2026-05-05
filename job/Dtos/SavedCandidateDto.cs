using System;

namespace job.Dtos;

public class SavedCandidateDto
{
    public int Id { get; set; }
    public string CandidateId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime SavedAt { get; set; }
    public string? Note { get; set; }
    public string? Role { get; set; }
    public string? Location { get; set; }
    public string? CvUrl { get; set; }
}

public class CreateSavedCandidateDto
{
    public string CandidateId { get; set; } = null!;
    public string? Note { get; set; }
}
