using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using job.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace job.Data;

public partial class JobPtitContext : IdentityDbContext<ApplicationUser>
{
    public JobPtitContext()
    {
    }

    public JobPtitContext(DbContextOptions<JobPtitContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<CandidateProfile> CandidateProfiles { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<WorkExperience> WorkExperiences { get; set; }
}
