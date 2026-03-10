using System;
using System.Collections.Generic;

namespace job.Models;

public partial class WorkExperience
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public virtual CandidateProfile CandidateProfile { get; set; } = null!;
}
