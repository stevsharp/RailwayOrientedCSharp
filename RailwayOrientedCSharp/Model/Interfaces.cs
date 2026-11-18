namespace RailwayOrientedCSharp.Model;

using System.Linq;
using System.Threading.Tasks;

// =================================================================
// INTERFACES 
// =================================================================
public interface ICouponRepository { Task<Coupon?> GetByCodeAsync(string code); }
public interface ICustomerRepository { Task<Customer?> GetByIdAsync(string customerId); }
public interface IFraudDetectionService { Task<FraudAssessment> EvaluateRiskAsync(string customerId, decimal amount); }
public interface IBillingGateway { Task<bool> ChargeAsync(string customerId, decimal amount); }
public interface ILogger { void Log(string message); }
