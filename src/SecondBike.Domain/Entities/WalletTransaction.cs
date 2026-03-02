using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Individual wallet transaction record.
/// </summary>
public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public WalletTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? RelatedOrderId { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
