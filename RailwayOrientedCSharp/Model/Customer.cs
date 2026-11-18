namespace RailwayOrientedCSharp.Model;

using System;

// =================================================================
// EXAMPLE 2: ORDER WORKFLOW WITH FRAUD SCREENING
// =================================================================
public record Customer(string Id, string Name, decimal CreditLimit, string Tier = "Standard", bool IsBanned = false)
{
    public bool IsVip => Tier.Equals("VIP", StringComparison.OrdinalIgnoreCase);
}
