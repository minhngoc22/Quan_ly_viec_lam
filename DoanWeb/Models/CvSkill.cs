using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class CvSkill
{
    public int SkillId { get; set; }

    public int Cvid { get; set; }

    public string SkillName { get; set; } = null!;

    public string? SkillLevel { get; set; }

    public virtual Cv Cv { get; set; } = null!;
}
