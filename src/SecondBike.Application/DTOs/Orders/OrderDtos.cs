namespace SecondBike.Application.DTOs.Orders;

public class CreateOrderDto
{
    public int ListingId { get; set; }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public byte? OrderStatus { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? RemainingAmount { get; set; }
    public byte? DepositStatus { get; set; }
    public DateTime? OrderDate { get; set; }

    public string BikeTitle { get; set; } = string.Empty;
    public string? BikeImageUrl { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;

    public List<PaymentDto> Payments { get; set; } = new();
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionRef { get; set; }
    public DateTime? PaymentDate { get; set; }
}

/// <summary>
/// Request to create a VNPay payment URL for deposit or full payment.
/// </summary>
public class CreatePaymentUrlDto
{
    public int OrderId { get; set; }

    /// <summary>
    /// "deposit" = pay 20% deposit, "full" = pay remaining amount.
    /// </summary>
    public string PaymentType { get; set; } = "deposit";
}

/// <summary>
/// Response containing the VNPay redirect URL.
/// </summary>
public class PaymentUrlResultDto
{
    public string PaymentUrl { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
}

/// <summary>
/// VNPay IPN/Return callback query parameters mapped to a DTO.
/// </summary>
public class VnPayCallbackDto
{
    public string vnp_TxnRef { get; set; } = string.Empty;
    public string vnp_ResponseCode { get; set; } = string.Empty;
    public string vnp_TransactionNo { get; set; } = string.Empty;
    public long vnp_Amount { get; set; }
    public string vnp_BankCode { get; set; } = string.Empty;
    public string vnp_OrderInfo { get; set; } = string.Empty;
    public string vnp_PayDate { get; set; } = string.Empty;
    public string vnp_SecureHash { get; set; } = string.Empty;
}
