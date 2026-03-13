using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class CoverLetter
{
    public int LetterId { get; set; }

    public int CandidateId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;
}
