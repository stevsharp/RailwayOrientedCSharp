namespace RailwayOrientedCSharp.Model;
using System.Threading.Tasks;

public class MockBillingGateway : IBillingGateway
{
    public Task<bool> ChargeAsync(string customerId, decimal amount) => Task.FromResult(true);
}
