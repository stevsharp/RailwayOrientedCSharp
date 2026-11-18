namespace RailwayOrientedCSharp.Model;

using global::RailwayOrientedCSharp.Core;

using System;
using System.Threading.Tasks;

public class OrderProcessingWorkflow
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IFraudDetectionService _fraudService;
    private readonly IBillingGateway _billingGateway;
    private readonly ILogger _logger;

    public OrderProcessingWorkflow(
        ICustomerRepository customerRepo,
        IFraudDetectionService fraudService,
        IBillingGateway billingGateway,
        ILogger logger)
    {
        _customerRepo = customerRepo;
        _fraudService = fraudService;
        _billingGateway = billingGateway;
        _logger = logger;
    }

    public async Task<Result<string>> ProcessOrderAsync(string customerId, decimal amount)
    {
        return await ValidateCustomerExists(customerId)
            .TapAsync(c => _logger.Log($"Customer {c.Id} ({c.Tier}) loaded."))
            .EnsureAsync(c => !c.IsBanned, "Customer account is suspended.")
            .BindIfAsync(
                condition: c => amount > 1000m && !c.IsVip,
                conditionalStep: c => RunFraudCheckAsync(c, amount))
            .BindAsync(c => Task.FromResult(ValidateCreditLimit(c, amount)))
            .BindAsync(c => ChargeCustomerAsync(c, amount))
            .MapAsync(orderId => $"Order executed successfully. Tracking ID: {orderId}");
    }

    private async Task<Result<Customer>> ValidateCustomerExists(string customerId)
    {
        var customer = await _customerRepo.GetByIdAsync(customerId);
        return customer is not null
            ? Result.Success(customer)
            : Result.Failure<Customer>($"Customer '{customerId}' not found.");
    }

    private async Task<Result<Customer>> RunFraudCheckAsync(Customer customer, decimal amount)
    {
        _logger.Log($"Running fraud check for {customer.Id} (${amount})...");
        var assessment = await _fraudService.EvaluateRiskAsync(customer.Id, amount);
        return assessment.IsHighRisk
            ? Result.Failure<Customer>($"Blocked by fraud check: {assessment.Reason}")
            : Result.Success(customer);
    }

    private Result<Customer> ValidateCreditLimit(Customer customer, decimal amount) =>
        customer.CreditLimit >= amount
            ? Result.Success(customer)
            : Result.Failure<Customer>($"Credit limit exceeded (Available: ${customer.CreditLimit}).");

    private async Task<Result<string>> ChargeCustomerAsync(Customer customer, decimal amount)
    {
        var charged = await _billingGateway.ChargeAsync(customer.Id, amount);
        return charged
            ? Result.Success(Guid.NewGuid().ToString("N")[..8].ToUpper())
            : Result.Failure<string>("Payment gateway declined.");
    }
}
