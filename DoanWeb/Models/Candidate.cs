using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Candidate
{
    public int CandidateId { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual User CandidateNavigation { get; set; } = null!;

    public virtual ICollection<Cv> Cvs { get; set; } = new List<Cv>();

    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}
