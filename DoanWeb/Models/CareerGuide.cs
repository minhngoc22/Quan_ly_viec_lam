using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class CareerGuide
{
    public int GuideId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Content { get; set; } = null!;

    public string? Image { get; set; }

    public string? Author { get; set; }

    public DateTime? PostedDate { get; set; }

    public string? Category { get; set; }

    public int? Views { get; set; }

    public bool? IsActive { get; set; }

    public int? DisplayOrder { get; set; }
}
