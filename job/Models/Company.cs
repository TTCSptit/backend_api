using System;
using System.Collections.Generic;

namespace job.Models;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? LogoUrl { get; set; }

    public string? Description { get; set; }

    public string OwnerUserId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsVerified { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ApplicationUser OwnerUser { get; set; } = null!;
}
