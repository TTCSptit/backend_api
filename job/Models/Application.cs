using System;
using System.Collections.Generic;

namespace job.Models;

public partial class Application
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int JobId { get; set; }

    public string Status { get; set; }  = "Pending";

    public DateTime AppliedAt { get; set; }

    public int? AIScore { get; set; }
    public string? AIStrengths { get; set; }
    public string? AIWeaknesses { get; set; }
    public string? AIReasoning { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
