# Technology Stack

**Analysis Date:** 2026-06-13

## Languages

**Primary:**
- C# — All source code in `src/` and tests in `tests/`

**Secondary:**
- None. Pure C# library with no code generation, scripting, or multi-language components.

## Runtime

**Target Framework:**
- .NET Standard 2.0 (`netstandard2.0`) — defined in `src/PoliNorError.csproj` line 4
- .NET Standard 2.0 provides broad compatibility: .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+, Mono, Xamarin, Unity

**Test Runtime:**
- .NET Framework 4.7.2 (`v4.7.2`) — defined in `tests/PoliNorError.Tests.csproj` line 18
- Test project uses legacy-style `.csproj` (not SDK-style), targeting full .NET Framework

**Package Manager:**
- NuGet (primary)
- Library source: SDK-style project with no `PackageReference` entries — zero runtime NuGet dependencies
- Test project: uses `packages.config` format (legacy NuGet) with packages restored to `packages/` directory

**Lockfile:**
- Not applicable. No lockfile present; NuGet resolves from `packages.config` for tests.

## Frameworks

**Core:**
- None. The library is a standalone .NET Standard 2.0 assembly with zero external dependencies at runtime.

**Testing:**
- NUnit 4.5.1 — Test framework (`tests/packages.config`)
- NUnit3TestAdapter 4.6.0 — VS Test adapter (`tests/packages.config`)
- NSubstitute 5.3.0 — Mocking framework (`tests/packages.config`)
- Castle.Core 5.2.1 — Dynamic proxy for NSubstitute mocking of internal types (`tests/packages.config`)

**Build/Dev:**
- MSBuild (via Visual Studio 2019+, solution format v16)
- No build scripts, Cake, NUKE, or custom build tools detected

## Key Dependencies

**Runtime Dependencies:**
- **None.** The library has zero `PackageReference` entries in `src/PoliNorError.csproj`. It is a self-contained .NET Standard 2.0 assembly with no external runtime dependencies.

**Test Dependencies (via `tests/packages.config`):**
- `NUnit` 4.5.1 — Test assertion and execution framework
- `NUnit3TestAdapter` 4.6.0 — Visual Studio test discovery
- `NSubstitute` 5.3.0 — Interface/class mocking
- `Castle.Core` 5.2.1 — Dynamic proxy generation (NSubstitute dependency for mocking internals)
- `System.Buffers` 4.6.1 — Polyfill for `ArrayPool<T>`
- `System.Memory` 4.6.3 — Polyfill for `Span<T>`, `Memory<T>`
- `System.Numerics.Vectors` 4.6.1 — Hardware-accelerated vector types
- `System.Runtime.CompilerServices.Unsafe` 6.1.2 — Low-level unsafe utilities
- `System.Threading.Tasks.Extensions` 4.6.3 — `ValueTask<T>` support
- `System.ValueTuple` 4.6.2 — C# 7 tuple support for .NET Framework 4.7.2

**Infrastructure Dependencies:**
- Castle.Core 5.2.1 — Required for Moq/NSubstitute to generate proxies for `internal` classes (enabled by `InternalsVisibleTo("DynamicProxyGenAssembly2")` in `src/PoliNorError.csproj` line 49)

## Configuration

**Environment:**
- No `.env` files. No environment variable configuration detected.
- No `global.json` present — uses whatever .NET SDK is installed.
- No `nuget.config` present — uses default NuGet feeds.

**Build Configuration:**
- `src/PoliNorError.csproj` — SDK-style project (`Microsoft.NET.Sdk`)
- `tests/PoliNorError.Tests.csproj` — Legacy-style project (ToolsVersion 15.0, MSBuild 2003 namespace)
- `PoliNorError.sln` — Visual Studio 2019 solution file (format v12.00, VS version 16)

**Compiler Settings (src):**
- `TargetFramework`: `netstandard2.0`
- `NoWarn`: `1701;1702;1591` (suppresses assembly reference and missing XML doc warnings, both Debug and Release)
- `GenerateDocumentationFile`: `true` (XML documentation generated for NuGet package)
- `GeneratePackageOnBuild`: `true` (NuGet package created on every build)
- `Deterministic`: not explicitly set in src (defaults to `false` for SDK-style projects unless specified)
- `InternalsVisibleTo`: `PoliNorError.Tests` and `DynamicProxyGenAssembly2` (for NSubstitute/Moq)

**Compiler Settings (tests):**
- `TargetFrameworkVersion`: `v4.7.2`
- `PlatformTarget`: `x64` (Debug only)
- `Deterministic`: `true`
- `DefineConstants`: `DEBUG;TRACE` (Debug), `TRACE` (Release)
- `DebugType`: `full` (Debug), `pdbonly` (Release)

## Build System

**Build Tool:**
- MSBuild (Visual Studio 2019+)
- Solution: `PoliNorError.sln` with two projects
- No `Directory.Build.props`, `Directory.Build.targets`, or custom `.props`/`.targets` files

**CI/CD:**
- No GitHub Actions workflows detected (`.github/workflows/` directory does not exist)
- Dependabot configured for NuGet dependency updates (`/.github/dependabot.yml`)

**Code Analyzers:**
- No Roslyn analyzers, StyleCop, SonarAnalyzer, or other static analysis tools detected in either project file
- No `.editorconfig` present

## NuGet Package Metadata

**Package Properties (from `src/PoliNorError.csproj`):**
- `Version`: 2.24.20
- `AssemblyVersion`: 2.24.20.0
- `Authors`: Andrey Kolesnichenko
- `License`: MIT (`PackageLicenseExpression`)
- `PackageIcon`: `PoliNorError.png`
- `PackageReadmeFile`: `docs/NuGet.md`
- `RepositoryUrl`: https://github.com/kolan72/PoliNorError
- `PackageTags`: Exception Handling Policy Retry Fallback Resilience Simple Error TryCatch CatchBlockHandler Try Catch

## Platform Requirements

**Development:**
- .NET SDK (any version supporting `netstandard2.0` target, typically .NET 6+)
- Visual Studio 2019+ or equivalent IDE for opening the solution
- .NET Framework 4.7.2 Developer Pack (for running tests)

**Production:**
- Any runtime implementing .NET Standard 2.0 (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+, Mono, Xamarin, Unity)

---

*Stack analysis: 2026-06-13*
