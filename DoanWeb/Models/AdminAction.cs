using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class AdminAction
{
    public int ActionId { get; set; }

    public int AdminId { get; set; }

    public string? ActionType { get; set; }

    public int? TargetId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Admin { get; set; } = null!;
}
