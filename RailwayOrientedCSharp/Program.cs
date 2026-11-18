using RailwayOrientedCSharp.Core;
using RailwayOrientedCSharp.Model;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine("  Railway-Oriented Programming (ROP) in C# - Live Demo");
        Console.WriteLine("=================================================================\n");

        await RunCouponDemoAsync();
        await RunOrderWorkflowDemoAsync();
        RunStateMachineDemo();

        Console.ReadLine();
    }

    // =============================================================
    // DEMO 1: Zero-If Coupon Redemption Pipeline
    // =============================================================
    private static async Task RunCouponDemoAsync()
    {
        Console.WriteLine("--- DEMO 1: Coupon Redemption Pipeline ---");
        var service = new CheckoutService(new MockCouponRepository(), new ConsoleLogger());

        // Case A: Multiple validation errors accumulated at once
        var invalidInputs = await service.RedeemCouponAsync("", -10m);
        PrintResult("Invalid Inputs (\"\", -$10)", invalidInputs);

        // Case B: Expired coupon
        var expired = await service.RedeemCouponAsync("OLD20", 100m);
        PrintResult("Expired Coupon (OLD20)", expired);

        // Case C: Below minimum order amount
        var tooSmall = await service.RedeemCouponAsync("SAVE25", 50m);
        PrintResult("Below Minimum Order ($50 for $100 min)", tooSmall);

        // Case D: Valid redemption
        var success = await service.RedeemCouponAsync("SAVE25", 120m);
        PrintResult("Valid Coupon (SAVE25 on $120)", success);
        Console.WriteLine();
    }

    // =============================================================
    // DEMO 2: Conditional Branching (EnsureAsync + BindIfAsync)
    // =============================================================
    private static async Task RunOrderWorkflowDemoAsync()
    {
        Console.WriteLine("--- DEMO 2: Order Processing (EnsureAsync & BindIfAsync) ---");
        var workflow = new OrderProcessingWorkflow(
            new MockCustomerRepository(),
            new MockFraudDetectionService(),
            new MockBillingGateway(),
            new ConsoleLogger());

        // Case A: Standard customer over $1,000 -> Triggers fraud check & passes
        var stdLarge = await workflow.ProcessOrderAsync("CUST-001", 1500m);
        PrintResult("Standard Customer ($1,500 - Fraud Check Runs)", stdLarge);

        // Case B: VIP customer over $1,000 -> Skips fraud check via BindIfAsync
        var vipLarge = await workflow.ProcessOrderAsync("CUST-VIP", 1500m);
        PrintResult("VIP Customer ($1,500 - Fraud Check Skipped)", vipLarge);

        // Case C: Banned customer -> Stops immediately at EnsureAsync
        var banned = await workflow.ProcessOrderAsync("CUST-BAN", 100m);
        PrintResult("Suspended Customer ($100)", banned);
        Console.WriteLine();
    }

    // =============================================================
    // DEMO 3: Immutable State Machine with ROP
    // =============================================================
    private static void RunStateMachineDemo()
    {
        Console.WriteLine("--- DEMO 3: Immutable Order State Machine ---");
        var order = new Order("ORD-900", OrderStatus.Pending, 250m);

        // Valid transition: Pending -> Approved -> Paid
        var validFlow = order.Fire(OrderTrigger.Submit)
            .Bind(approved => approved.Fire(OrderTrigger.Pay));
        PrintResult("Pending -> Submit -> Pay", validFlow);

        // Illegal transition: Pending -> Pay (skips approval)
        var illegalFlow = order.Fire(OrderTrigger.Pay);
        PrintResult("Pending -> Pay (Illegal Transition)", illegalFlow);
        Console.WriteLine();
    }

    private static void PrintResult<T>(string label, Result<T> result)
    {
        Console.Write($"  [{label}] => ");
        if (result.IsSuccess)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"SUCCESS: {result.Value}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FAILURE: {string.Join(" | ", result.Errors)}");
        }
        Console.ResetColor();
    }
}