namespace SecondBike.Application.DTOs.Ratings;

public class CreateRatingDto
{
    public Guid OrderId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public int? CommunicationRating { get; set; }
    public int? AccuracyRating { get; set; }
    public int? PackagingRating { get; set; }
    public int? SpeedRating { get; set; }
}

public class RatingDto
{
    public Guid Id { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public string FromUserName { get; set; } = string.Empty;
    public string? FromUserAvatar { get; set; }
    public int? CommunicationRating { get; set; }
    public int? AccuracyRating { get; set; }
    public int? PackagingRating { get; set; }
    public int? SpeedRating { get; set; }
    public string? SellerResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}
