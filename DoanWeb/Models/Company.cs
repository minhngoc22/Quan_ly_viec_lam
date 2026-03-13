using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public int EmployerId { get; set; }

    public int CategoryId { get; set; }

    public int ProvinceId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Description { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public string? Logo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual JobCategory Category { get; set; } = null!;

    public virtual Employer Employer { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual Province Province { get; set; } = null!;
}
