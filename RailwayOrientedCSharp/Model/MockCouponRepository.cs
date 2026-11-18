namespace RailwayOrientedCSharp.Model;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MockCouponRepository : ICouponRepository
{
    private readonly Dictionary<string, Coupon> _coupons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SAVE25"] = new Coupon("SAVE25", 25m, IsExpired: false, MinOrderAmount: 100),
        ["OLD20"] = new Coupon("OLD20", 20m, IsExpired: true, MinOrderAmount: 50)
    };

    public Task<Coupon?> GetByCodeAsync(string code) =>
        Task.FromResult(_coupons.TryGetValue(code, out var coupon) ? coupon : null);
}
