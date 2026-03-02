using SecondBike.Domain.Common;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Internal wallet for deposits, payments, and refunds.
/// </summary>
public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsLocked { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
