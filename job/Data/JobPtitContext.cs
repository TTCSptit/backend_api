using job.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        builder.Entity<CandidateProfile>(entity =>
        {
            entity.ToTable("CandidateProfiles");
            entity.HasKey(e => e.Id);
            entity.HasOne(p => p.User)
                  .WithOne(u => u.CandidateProfile)
                  .HasForeignKey<CandidateProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        builder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasOne(c => c.OwnerUser)
                  .WithOne()
                  .HasForeignKey<Company>(c => c.OwnerUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.OwnerUserId).IsUnique();
        });

        builder.Entity<Application>(entity =>
        {
            entity.ToTable("Applications");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Applications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Applications)
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.JobId }).IsUnique().HasDatabaseName("UQ_User_Job");
        });

        builder.Entity<CandidateProfile>()
            .HasMany(p => p.Skills)
            .WithMany(s => s.CandidateProfiles)
            .UsingEntity<Dictionary<string, object>>(
                "CandidateProfileSkills",
                j => j.HasOne<Skill>().WithMany().HasForeignKey("SkillId"),
                j => j.HasOne<CandidateProfile>().WithMany().HasForeignKey("CandidateProfileId"),
                j =>
                {
                    j.HasKey("CandidateProfileId", "SkillId");
                    j.ToTable("CandidateProfileSkills");
                }
            );

        builder.Entity<Job>(entity =>
        {
            entity.ToTable("Jobs");
            entity.HasOne(d => d.Category)
                  .WithMany(p => p.Jobs)
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Jobs)
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Education>(entity =>
        {
            entity.ToTable("Educations");
            entity.HasOne(d => d.CandidateProfile)
                  .WithMany(p => p.Educations)
                  .HasForeignKey(d => d.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorkExperience>(entity =>
        {
            entity.ToTable("WorkExperiences");
            entity.HasOne(d => d.CandidateProfile)
                  .WithMany(p => p.WorkExperiences)
                  .HasForeignKey(d => d.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        builder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<News>(entity =>
        {
            entity.ToTable("News");
        });

        builder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
        });

        builder.Entity<AiChatMessage>(entity =>
        {
            entity.ToTable("AiChatMessages");
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.UserId);
        });

        builder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("ChatSessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.User1Id);
            entity.HasIndex(e => e.User2Id);
        });

        builder.Entity<AiChatSession>(entity =>
        {
            entity.ToTable("AiChatSessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
        });

        builder.Entity<SavedCandidate>(entity =>
        {
            entity.ToTable("SavedCandidates");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Recruiter)
                  .WithMany()
                  .HasForeignKey(e => e.RecruiterId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Candidate)
                  .WithMany()
                  .HasForeignKey(e => e.CandidateId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.RecruiterId, e.CandidateId }).IsUnique();
        });
    }

    public virtual DbSet<Application> Applications { get; set; }
    public virtual DbSet<CandidateProfile> CandidateProfiles { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Company> Companies { get; set; }
    public virtual DbSet<Education> Educations { get; set; }
    public virtual DbSet<Job> Jobs { get; set; }
    public virtual DbSet<Skill> Skills { get; set; }
    public virtual DbSet<WorkExperience> WorkExperiences { get; set; }
    public virtual DbSet<News> News { get; set; }
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
    public virtual DbSet<AiChatMessage> AiChatMessages { get; set; }
    public virtual DbSet<ChatSession> ChatSessions { get; set; }
    public virtual DbSet<AiChatSession> AiChatSessions { get; set; }
    public virtual DbSet<UserResume> UserResumes { get; set; }
    public virtual DbSet<SavedCandidate> SavedCandidates { get; set; }
}
