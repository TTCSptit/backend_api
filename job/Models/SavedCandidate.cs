using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace job.Models;

public class SavedCandidate
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string RecruiterId { get; set; } = null!;

    [Required]
    public string CandidateId { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }

    [ForeignKey("RecruiterId")]
    public virtual ApplicationUser Recruiter { get; set; } = null!;

    [ForeignKey("CandidateId")]
    public virtual ApplicationUser Candidate { get; set; } = null!;
}
