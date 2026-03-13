using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class CvEducation
{
    public int EduId { get; set; }

    public int Cvid { get; set; }

    public string SchoolName { get; set; } = null!;

    public string? Major { get; set; }

    public int? StartYear { get; set; }

    public int? EndYear { get; set; }

    public string? Description { get; set; }

    public virtual Cv Cv { get; set; } = null!;
}
