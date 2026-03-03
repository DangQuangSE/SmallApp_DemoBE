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
    public DateTime? OrderDate { get; set; }

    public string BikeTitle { get; set; } = string.Empty;
    public string? BikeImageUrl { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
}

public class ProcessPaymentDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
}
