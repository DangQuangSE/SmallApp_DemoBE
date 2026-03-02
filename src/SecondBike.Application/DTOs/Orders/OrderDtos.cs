using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Orders;

public class CreateOrderDto
{
    public Guid BikePostId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal DepositPercentage { get; set; } = 15;
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal BikePrice { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public string BikeTitle { get; set; } = string.Empty;
    public string? BikeImageUrl { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
}

public class ProcessPaymentDto
{
    public Guid OrderId { get; set; }
    public PaymentType Type { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentGateway Gateway { get; set; }
}
