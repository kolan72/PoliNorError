# INTEGRATIONS

## External Integrations
- NuGet ecosystem for package restore and distribution
- GitHub repository links embedded in package metadata

## Dependency Surface
- Runtime: minimal explicit external runtime dependencies in the main library
- Test-only dependencies:
  - `nunit.framework`
  - `NSubstitute`
  - `Castle.Core`
  - `System.*` compatibility/support packages for test target

## CI/CD and Distribution Signals
- Package metadata includes icon/readme/repository URL
- `GeneratePackageOnBuild=true` indicates packaging is part of normal build workflow

## Interop Boundaries
- Public API exposes delegates/actions/funcs and cancellation token patterns
- Strong interoperability with consumer code via extension methods and policy wrappers

## Risks / Friction Points
- Old-style test project may require extra maintenance for package restore and tooling compatibility
- Restore can be sensitive to machine-level TLS/certificate configuration (observed in this workspace)
