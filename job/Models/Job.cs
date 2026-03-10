using System;
using System.Collections.Generic;

namespace job.Models;

public partial class Job
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public int? SalaryMin { get; set; }

    public int? SalaryMax { get; set; }

    public bool IsNegotiable { get; set; }

    public int JobType { get; set; }

    public int Status { get; set; }

    public int CategoryId { get; set; }

    public int CompanyId { get; set; }

    public string CreatedByUserId { get; set; } = null!;

    public int ViewsCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Category Category { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual ApplicationUser CreatedByUser { get; set; } = null!;
}
