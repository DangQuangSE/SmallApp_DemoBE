namespace SecondBike.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Suspended = 2,
    Banned = 3,
    Unverified = 4
}

public enum BikeCategory
{
    Mountain = 1,
    Road = 2,
    Hybrid = 3,
    Electric = 4,
    Other = 5
}

public enum BikeSize
{
    ExtraSmall = 1,
    Small = 2,
    Medium = 3,
    Large = 4,
    ExtraLarge = 5
}

public enum BikeCondition
{
    New = 1,
    LikeNew = 2,
    VeryGood = 3,
    Good = 4,
    Fair = 5
}

public enum PostStatus
{
    Draft = 1,
    Active = 2,
    Sold = 3,
    Hidden = 4,
    Deleted = 5
}

public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Completed = 6
}

public enum PaymentType
{
    Deposit = 1,
    FullPayment = 2
}

public enum PaymentMethod
{
    VnPay = 1,
    Paypal = 2,
    Wallet = 3
}

public enum PaymentGateway
{
    VnPay = 1,
    Paypal = 2
}

public enum InspectionStatus
{
    Pending = 1,
    Assigned = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}

public enum OverallCondition
{
    Poor = 1,
    Fair = 2,
    Good = 3,
    Excellent = 4
}
