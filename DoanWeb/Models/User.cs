using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? Status { get; set; }

    public string? Avatar { get; set; }

    public string? AvatarColor { get; set; }

    public virtual ICollection<AdminAction> AdminActions { get; set; } = new List<AdminAction>();

    public virtual Candidate? Candidate { get; set; }

    public virtual Employer? Employer { get; set; }

    public virtual ICollection<JobComment> JobComments { get; set; } = new List<JobComment>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<Report> ReportReporters { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportTargetUsers { get; set; } = new List<Report>();
}
