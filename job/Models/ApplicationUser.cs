using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace job.Models;

public partial class ApplicationUser: IdentityUser
{
    public string FullName { get; set; }
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
