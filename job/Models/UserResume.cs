using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace job.Models;

public class UserResume
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string FileName { get; set; } = null!;

    [Required]
    public string FilePath { get; set; } = null!;

    public string? FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsMain { get; set; } = false;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
