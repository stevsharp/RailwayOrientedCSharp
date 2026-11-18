namespace RailwayOrientedCSharp.Model;

using System.Collections.Generic;
using System.Threading.Tasks;

public class MockCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<string, Customer> _customers = new()
    {
        ["CUST-001"] = new Customer("CUST-001", "Alice", 10000m, "Standard", IsBanned: false),
        ["CUST-VIP"] = new Customer("CUST-VIP", "Evelyn", 25000m, "VIP", IsBanned: false),
        ["CUST-BAN"] = new Customer("CUST-BAN", "Mallory", 5000m, "Standard", IsBanned: true)
    };

    public Task<Customer?> GetByIdAsync(string customerId) =>
        Task.FromResult(_customers.TryGetValue(customerId, out var c) ? c : null);
}
