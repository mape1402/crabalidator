# Crabalidator Architecture

Crabalidator is a .NET validation library intended to replace FluentValidation in application code while improving runtime performance, diagnostics, and extensibility.

The core architectural rule is:

> Configuration can inspect metadata, but validation execution must run through generated validators.

DynaBee is the initial runtime generation backend. Crabalidator owns validation semantics, configuration, planning, diagnostics, result models, dependency injection, and runtime caching. DynaBee owns runtime type generation, generated method bodies, generated instance creation, and method invocation.

## Goals

- Provide a familiar fluent validation experience for teams using FluentValidation.
- Generate high-performance validator implementations for hot validation paths.
- Avoid reflection, expression interpretation, and repeated rule discovery during validation execution.
- Support sync and async validation without forcing async overhead into sync-only validators.
- Support property rules, object rules, cross-property rules, nested validators, collections, conditions, cascade behavior, severity, error codes, localization hooks, and custom state.
- Support DI-first usage for validators, rule dependencies, custom validators, message formatters, and async external checks.
- Provide clear diagnostics for configured rules, generated plans, unsupported constructs, and runtime failures.
- Keep DynaBee behind a backend boundary so another generation strategy can be added later.

## Non-Goals

- Do not clone FluentValidation internals.
- Do not expose DynaBee types through Crabalidator public APIs.
- Do not use reflection as the runtime validation path.
- Do not require consumers to instantiate generated validator types manually.
- Do not make async validation the only execution model.
- Do not hide ambiguous rule behavior behind conventions when explicit configuration is safer.

## High-Level Flow

```text
User Configuration
    |
    v
Validator Registry
    |
    v
Rule Model
    |
    v
Validation Plan
    |
    v
Generation Backend
    |
    v
Generated Validator Type
    |
    v
Runtime Validator Cache / DI
    |
    v
ValidationResult
```

1. Users define validators through fluent classes, inline registration, attributes, or adapter APIs.
2. Crabalidator captures rules into a backend-neutral configuration model.
3. Crabalidator validates the model and creates a `ValidationPlan`.
4. The configured generation backend compiles the plan into an executable validator.
5. The compiled validator is cached by validated type and optional profile/variant.
6. Runtime calls execute generated code and return `ValidationResult`.

## Public API Layer

Primary public concepts:

- `ICrabalidator`
- `ICrabalidator<T>`
- `IValidator<T>`
- `IAsyncValidator<T>`
- `CrabValidator<T>`
- `ICrabRuleBuilder<T, TProperty>`
- `ValidationResult`
- `ValidationFailure`
- `ValidationContext<T>`
- `CrabalidatorOptions`
- `CrabalidatorProfile`

Example target usage:

```csharp
public sealed class CustomerValidator : CrabValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .When(x => x.RequiresAdultValidation);
    }
}
```

DI usage:

```csharp
services.AddCrabalidator(registration =>
{
    registration.AddValidators(typeof(CustomerValidator).Assembly);
});

var validator = provider.GetRequiredService<IValidator<Customer>>();
var result = validator.Validate(customer);
```

## Layer Responsibilities

### Configuration

Captures user intent. This layer should not generate code.

Responsibilities:

- Register validator types.
- Capture property rules and object-level rules.
- Capture conditions, cascade mode, messages, error codes, severity, custom state, and rule sets.
- Capture sync and async rule definitions separately.
- Capture nested validators and collection rules.
- Capture DI dependencies required by custom rules.
- Validate configuration consistency before compilation.

Important models:

- `ValidatorDescriptor`
- `RuleDescriptor`
- `PropertyRuleDescriptor`
- `ObjectRuleDescriptor`
- `ConditionDescriptor`
- `RuleSetDescriptor`
- `ValidatorKey`
- `RuleDependencyDescriptor`

### Planning

Turns configuration into an execution plan. Planning is backend-neutral.

Responsibilities:

- Normalize rule order.
- Resolve property access paths.
- Classify rules as sync-only, async-only, or dual-mode.
- Determine whether a generated validator needs `ValidationContext<T>`.
- Determine whether a generated validator needs DI dependencies.
- Determine cascade behavior and short-circuit points.
- Expand nested validators and collection validators.
- Produce diagnostics for unsupported or inefficient rules.

Important models:

