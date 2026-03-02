using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Tracks a single payment transaction against an order.
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentGateway Gateway { get; set; }

    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public decimal? RefundAmount { get; set; }

    public virtual Order Order { get; set; } = null!;
}
