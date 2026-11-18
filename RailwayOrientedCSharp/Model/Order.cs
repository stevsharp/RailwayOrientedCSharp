namespace RailwayOrientedCSharp.Model;

using global::RailwayOrientedCSharp.Core;

public record Order(string Id, OrderStatus Status, decimal Amount)
{
    public Result<Order> Fire(OrderTrigger trigger) => (Status, trigger) switch
    {
        (OrderStatus.Pending, OrderTrigger.Submit) => Result.Success(this with { Status = OrderStatus.Approved }),
        (OrderStatus.Pending, OrderTrigger.FlagFraud) => Result.Success(this with { Status = OrderStatus.FraudReview }),
        (OrderStatus.FraudReview, OrderTrigger.ClearFraud) => Result.Success(this with { Status = OrderStatus.Approved }),
        (OrderStatus.Approved, OrderTrigger.Pay) => Result.Success(this with { Status = OrderStatus.Paid }),
        (_, OrderTrigger.Reject) => Result.Success(this with { Status = OrderStatus.Rejected }),
        _ => Result.Failure<Order>($"Cannot fire '{trigger}' while order is '{Status}'.")
    };
}
