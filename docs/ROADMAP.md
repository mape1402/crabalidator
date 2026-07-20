# Crabalidator Roadmap

This roadmap prioritizes a working generated validation core before broad FluentValidation parity.

## Phase 0: Foundation

Goal: establish repository structure, contracts, and CI-ready build.

- Create solution and project layout.
- Multi-target `net8.0`, `net9.0`, and `net10.0`.
- Add package metadata consistent with the Elysium libraries.
- Add references to DynaBee and DI abstractions.
- Add xUnit test project.
- Add benchmark project.
- Add basic README with positioning and quick start.

Exit criteria:

- `dotnet build` succeeds.
- `dotnet test` succeeds with project structure tests.
- Public package metadata is present.

## Phase 1: Core Validation Model

Goal: capture validation intent without generation.

- Define `ValidationResult` and `ValidationFailure`.
- Define `IValidator<T>` and `IAsyncValidator<T>`.
- Define `ValidationContext<T>`.
- Implement `CrabValidator<T>` base class.
- Implement `RuleFor(...)` using expression-based configuration capture.
- Implement basic rule descriptors.
- Add property name/path extraction.
- Add configuration diagnostics.

Initial rules:

- `NotNull`
- `NotEmpty`
- `Equal`
- `NotEqual`
- `GreaterThan`
- `GreaterThanOrEqualTo`
- `LessThan`
- `LessThanOrEqualTo`
- `Length`
- `MinimumLength`
- `MaximumLength`

Exit criteria:

- Rules can be configured fluently.
- Configuration can be described for diagnostics.
- No runtime code generation is required yet.

## Phase 2: Planning

Goal: convert rule descriptors into backend-neutral validation plans.

- Implement `ValidationPlan`.
- Implement `ValidationPlanBuilder`.
- Normalize rule order and cascade behavior.
- Classify validators by sync/async requirements.
- Create failure plans with message, code, severity, attempted value, and property path.
- Add plan diagnostics.

Exit criteria:

- A configured validator produces a deterministic plan.
- Plan tests cover property paths, rule ordering, messages, and cascade behavior.

## Phase 3: DynaBee Sync Backend

Goal: execute sync validators through generated code.

- Define `IValidationGenerationBackend`.
- Define `CompiledValidator`.
- Define typed invoker contracts.
- Implement `DynabeeValidationGenerationBackend`.
- Generate validator classes with a sync `Validate` method.
- Generate direct property access for simple property rules.
- Generate failure creation only on rule failure.
- Add typed delegate fast path.
- Add compiled validator registry and typed cache.

Exit criteria:

- Basic validators execute through DynaBee-generated code.
- Tests prove generated validators return correct failures.
- Benchmarks compare against a simple reflection/interpreted baseline.

## Phase 4: DI Integration

Goal: make Crabalidator usable in real apps.

- Implement `services.AddCrabalidator(...)`.
- Add validator assembly scanning.
- Register typed `IValidator<T>` adapters.
- Support singleton/scoped/transient validator registration choices.
- Support generated validator dependencies.
- Add samples.

Exit criteria:

- Users can register validators from an assembly.
- Users can resolve `IValidator<T>` from DI.
- Generated validators can call DI-backed custom rules.

## Phase 5: Fluent Rule Surface

Goal: cover the high-value FluentValidation replacement surface.

- Add message customization with placeholders.
- Add error codes.
- Add severity.
- Add custom state.
- Add `When` and `Unless`.
- Add cascade modes.
- Add `Must`.
- Add object-level rules.
- Add enum validators.
- Add regex and email validators.
- Add credit card and URL validators only if they can be well specified.

Exit criteria:

- Common business validators can migrate without custom infrastructure.
- Fluent API tests cover chaining behavior and diagnostics.

## Phase 6: Nested Objects and Collections

Goal: validate real object graphs efficiently.

- Add `SetValidator`.
- Add child validator discovery and injection.
- Add `RuleForEach`.
- Add collection index path formatting.
- Add null child behavior options.
- Add cascade behavior across nested validation.

Exit criteria:

- Nested and collection validation works through generated code.
- Result paths are stable and migration-friendly.

## Phase 7: Async Validation

Goal: support external checks without penalizing sync validators.

- Add async rule descriptors.
- Add `MustAsync`.
- Add async custom validators.
- Generate async validator methods.
- Return `ValueTask<ValidationResult>` where possible.
- Support cancellation tokens.
- Add diagnostics for sync calls against async-only validators.

Exit criteria:

- Sync-only validators stay sync-only.
- Async validators execute correctly with cancellation.
- Mixed sync/async validators preserve rule order and cascade behavior.

## Phase 8: Advanced Diagnostics and Developer Experience

Goal: make generated validation understandable.

- Add `ICrabalidatorConfiguration`.
- Add `DescribeConfiguration()`.
- Add `DescribePlan<T>()`.
- Add unsupported-rule diagnostics.
- Add analyzers for common mistakes.
- Add source package symbols and XML docs.

Exit criteria:

- Users can inspect what Crabalidator registered and generated.
- Common configuration mistakes are caught before production.

## Phase 9: Performance Hardening

Goal: prove the performance story.

- Add BenchmarkDotNet suite.
- Compare against FluentValidation for common scenarios.
- Measure no-failure allocations.
- Measure many-failure allocations.
- Measure nested object validation.
- Measure collection validation.
- Measure async validation overhead.
- Tune generated code and result allocation strategy.

Exit criteria:

- Benchmarks show clear wins on hot sync paths.
- Any regressions are tracked with named benchmark scenarios.

## Phase 10: Migration Layer

Goal: make adoption easier for FluentValidation users.

- Document API differences.
- Add migration guide.
- Add compatible naming aliases where appropriate.
- Consider optional package for adapting existing FluentValidation validators.
- Provide examples for ASP.NET Core integration.

Exit criteria:

- A team can migrate common validators incrementally.
- Unsupported FluentValidation behaviors are clearly documented.

## Suggested MVP Scope

The first useful MVP should include:

- `CrabValidator<T>`
- `RuleFor`
- sync generated validation through DynaBee
- basic comparison/null/string validators
- `When`
- cascade modes
- custom messages
- DI registration
- typed `IValidator<T>`
- diagnostics for registered validators and plans
- initial benchmarks

Everything else should build after the generated sync pipeline is proven.

## Architecture Risks

- Expression capture can become too tied to runtime expression interpretation. Mitigation: expressions should be used for configuration discovery only; execution plans must contain resolved access paths.
- Async support can pollute sync performance. Mitigation: separate sync and async generated paths.
- FluentValidation parity can bloat the core. Mitigation: prioritize migration-critical behaviors and document differences.
- DynaBee capabilities may need small enhancements. Mitigation: track requirements in docs, as OctoMap did, and keep backend boundaries clean.
- Failure allocation can dominate performance in invalid inputs. Mitigation: optimize the valid path first, then design pooled or builder-based failure collection only if benchmarks justify it.
