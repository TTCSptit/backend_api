using System;
using System.Collections.Generic;

namespace job.Models;

public partial class CandidateProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Title { get; set; }

    public string? Phone { get; set; }

    public string? Location { get; set; }

    public string? AboutMe { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Cvurl { get; set; }

    public virtual ICollection<Education> Educations { get; set; } = new List<Education>();

    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
