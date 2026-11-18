namespace RailwayOrientedCSharp.Model;
using System.Threading.Tasks;

public class MockFraudDetectionService : IFraudDetectionService
{
    public Task<FraudAssessment> EvaluateRiskAsync(string customerId, decimal amount) =>
        Task.FromResult(amount > 5000m
            ? new FraudAssessment(true, 91, "Velocity threshold exceeded.")
            : new FraudAssessment(false, 12, "Normal activity."));
}
