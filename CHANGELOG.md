# Changelog

## [v1.0.2] - 2026-08-06

### Added

- Built-in rule extension methods for strings, comparable values, collections, membership checks, defaults, and null/default checks in the base `Crabalidator` namespace.
- BenchmarkDotNet coverage for built-in rule extensions against FluentValidation equivalents.
- BenchmarkDotNet coverage for custom `Must(...)` predicates using public static, public instance, and fallback lambda delegates.
- Explicit nested validator helpers through `ValidateNestedWith<TValidator>(...)` and `ValidateEachWith<TValidator>(...)`.

### Optimized

- Optimized DynaBee generated validators to emit direct calls for visible `Must(Func<TProperty, bool>)` delegate methods.
- Added fast-path support for public static `Must` predicates and public instance predicates with captured targets.
- Updated built-in extension rules that require predicate state to use visible predicate methods so generated validators can avoid `Delegate.Invoke`.
- Improved mixed extension-rule validation performance against FluentValidation baselines.

### Fixed

- Preserved property types when explicit nested validator helpers receive object-typed property expressions.
- Allowed explicit nested validator selection without manually constructing validators, keeping dependency injection intact.
- Kept opaque lambdas and private predicate methods on the safe fallback path.

## [v1.0.1] - 2026-07-22

### Added

- Inferred nested object validation through `ValidateNested(...)`.
- Inferred collection element validation through `ValidateEach(...)`.
- Type-based nested validator overloads for explicit validator selection without manual construction.

### Fixed

- Resolved inferred nested validators from dependency injection so child validators can use constructor dependencies and registered lifetimes.
- Built DI validator plans with the active service provider for compiled validators and diagnostics.
- Avoided manual `new` validator construction in samples, README snippets, and benchmark nested validation setup.

## [v1.0.0] - 2026-07-21

### Added

- Initial Crabalidator solution structure based on DynaBee.
- Library, test, benchmark, sample, documentation, package, and CI scaffolding.
- Stable NuGet package metadata, package icon, SourceLink, deterministic builds, symbols, and release workflow support.
- Core validation model with validators, contexts, results, descriptors, diagnostics, and fluent rule configuration.
- Public validation APIs through `CrabValidator<T>`, `IValidator<T>`, `IAsyncValidator<T>`, `ICrabalidator`, and `ValidationResult`.
- Fluent `RuleFor(...)` rules for empty checks, length checks, comparisons, equality, and custom predicates.
- Property-level `When(...)` and `Unless(...)` conditions.
- Property cascade behavior through `Cascade(CascadeMode.Stop)`.
- Nested object validation through `SetValidator(...)`.
- Collection element validation through `RuleForEach(...)`.
- Async custom validation through `MustAsync(...)` with cancellation support.
- Async nested validator execution.
- Backend-neutral validation planning with rule order, failure metadata, diagnostics, and interpreted execution.
- DynaBee sync generation backend with generated validator types, generated method bodies, compiled invokers, and registry caching.
- DynaBee-generated hot paths for direct sync validation, typed property getters, typed rule checks, typed `Must(...)` delegates, conditions, nested validation, and collection validation.
- Dependency injection registration through `Microsoft.Extensions.DependencyInjection`.
- Validator discovery, compiled validator adapters, and the default Crabalidator runtime service.
- Runtime diagnostics for registered validators and readable validation plan output through `DescribePlan(...)`.
- Basic console sample covering sync rules, conditions, nested validation, collection validation, async validation, and diagnostics.
- BenchmarkDotNet scenarios for direct, interpreted, generated DI, nested, async, and delegate-heavy validation paths.
- FluentValidation benchmark baselines for sync, nested, async, and delegate-heavy validation scenarios.

### Optimized

- Reduced context allocations in direct, DI, nested, and generated validation hot paths.
- Inlined nested plan execution to avoid intermediate child `ValidationResult` allocations.
- Avoided duplicate failure collection copies when plans produce invalid results.
- Pre-sized validation failure collections from validation plan estimates.
- Optimized generated invalid paths for common rule kinds and delegate predicates.

### Notes

- Crabalidator targets `net8.0`, `net9.0`, and `net10.0`.
- DynaBee is an internal generation backend dependency and is not exposed through Crabalidator public APIs.
- Validators that contain async rules must be executed with `ValidateAsync(...)`.
- NativeAOT/mobile-friendly generation is not part of this release.
