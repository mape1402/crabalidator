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
