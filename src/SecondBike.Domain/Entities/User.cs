using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AuditLogSystem> AuditLogSystems { get; set; } = new List<AuditLogSystem>();

    public virtual ICollection<BicycleListing> BicycleListings { get; set; } = new List<BicycleListing>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatSession> ChatSessionBuyers { get; set; } = new List<ChatSession>();

    public virtual ICollection<ChatSession> ChatSessionSellers { get; set; } = new List<ChatSession>();

    public virtual ICollection<Feedback> FeedbackTargetUsers { get; set; } = new List<Feedback>();

    public virtual ICollection<Feedback> FeedbackUsers { get; set; } = new List<Feedback>();

    public virtual ICollection<InspectionRequest> InspectionRequests { get; set; } = new List<InspectionRequest>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<ReportAbuse> ReportAbuses { get; set; } = new List<ReportAbuse>();

    public virtual ICollection<RequestAbuse> RequestAbuseReporters { get; set; } = new List<RequestAbuse>();

    public virtual ICollection<RequestAbuse> RequestAbuseTargetUsers { get; set; } = new List<RequestAbuse>();

    public virtual UserRole Role { get; set; } = null!;

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
