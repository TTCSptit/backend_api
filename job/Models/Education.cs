using System;
using System.Collections.Generic;

namespace job.Models;

public partial class Education
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string? Degree { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual CandidateProfile CandidateProfile { get; set; } = null!;
}
