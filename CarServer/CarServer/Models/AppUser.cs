using System;
using System.Collections.Generic;

namespace CarServer.Models;

public partial class AppUser
{
    public Guid Id { get; set; }

    public int IdRole { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public string? PasswordResetToken { get; set; }

    public virtual Role Role { get; set; } = null!;
}
