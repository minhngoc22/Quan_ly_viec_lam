using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int JobId { get; set; }

    public int CandidateId { get; set; }

    public DateTime? AppliedAt { get; set; }

    public string? Status { get; set; }

    public string? RejectReason { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;
}
