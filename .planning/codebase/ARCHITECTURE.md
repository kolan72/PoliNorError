# ARCHITECTURE

## Overview
PoliNorError is a policy-oriented exception-handling framework. The core model centers around:
- Policy abstractions (`Policy`, `IPolicyProcessor`, etc.)
- Concrete policies (`SimplePolicy`, `RetryPolicy`, `FallbackPolicy`)
- Processor implementations that execute delegates and apply policy behavior
- Optional wrappers and collections for composing multiple policies

## Main Subsystems
- `src/Simple`: simple execution policy
- `src/Retry`: retry policy and retry-delay model
- `src/Fallback`: fallback policy and fallback function provider model
- `src/Wrap`: policy wrapping/composition primitives
- `src/Collections`: policy delegate collections and aggregation results
- `src/ErrorProcessors`: pluggable error processors and typed error processing
- `src/ExceptionFilter`: include/exclude error filtering mechanics

## Control Flow (Typical)
1. User configures a policy (or policy collection).
2. A delegate is handled by the policy.
3. Policy processor catches and classifies exceptions.
4. Policy behavior applies (retry/fallback/simple).
5. Policy result handlers run.
6. `PolicyResult`/`PolicyResult<T>` is returned.

## Key Design Traits
- Generic and non-generic delegate support
- Sync and async parity across APIs
- Explicit cancellation conversion behaviors
- High configurability via extension methods
