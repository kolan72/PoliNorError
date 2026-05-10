# STRUCTURE

## Top-Level Layout
- `src/` main library code
- `tests/` NUnit test suite
- `packages/` local NuGet package cache/refs for legacy test project
- `.github/` repository automation/workflows

## Source Layout (`src/`)
- `Fallback/` fallback policies and providers
- `Retry/` retry policies and delay strategies
- `Simple/` simple policy and processor
- `Collections/` policy collection orchestration
- `Wrap/` policy wrapping/composition
- `ErrorProcessors/` error processor abstractions and concrete implementations
- `ExceptionFilter/` filtering configuration
- `Extensions/` fluent extension API surfaces
- `TryCatch/`, `PipelineFunc/`, `HandlerRunners/`, `Utilities/` support infrastructure

## Test Layout (`tests/`)
- Broad coverage by feature area: fallback, retry, simple, wrappers, filters, result handlers
- Dedicated subfolder for retry delay tests (`RetryDelay.Tests/`)
- Legacy csproj includes explicit compile includes for each test file

## Approximate Scale
- Source namespaces/files are sizable (hundreds of C# files)
- Tests include ~78 directly listed `.cs` files in root plus nested test files
