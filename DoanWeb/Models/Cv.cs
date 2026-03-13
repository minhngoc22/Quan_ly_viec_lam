using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Cv
{
    public int Cvid { get; set; }

    public int CandidateId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? CareerGoal { get; set; }

    public string? Avatar { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual ICollection<CvCertificate> CvCertificates { get; set; } = new List<CvCertificate>();

    public virtual ICollection<CvEducation> CvEducations { get; set; } = new List<CvEducation>();

    public virtual ICollection<CvExperience> CvExperiences { get; set; } = new List<CvExperience>();

    public virtual ICollection<CvSkill> CvSkills { get; set; } = new List<CvSkill>();
}
