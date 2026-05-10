# STACK

## Runtime and Language
- Language: C#
- Primary runtime target: .NET Standard 2.0 (`src/PoliNorError.csproj`)
- Test runtime target: .NET Framework 4.7.2 (`tests/PoliNorError.Tests.csproj`)

## Build and Packaging
- SDK-style library project for main package (`Microsoft.NET.Sdk`)
- NuGet package metadata and package-on-build enabled in source project
- Legacy non-SDK test project with explicit package references in `packages.config` and `<Reference>` entries

## Core Libraries and Tools
- Testing: NUnit 4.x, NUnit3TestAdapter
- Mocking/Substitution: NSubstitute, Castle.Core
- Internal visibility for tests and DynamicProxy generated assemblies

## Architectural Style
- Policy-based error handling framework
- Major policy families: `Simple`, `Retry`, `Fallback`
- Wrapper/pipeline support for policy composition and delegate collections

## Current State Notes
- Mixed modern/legacy project model (SDK-style + old-style csproj)
- Large test suite (70+ test files)
- Rich extension-driven API surface for ergonomics
