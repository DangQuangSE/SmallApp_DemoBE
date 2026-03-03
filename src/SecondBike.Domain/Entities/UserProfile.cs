using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class UserProfile
{
    public int ProfileId { get; set; }

    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual User User { get; set; } = null!;
}
