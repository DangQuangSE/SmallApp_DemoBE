namespace SecondBike.Application.DTOs.Ratings;

public class CreateRatingDto
{
    public int OrderId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

public class RatingDto
{
    public int FeedbackId { get; set; }
    public int OrderId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public string FromUserName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

public class SellerStatsDto
{
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}
