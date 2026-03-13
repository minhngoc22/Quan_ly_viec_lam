using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class JobComment
{
    public int CommentId { get; set; }

    public int JobId { get; set; }

    public int UserId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
