using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int ReporterId { get; set; }

    public int? JobId { get; set; }

    public int? TargetUserId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Job? Job { get; set; }

    public virtual User Reporter { get; set; } = null!;

    public virtual User? TargetUser { get; set; }
}