- `ValidationPlan`
- `RulePlan`
- `PropertyAccessPlan`
- `FailurePlan`
- `ConditionPlan`
- `NestedValidationPlan`
- `CollectionValidationPlan`

### Generation

Compiles validation plans into executable validators.

Backend-neutral contracts:

- `IValidationGenerationBackend`
- `CompiledValidator`
- `ICompiledValidatorInvoker`
- `ICompiledAsyncValidatorInvoker`

DynaBee implementation:

- `DynabeeValidationGenerationBackend`
- `DynabeeCompiledValidatorInvoker`
- `DynabeeTypedValidatorInvoker<T>`
- `DynabeeTypedAsyncValidatorInvoker<T>`

The generation backend receives a `ValidationPlan` and returns a `CompiledValidator`. The generated type should implement internal runtime contracts, not public configuration types.

Generated sync method shape:

```csharp
ValidationResult Validate(T instance, ValidationContext<T> context);
```

Generated async method shape:

```csharp
ValueTask<ValidationResult> ValidateAsync(
    T instance,
    ValidationContext<T> context,
    CancellationToken cancellationToken);
```

The sync path must not allocate async state machines for sync-only validators.

### Runtime

Owns execution and caching.

Responsibilities:

- Resolve compiled validators.
- Cache by `ValidatorKey`.
- Provide typed fast paths through generic static caches.
- Create validation contexts.
- Invoke sync or async compiled validators.
- Compose nested and collection results.
- Protect generated execution with clear exception wrapping where useful.

Important components:

- `CompiledValidatorRegistry`
- `TypedCompiledValidatorCache<T>`
- `Crabalidator`
- `CrabValidatorAdapter<T>`
- `ValidationContextFactory`

### Dependency Injection

Registers Crabalidator services and discovers validators.

Expected APIs:

- `services.AddCrabalidator(...)`
- `registration.AddValidator<TValidator>()`
- `registration.AddValidators(Assembly assembly)`
- `registration.AddProfile<TProfile>()`
- `registration.Options`

Default service registrations:

- configuration registry
- plan builder
- DynaBee generation backend
- compiled validator registry
- public `ICrabalidator`
- typed `IValidator<T>` adapters

### Diagnostics

Diagnostics should be first-class.

Capabilities:

- Describe registered validators.
- Describe generated validation plans.
- Report unsupported rules before runtime.
- Warn when a rule forces async-only execution.
- Warn when a rule cannot be compiled and would require fallback behavior.
- Track generated validator type names in internal diagnostics.

Public-facing diagnostics should not require DynaBee knowledge.

## DynaBee Boundary

DynaBee must stay inside `Generation.Dynabee`.

Allowed DynaBee usage:

- create generated validator classes
- implement generated methods
- inject dependencies into generated validators
- create typed delegates or invocation adapters

Disallowed outside the backend:

- public API references to DynaBee
- configuration model references to DynaBee
- planning model references to DynaBee
- runtime cache keys based on DynaBee types

This mirrors OctoMap's backend pattern while adapting it to validation.

## Performance Principles

- Compile once, execute many times.
- Cache compiled validators by stable keys.
- Prefer typed delegates over object argument arrays for common paths.
- Keep the sync path sync.
- Avoid per-rule allocations in successful validation where possible.
- Lazily allocate failure lists only after the first failure.
- Avoid expression compilation on hot paths.
- Avoid repeated property metadata lookup during validation.
- Use generated direct property access when possible.

## Initial Project Shape

```text
src/
  Crabalidator/
    Abstractions/
    Configuration/
    Planning/
    Generation/
      Dynabee/
    Runtime/
    DependencyInjection/
    Diagnostics/
tests/
  Crabalidator.Tests/
benchmarks/
  Crabalidator.Benchmarks/
samples/
  Crabalidator.Samples.Basic/
docs/
  ARCHITECTURE.md
  ROADMAP.md
```

## Compatibility Strategy

Crabalidator should feel familiar to FluentValidation users, but it should not be constrained by FluentValidation internals.

Recommended compatibility layers:

- Similar rule names for common validators.
- Migration-oriented extension methods where names match safely.
- Optional adapter package later for bridging existing FluentValidation validators.
- Clear documentation for differences in cascade behavior, async execution, and DI resolution.
