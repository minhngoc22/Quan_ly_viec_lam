using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DoanWeb.Models;

public partial class JobContext : DbContext
{
    public JobContext()
    {
    }

    public JobContext(DbContextOptions<JobContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminAction> AdminActions { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<CareerGuide> CareerGuides { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Cv> Cvs { get; set; }

    public virtual DbSet<CvCertificate> CvCertificates { get; set; }

    public virtual DbSet<CvEducation> CvEducations { get; set; }

    public virtual DbSet<CvExperience> CvExperiences { get; set; }

    public virtual DbSet<CvSkill> CvSkills { get; set; }

    public virtual DbSet<Employer> Employers { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobCategory> JobCategories { get; set; }

    public virtual DbSet<JobComment> JobComments { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SavedJob> SavedJobs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=JOB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminAction>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("PK__AdminAct__FFE3F4B941B5845B");

            entity.Property(e => e.ActionId).HasColumnName("ActionID");
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TargetId).HasColumnName("TargetID");

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminActions)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AdminActi__Admin__6477ECF3");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Applicat__C93A4F791147430F");

            entity.HasIndex(e => new { e.JobId, e.CandidateId }, "UQ_Applications").IsUnique();

            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CandidateId).HasColumnName("CandidateID");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Candidate).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Candi__5BE2A6F2");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobID__5AEE82B9");
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.CandidateId).HasName("PK__Candidat__DF539BFC0049BD2A");

            entity.Property(e => e.CandidateId)
                .ValueGeneratedNever()
                .HasColumnName("CandidateID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.CandidateNavigation).WithOne(p => p.Candidate)
                .HasForeignKey<Candidate>(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Candidate__Candi__3D5E1FD2");
        });

        modelBuilder.Entity<CareerGuide>(entity =>
        {
            entity.HasKey(e => e.GuideId).HasName("PK__CareerGu__E77EE03EA1E093A8");

            entity.ToTable("CareerGuide");

            entity.Property(e => e.GuideId).HasColumnName("GuideID");
            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PostedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Views).HasDefaultValue(0);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Companie__2D971C4CF15C7434");

            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployerId).HasColumnName("EmployerID");
            entity.Property(e => e.Logo).HasMaxLength(255);
            entity.Property(e => e.Website).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Companies)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Companies__Categ__49C3F6B7");

            entity.HasOne(d => d.Employer).WithMany(p => p.Companies)
                .HasForeignKey(d => d.EmployerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Companies__Emplo__48CFD27E");

            entity.HasOne(d => d.Province).WithMany(p => p.Companies)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Companies__Provi__4AB81AF0");
        });

        modelBuilder.Entity<Cv>(entity =>
        {
            entity.HasKey(e => e.Cvid).HasName("PK__CVs__A04CFC43E52BBE89");

            entity.ToTable("CVs");

            entity.Property(e => e.Cvid).HasColumnName("CVID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CandidateId).HasColumnName("CandidateID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Cvs)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CVs__CandidateID__7E37BEF6");
        });

        modelBuilder.Entity<CvCertificate>(entity =>
        {
            entity.HasKey(e => e.CertId).HasName("PK__CvCertif__E5BD38E562580493");

            entity.Property(e => e.CertId).HasColumnName("CertID");
            entity.Property(e => e.CertificateName).HasMaxLength(150);
            entity.Property(e => e.Cvid).HasColumnName("CVID");
            entity.Property(e => e.Organization).HasMaxLength(150);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvCertificates)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CvCertific__CVID__09A971A2");
        });

        modelBuilder.Entity<CvEducation>(entity =>
        {
            entity.HasKey(e => e.EduId).HasName("PK__CvEducat__1FD9496E8E4EFD66");

            entity.Property(e => e.EduId).HasColumnName("EduID");
            entity.Property(e => e.Cvid).HasColumnName("CVID");
            entity.Property(e => e.Major).HasMaxLength(100);
            entity.Property(e => e.SchoolName).HasMaxLength(150);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvEducations)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CvEducatio__CVID__06CD04F7");
        });

        modelBuilder.Entity<CvExperience>(entity =>
        {
            entity.HasKey(e => e.ExpId).HasName("PK__CvExperi__45B117C71387741F");

            entity.Property(e => e.ExpId).HasColumnName("ExpID");
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Cvid).HasColumnName("CVID");
            entity.Property(e => e.Position).HasMaxLength(150);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvExperiences)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CvExperien__CVID__03F0984C");
        });

        modelBuilder.Entity<CvSkill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__CvSkills__DFA091E73C1B0C0D");

            entity.Property(e => e.SkillId).HasColumnName("SkillID");
            entity.Property(e => e.Cvid).HasColumnName("CVID");
            entity.Property(e => e.SkillLevel).HasMaxLength(50);
            entity.Property(e => e.SkillName).HasMaxLength(100);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvSkills)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CvSkills__CVID__01142BA1");
        });

        modelBuilder.Entity<Employer>(entity =>
        {
            entity.HasKey(e => e.EmployerId).HasName("PK__Employer__CA445241FFB1A315");

            entity.Property(e => e.EmployerId)
                .ValueGeneratedNever()
                .HasColumnName("EmployerID");
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(d => d.EmployerNavigation).WithOne(p => p.Employer)
                .HasForeignKey<Employer>(d => d.EmployerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employers__Emplo__403A8C7D");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Jobs__056690E2BB1903C2");

            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmployerId).HasColumnName("EmployerID");
            entity.Property(e => e.ExpireDate).HasColumnType("datetime");
            entity.Property(e => e.GenderRequirement)
                .HasMaxLength(20)
                .HasDefaultValue("Không yêu cầu");
            entity.Property(e => e.ImageUrl).HasMaxLength(300);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.JobType).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Priority).HasDefaultValue(0);
            entity.Property(e => e.Salary).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.VacancyCount).HasDefaultValue(1);

            entity.HasOne(d => d.Category).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Jobs__CategoryID__5441852A");

            entity.HasOne(d => d.Company).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Jobs__CompanyID__534D60F1");

            entity.HasOne(d => d.Employer).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.EmployerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Jobs__EmployerID__52593CB8");

            entity.HasOne(d => d.Province).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK__Jobs__ProvinceId__5535A963");
        });

        modelBuilder.Entity<JobCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__JobCateg__19093A2B1ACE1CCB");

            entity.HasIndex(e => e.CategoryName, "UQ__JobCateg__8517B2E05E312591").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<JobComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__JobComme__C3B4DFAAE8EE7CD0");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Job).WithMany(p => p.JobComments)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobCommen__JobID__0D7A0286");

            entity.HasOne(d => d.User).WithMany(p => p.JobComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobCommen__UserI__0E6E26BF");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07F009FC1E");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Link).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__74AE54BC");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Password__658FEE8AD4835A9D");

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .HasColumnName("OTP");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PasswordR__UserI__693CA210");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.ProvinceId).HasName("PK__Province__FD0A6F834C3D0F43");

            entity.Property(e => e.ProvinceName).HasMaxLength(100);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD48E5780C04FF");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.ReporterId).HasColumnName("ReporterID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Chưa xử lý");
            entity.Property(e => e.TargetUserId).HasColumnName("TargetUserID");

            entity.HasOne(d => d.Job).WithMany(p => p.Reports)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__Reports__JobID__6EF57B66");

            entity.HasOne(d => d.Reporter).WithMany(p => p.ReportReporters)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reports__Reporte__6E01572D");

            entity.HasOne(d => d.TargetUser).WithMany(p => p.ReportTargetUsers)
                .HasForeignKey(d => d.TargetUserId)
                .HasConstraintName("FK__Reports__TargetU__6FE99F9F");
        });

        modelBuilder.Entity<SavedJob>(entity =>
        {
            entity.HasKey(e => e.SavedId).HasName("PK__SavedJob__0B058FFC7122E5B0");

            entity.Property(e => e.SavedId).HasColumnName("SavedID");
            entity.Property(e => e.CandidateId).HasColumnName("CandidateID");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Candidate).WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedJobs__Candi__5FB337D6");

            entity.HasOne(d => d.Job).WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedJobs__JobID__60A75C0F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC0A81933B");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341E031E72").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.AvatarColor).HasMaxLength(7);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
