using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class PasswordResetToken
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string Otp { get; set; } = null!;

    public DateTime ExpiryTime { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
