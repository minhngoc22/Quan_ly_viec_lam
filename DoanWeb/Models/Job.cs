using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Job
{
    public int JobId { get; set; }

    public int EmployerId { get; set; }

    public int CompanyId { get; set; }

    public int? CategoryId { get; set; }

    public int? ProvinceId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public string? Benefits { get; set; }

    public string? Location { get; set; }

    public string Salary { get; set; } = null!;

    public string? JobType { get; set; }

    public string? GenderRequirement { get; set; }

    public int? VacancyCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpireDate { get; set; }

    public bool? IsApproved { get; set; }

    public string? Keywords { get; set; }

    public string? ImageUrl { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? Priority { get; set; }

    public bool IsExpired { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual JobCategory? Category { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Employer Employer { get; set; } = null!;

    public virtual ICollection<JobComment> JobComments { get; set; } = new List<JobComment>();

    public virtual Province? Province { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}
