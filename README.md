```markdown
# RailwayOrientedCSharp

A lightweight, zero-dependency implementation of **Railway-Oriented Programming (ROP)** in C#. 

This repository demonstrates how to replace nested `if/else` blocks, hidden `throw` statements, and scattered `try/catch` handlers with clean, declarative pipelines using custom `Result` / `Result<T>` types and functional extension methods.

```text
Input ──► [ Validate ] ──► [ Get Data ] ──► [ Check Rules ] ──► [ Transform ] ──► Success<T>
               │                │                 │
               ▼                ▼                 ▼
         (Failure Track) ───────────────────────────────────────────────────────► Failure(Errors)

```

---

## Why Railway-Oriented Programming?

* **Honest Signatures:** Methods return `Result<T>` instead of lying with `T` and throwing hidden exceptions.
* **Accumulative Validation:** Collect all input errors at once using `Combine` instead of failing on the very first mistake.
* **Flat Pipelines:** Chain async steps linearly with automatic short-circuiting whenever a rule fails.
* **Isolated Side Effects:** Keep logging, auditing, and notifications separated from business math via `TapAsync`.
* **Safe State Transitions:** Combine immutable C# `record` types and `switch` expressions to prevent illegal state changes.

---

## Project Structure

```text
RailwayOrientedCSharp/
├── RailwayOrientedCSharp.csproj
├── Program.cs                        # Entry point running all 3 live scenarios
├── Core/
│   ├── Result.cs                     # Result & Result<T> types
│   └── ResultExtensions.cs           # Combine, Bind, BindIf, Ensure, Map, Tap
└── Examples/
    ├── CouponCheckoutExample.cs      # Zero-if coupon redemption pipeline
    ├── OrderProcessingExample.cs     # Conditional fraud screening with BindIfAsync
    └── OrderStateMachineExample.cs   # Immutable state machine returning Result<Order>

```

---

## Prerequisites & Running the Demo

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later

Clone the repository and run the console application:

```bash
git clone [https://github.com/](https://github.com/)<your-username>/RailwayOrientedCSharp.git
cd RailwayOrientedCSharp
dotnet run

```

### Included Demo Scenarios

1. **Coupon Redemption Pipeline:** Shows accumulative input validation (`Combine`), dynamic error guards (`EnsureAsync`), and isolated logging (`TapAsync`).
2. **Order Processing Workflow:** Demonstrates conditional step execution (`BindIfAsync`) that triggers fraud checks only for non-VIP orders over $1,000.
3. **Immutable Order State Machine:** Models `Pending → Approved → Paid` transitions using C# pattern matching and rejects illegal jumps cleanly.

---

## Core Combinators Reference

| Combinator | Signature Summary | Purpose |
| --- | --- | --- |
| **`Combine`** | `Combine(params Result[]) → Result` | Merges multiple validation checks so all errors are returned together. |
| **`Bind` / `BindAsync**` | `Result<TIn> → (TIn → Result<TOut>) → Result<TOut>` | Chains steps that can fail. Skips execution if already on the failure track. |
| **`BindIfAsync`** | `Task<Result<T>> → (predicate, step) → Task<Result<T>>` | Runs an async step **only** when the predicate is true; otherwise passes the current `Result<T>` untouched. |
| **`EnsureAsync`** | `Task<Result<T>> → (predicate, error) → Task<Result<T>>` | Evaluates a business rule. Switches to the failure track if the predicate returns `false`. |
| **`MapAsync`** | `Task<Result<TIn>> → (TIn → TOut) → Task<Result<TOut>>` | Transforms the inner value when on the success track. |
| **`TapAsync`** | `Task<Result<T>> → (T → void) → Task<Result<T>>` | Executes side effects (logging, metrics) without modifying the pipeline value. |

---

## Quick Example

```csharp
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

```

---

## License

MIT — feel free to copy, adapt, and use `Result` and `ResultExtensions` in your own .NET projects.


