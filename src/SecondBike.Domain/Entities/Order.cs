using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int BuyerId { get; set; }

    public decimal? TotalAmount { get; set; }

    public byte? OrderStatus { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Payout> Payouts { get; set; } = new List<Payout>();

    public virtual ICollection<ServiceFee> ServiceFees { get; set; } = new List<ServiceFee>();
}
