using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Purchase order with deposit / full-payment workflow.
/// </summary>
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid BikePostId { get; set; }

    // Pricing
    public decimal BikePrice { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal DepositPercentage { get; set; } = 15;
    public decimal RemainingAmount { get; set; }
    public decimal? ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }

    // Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime? DepositPaidAt { get; set; }
    public DateTime? FullPaymentAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Shipping
    public string ShippingAddress { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }

    // Dispute
    public bool HasDispute { get; set; }
    public string? DisputeReason { get; set; }
    public Guid? DisputeResolvedBy { get; set; }
    public DateTime? DisputeResolvedAt { get; set; }
    public string? DisputeResolution { get; set; }

    // Navigation
    public virtual AppUser Buyer { get; set; } = null!;
    public virtual AppUser Seller { get; set; } = null!;
    public virtual BikePost BikePost { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual Rating? Rating { get; set; }
}
