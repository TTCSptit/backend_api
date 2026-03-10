using System;
using System.Collections.Generic;

namespace job.Models;

public partial class Application
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int JobId { get; set; }

    public int Status { get; set; }

    public DateTime AppliedAt { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
