namespace RailwayOrientedCSharp.Model;

using global::RailwayOrientedCSharp.Core;

using System;
using System.Threading.Tasks;

public class CheckoutService
{
    private readonly ICouponRepository _repo;
    private readonly ILogger _logger;

    public CheckoutService(ICouponRepository repo, ILogger logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public Task<Result<decimal>> RedeemCouponAsync(string couponCode, decimal orderTotal) =>
        ValidateInputs(couponCode, orderTotal)
            .BindAsync(() => GetCouponAsync(couponCode))
            .EnsureAsync(coupon => !coupon.IsExpired, "Coupon has expired.")
            .EnsureAsync(
                coupon => orderTotal >= coupon.MinOrderAmount,
                coupon => $"Minimum order of ${coupon.MinOrderAmount} required.")
            .TapAsync(coupon => _logger.Log($"Applying coupon {coupon.Code}..."))
            .MapAsync(coupon => Math.Max(0, orderTotal - coupon.DiscountAmount))
            .TapAsync(finalTotal => _logger.Log($"Coupon applied. New total: ${finalTotal}"));

    private Result ValidateInputs(string couponCode, decimal orderTotal)
    {
        var codeResult = string.IsNullOrWhiteSpace(couponCode)
            ? Result.Failure("Coupon code is required.")
            : Result.Success();

        var totalResult = orderTotal <= 0
            ? Result.Failure("Order total must be positive.")
            : Result.Success();

        return ResultExtensions.Combine(codeResult, totalResult);
    }

    private async Task<Result<Coupon>> GetCouponAsync(string couponCode)
    {
        var coupon = await _repo.GetByCodeAsync(couponCode);
        return coupon is not null
            ? Result.Success(coupon)
            : Result.Failure<Coupon>("Coupon not found.");
    }
}
