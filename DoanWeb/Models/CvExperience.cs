using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class CvExperience
{
    public int ExpId { get; set; }

    public int Cvid { get; set; }

    public string Position { get; set; } = null!;

    public string? CompanyName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public virtual Cv Cv { get; set; } = null!;
}
