# Crabalidator

[![Build](https://github.com/mape1402/crabalidator/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/crabalidator/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/Crabalidator.svg)](https://www.nuget.org/packages/Crabalidator)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Crabalidator** is a high-performance .NET validation library built on top of **DynaBee** runtime code generation.

Crabalidator is designed for applications that want FluentValidation-style validators, but prefer compiled runtime validation plans over reflection-heavy execution paths. Validators describe rules through a familiar fluent API, Crabalidator turns those rules into backend-neutral validation plans, and DynaBee generates executable validation methods for hot sync paths.

## What Crabalidator Does

- Configures validators with `CrabValidator<T>` and `RuleFor(...)`.
- Supports common sync rules such as `NotEmpty`, string rules, collection count rules, comparisons, equality, membership, and custom `Must(...)` predicates.
- Supports property-level `When(...)` and `Unless(...)` conditions.
- Supports `Cascade(CascadeMode.Stop)` for fail-fast property validation.
- Supports inferred nested object validation through `ValidateNested(...)`.
- Supports inferred collection element validation through `ValidateEach(...)`.
- Supports async custom rules through `MustAsync(...)`.
- Supports async nested validators and cancellation.
- Integrates with `Microsoft.Extensions.DependencyInjection`.
- Provides `ICrabalidator`, typed `IValidator<T>`, and typed `IAsyncValidator<T>` runtime APIs.
- Provides registered validator diagnostics and readable validation plan output through `DescribePlan(...)`.
- Uses DynaBee-generated method bodies and invokers for optimized sync validation paths.
- Optimizes custom `Must(...)` predicates by emitting direct DynaBee calls for visible delegate methods.
- Includes BenchmarkDotNet coverage against FluentValidation baselines.

## Design Goals

Crabalidator is intentionally split from the generation engine.

Crabalidator owns:

- validation configuration
- validation planning
- rule semantics
- diagnostics
- dependency injection
- runtime validator registration
- public validation APIs

DynaBee owns:

- generated type creation
- generated method body creation
- generated method invocation
- low-level runtime code generation details

This boundary keeps Crabalidator focused on validation behavior while allowing DynaBee to evolve as a general-purpose runtime generation engine.

## Requirements

- .NET SDK 10.0+ recommended for development.
- The library multi-targets `net8.0`, `net9.0`, and `net10.0`.

## Installation

Install Crabalidator from NuGet:

```bash
dotnet add package Crabalidator --version 1.0.2
```

For local development, reference the project directly or use the solution in this repository.

## Quick Start

Define a validator:

```csharp
using Crabalidator;

public sealed class CustomerValidator : CrabValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(value => value.Contains('@'));
    }
}

public sealed class Customer
{
    public string Name { get; set; }

    public int Age { get; set; }

    public string Email { get; set; }
}
```

## Built-In Rules

Crabalidator exposes common validators as extension methods in the base `Crabalidator` namespace:

```csharp
RuleFor(x => x.Name)
    .NotEmpty()
    .MinimumLength(3)
    .MaximumLength(80)
    .StartsWith("CR")
    .Contains("AB")
    .Matches("^CRAB");

RuleFor(x => x.Email)
    .NotNull()
    .EmailAddress();

RuleFor(x => x.Age)
    .NotEmpty()
    .InclusiveBetween(18, 99);

RuleFor(x => x.Items)
    .NotEmpty()
    .MinimumCount(1)
    .MaximumCount(10);

RuleFor(x => x.Status)
    .In("ACTIVE", "PENDING")
    .NotIn("BLOCKED");
```

## Custom Rules

Use `Must(...)` for project-specific rules:

```csharp
RuleFor(x => x.Code)
    .Must(OrderRules.HasValidCode);

public static class OrderRules
{
    public static bool HasValidCode(string value)
        => value != null && value.StartsWith("CR", StringComparison.Ordinal);
}
```

When the predicate method is visible to generated code, Crabalidator emits a direct DynaBee call instead of going through `Delegate.Invoke`. Opaque lambdas and private methods still work and automatically fall back to delegate invocation.

Validate directly:

```csharp
var validator = new CustomerValidator();
var result = validator.Validate(new Customer());

if (!result.IsValid)
{
    foreach (var failure in result.Errors)
    {
        Console.WriteLine($"{failure.PropertyName}: {failure.ErrorMessage}");
    }
}
```

## Dependency Injection

Register Crabalidator with the assemblies that contain validators:

```csharp
using Crabalidator;
using Crabalidator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddCrabalidator(typeof(CustomerValidator).Assembly);

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<IValidator<Customer>>();

var result = validator.Validate(customer);
```

Applications that prefer a single service entry point can use `ICrabalidator`:

```csharp
var crabalidator = provider.GetRequiredService<ICrabalidator>();

ValidationResult result = crabalidator.Validate(customer);
```

## Conditions And Cascade

Conditions apply to the property rule chain:

```csharp
RuleFor(x => x.ReferralCode)
    .NotEmpty()
    .When(x => x.RequiresReferral);
```

Use cascade stop to avoid running later validators for the same property after the first failure:

```csharp
RuleFor(x => x.Name)
    .Cascade(CascadeMode.Stop)
    .NotEmpty()
    .MinimumLength(3);
```

## Nested Validators

Validate child objects:

```csharp
ValidateNested(x => x.Address);
```

Validate collection elements:

```csharp
RuleFor(x => x.Items)
    .NotEmpty();

ValidateEach(x => x.Items);
```

Nested failures are returned with prefixed paths such as `Address.PostalCode` or `Items[0].Sku`.

When more than one validator exists for a nested model, choose the validator explicitly:

```csharp
ValidateNestedWith<ShippingAddressValidator>(x => x.Address);
ValidateEachWith<StrictOrderItemValidator>(x => x.Items);
```

## Async Validation

Use `MustAsync(...)` for async checks:

```csharp
RuleFor(x => x.Username)
    .NotEmpty()
    .MustAsync(IsUsernameAvailableAsync);

static async ValueTask<bool> IsUsernameAvailableAsync(
    string username,
    CancellationToken cancellationToken)
{
    await Task.Delay(10, cancellationToken);
    return username != "taken";
}
```

Validators that contain async rules must be executed with `ValidateAsync(...)`.

## Testing

Install the testing helpers from NuGet:

```bash
dotnet add package Crabalidator.Testing
```

Register validators in a lightweight test service collection:

```csharp
using Crabalidator.Testing;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddCrabalidatorTesting(typeof(CustomerValidator).Assembly);

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<IAsyncValidator<Customer>>();

var result = await validator.ValidateAsync(customer);
```

External test hosts can use the adapter-friendly registration:

```csharp
services.AddCrabalidatorTestingAdapter(typeof(CustomerValidator).Assembly);
```

Run a specific validator directly without building a service provider:

```csharp
var result = await CrabalidatorTest
    .For<CustomerValidator>()
    .ValidateAsync(customer);
```

Assert validation results:

```csharp
result.ShouldBeValid();

result.ShouldHaveErrorFor<Customer>(x => x.Email);
result.ShouldHaveErrorMessage("Email is required.");
result.ShouldHaveErrorCode("customer.email.required");
```

Property assertions can be scoped to a single property and chained:

```csharp
result
    .ShouldHaveErrorFor<Customer>(x => x.Email)
    .WithErrorCount(2)
    .WithMessage("Email is required.")
    .WithErrorCode("customer.email.required")
    .WithSeverity(ValidationSeverity.Error);
```

Results returned by `CrabalidatorTest.For<TValidator>()` are typed, so direct validator tests can use the shorter property assertion:

```csharp
var result = await CrabalidatorTest
    .For<CustomerValidator>()
    .ValidateAsync(customer);

result.ShouldHaveErrorFor(x => x.Email);
```

## Diagnostics

Crabalidator can describe registered validators and generated validation plans:

```csharp
using Crabalidator.Diagnostics;

var diagnostics = provider.GetRequiredService<ICrabalidatorDiagnostics>();

Console.WriteLine(diagnostics.DescribeRegisteredValidators());
Console.WriteLine(diagnostics.DescribePlan<Customer>());
```

## Samples And Benchmarks

Run the basic sample:

```bash
dotnet run --project samples/Crabalidator.Samples.Basic
```

Run the test suite:

```bash
dotnet test
```

Run benchmarks:

```bash
dotnet run --project benchmarks/Crabalidator.Benchmarks -c Release
```

## Benchmark Snapshot

These numbers come from BenchmarkDotNet on Windows 11, .NET 8.0, Release mode, using the short benchmark job in this repository. Treat them as a local comparison point rather than a universal guarantee.

### Built-In Extension Rules

| Scenario | Crabalidator | FluentValidation | Speedup | Crabalidator Alloc | FluentValidation Alloc |
| --- | ---: | ---: | ---: | ---: | ---: |
| Valid model | 265.4 ns | 643.4 ns | 2.4x | 48 B | 672 B |
| Invalid model | 471.0 ns | 9,427.9 ns | 20.0x | 1,160 B | 20,584 B |

### Custom `Must(...)` Predicate

| Scenario | Crabalidator | FluentValidation | Speedup | Crabalidator Alloc | FluentValidation Alloc |
| --- | ---: | ---: | ---: | ---: | ---: |
| Public static predicate, valid model | 9.3 ns | 134.4 ns | 14.4x | 0 B | 600 B |
| Public static predicate, invalid model | 22.9 ns | 679.7 ns | 29.7x | 152 B | 1,856 B |

