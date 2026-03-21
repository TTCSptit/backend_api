using job.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CandidateProfile>()
        .HasMany(p => p.Skills)
        .WithMany(s => s.CandidateProfiles)
        .UsingEntity<Dictionary<string, object>>(
            "CandidateProfileSkills", 
            j => j.HasOne<Skill>().WithMany().HasForeignKey("SkillId"), 
            j => j.HasOne<CandidateProfile>().WithMany().HasForeignKey("CandidateProfileId")
    );
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
