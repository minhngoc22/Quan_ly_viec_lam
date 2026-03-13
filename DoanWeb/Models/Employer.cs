using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class Employer
{
    public int EmployerId { get; set; }

    public string? CompanyName { get; set; }

    public string? Position { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual User EmployerNavigation { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
