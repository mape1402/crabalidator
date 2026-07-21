# Changelog

All notable changes to Crabalidator will be documented in this file.

## [Unreleased]

- Established initial repository structure, architecture notes, and roadmap.
- Added the Phase 1 core validation model with validators, contexts, results, descriptors, diagnostics, and basic fluent rules.
- Added backend-neutral validation planning with rule order, failure metadata, diagnostics, and interpreted plan execution.
- Added the first DynaBee sync generation backend with compiled validator invokers and registry caching.
- Added a basic console sample project.
- Added dependency injection registration, validator discovery, compiled validator adapters, and the default Crabalidator service.
- Added fluent rule metadata, conditions, cascade behavior, and custom predicates.
- Added nested object and collection validation with prefixed child failure paths.
- Added async custom validation with cancellation support and async nested validator execution.
- Added readable validation plan diagnostics through `DescribePlan()`.
- Added DI-backed runtime diagnostics for registered validators and model plans.
- Added initial BenchmarkDotNet scenarios for direct, interpreted, generated DI, nested, and async validation paths.
- Reduced context allocations in direct, DI, and nested validation hot paths.
- Added FluentValidation benchmark baselines for sync, nested, and async validation.
- Inlined nested plan execution to avoid intermediate child `ValidationResult` allocations.
- Avoided duplicate failure collection copies when plans produce invalid results.
