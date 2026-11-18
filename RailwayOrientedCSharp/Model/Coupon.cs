namespace RailwayOrientedCSharp.Model;

public record Coupon(string Code, decimal DiscountAmount, bool IsExpired, int MinOrderAmount);
