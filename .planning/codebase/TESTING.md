# TESTING

## Framework and Setup
- Test framework: NUnit 4.x
- Test adapter: NUnit3TestAdapter
- Mock/stub tooling: NSubstitute (with Castle.Core)

## Coverage Shape
- Core policy families are heavily tested:
  - `Fallback*Tests`
  - `Retry*Tests`
  - `Simple*Tests`
- Cross-cutting areas covered:
  - error processors
  - exception filtering
  - delegate pipelines
  - wrappers and policy collections
  - policy result handlers and result states

## Strengths
- Wide breadth across public API and behavior variants
- Sync/async behavior is explicitly validated
- Cancellation paths are tested in many modules

## Potential Gaps
- Legacy test project format increases maintenance overhead
- Restore/build environment sensitivity can block quick local execution

## Suggested Next Testing Improvements
1. Add targeted regression tests for newly introduced APIs before merge.
2. Consider migrating tests to SDK-style csproj to simplify toolchain compatibility.
3. Add smoke tests for package build/pack flow in CI if not already present.
