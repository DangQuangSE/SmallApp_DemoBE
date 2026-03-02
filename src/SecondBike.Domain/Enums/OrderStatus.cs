namespace SecondBike.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    DepositPaid = 2,
    FullyPaid = 3,
    Shipping = 4,
    Delivered = 5,
    Completed = 6,
    Cancelled = 7,
    Disputed = 8,
    Refunded = 9
}
