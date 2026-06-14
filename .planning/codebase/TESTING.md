# Testing Patterns

**Analysis Date:** 2026-06-13

## Test Framework

**Runner:**
- NUnit 4.5.1 (latest referenced in `tests/PoliNorError.Tests.csproj`)
- NUnit3TestAdapter 4.6.0 for Visual Studio / `dotnet test` integration
- Config: `tests/PoliNorError.Tests.csproj` (legacy .NET Framework project format, not SDK-style)

**Assertion Library:**
- NUnit built-in: `Assert.That(...)` constraint model (used in newer tests)
- `NUnit.Framework.Legacy.ClassicAssert` — legacy-style assertions used extensively in older tests
- Both styles coexist; new tests prefer `Assert.That()` with constraints

**Mocking Framework:**
- NSubstitute 5.3.0
- Castle.Core 5.2.1 (NSubstitute dependency)

**Target Framework:**
- .NET Framework 4.7.2 (`<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`)

**Run Commands:**
```bash
# Run all tests (from solution root)
dotnet test tests/PoliNorError.Tests.csproj

# Run via NUnit console (if installed)
nunit3-console tests/bin/Debug/PoliNorError.Tests.dll

# Run specific test by name
dotnet test tests/PoliNorError.Tests.csproj --filter "FullyQualifiedName~RetryPolicyTests.Should_Handle_WorkWith_No_CancelToken"
```

## Test File Organization

**Location:**
- All test files are in `tests/` directory — flat structure, no subdirectories (except `tests/RetryDelay.Tests/`)
- No co-location with source files

**Naming:**
- Test files match the class/feature being tested: `DefaultRetryProcessorTests.cs`, `RetryPolicyTests.cs`, `FallbackPolicyTests.cs`
- Suffix: `Tests` (not `Test`)
- Partial test classes split with dot notation: `PolicyProcessor.HandleException.Tests.cs`, `PolicyProcessor.HandleExceptionAsync.Tests.cs`, `PolicyCollectionWrapUpTests.T.cs`

**Structure:**
```
tests/
├── DefaultRetryProcessorTests.cs          # Tests for DefaultRetryProcessor
├── DefaultRetryProcessorAsyncTests.cs     # Async tests for DefaultRetryProcessor
├── RetryPolicyTests.cs                    # Tests for RetryPolicy
├── FallbackPolicyTests.cs                 # Tests for FallbackPolicy
├── SimplePolicyTests.cs                   # Tests for SimplePolicy
├── TryCatchTests.cs                       # Tests for TryCatch
├── RetryDelay.Tests/                      # Subdirectory for RetryDelay tests
│   ├── RetryDelayTests.cs
│   ├── RetryDelayChecker.cs
│   ├── RetryDelayJitteredTests.cs
│   └── RetryDelayRepeater.cs
├── ErrorWithInnerExcThrowingFuncs.cs      # Shared test helper
├── RetryObjectsToTest.cs                  # Shared test helper (fake objects)
├── PredicateFuncsForTests.cs              # Shared predicate functions
├── TaskWaitingDelegates.cs                # Cancellation test helpers
├── IErrorProcessorRegistration.cs         # Test interface + implementations
└── Properties/AssemblyInfo.cs
```

## Test Structure

**Class Organization:**
- Test classes are `internal` (not public)
- No base test class — each test class is standalone
- Test classes organized by the class/feature under test
- No `[SetUp]` / `[TearDown]` observed — state is created inline per test

**Suite Organization:**
```csharp
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PoliNorError.Tests
{
    internal class RetryPolicyTests
    {
        [Test]
        public void Should_Handle_WorkWith_No_CancelToken()
        {
            var retry = new RetryPolicy(1);
            void action() => Expression.Empty();

            var res = retry.Handle(action);
            ClassicAssert.IsFalse(res.IsFailed);
            ClassicAssert.IsFalse(res.IsCanceled);
            ClassicAssert.IsFalse(res.Errors.Any());
            ClassicAssert.IsTrue(res.NoError);
        }

        [Test]
        public async Task Should_HandleAsync_WorkWithClosure()
        {
            var retry = new RetryPolicy(1);
            var testClass = new TestAsyncClass();
            Func<CancellationToken, Task> taskSave = testClass.SaveAsync;

            await retry.HandleAsync(taskSave);
            ClassicAssert.AreEqual(1, testClass.I);
        }
    }
}
```

**Test Naming Convention:**
- Pattern: `Should_[ExpectedBehavior]_[When_Condition]` or `Should_[ExpectedBehavior]_If_[Condition]`
- Examples:
  - `Should_Retry_WhenZeroOrOneRetry`
  - `Should_Retry_Break_And_BeFailedCanceled_WhenDelegateWithErrorAndCanceled`
  - `Should_Generic_IncludeError_Work`
  - `Should_Handle_WorkWith_No_CancelToken`
  - `Should_CrossHandle_Sync_NoGeneric_ByFallbackAsync_If_Error`
  - `Should_UnprocessedError_Be_Null_Even_SetFailed_In_PolicyResultHandler`
  - `Should_Fallback_Result_BeCanceled_IfTokenJustCanceled`
  - `Should_Work_For_Handle_Null_Delegate`

**Parameterized Tests:**
```csharp
[Test]
[TestCase(0, 2)]
[TestCase(1, 2)]
public void Should_Retry_WhenZeroOrOneRetry(int retryCount, int resErrorCount)
{
    void save() => throw new Exception();
    var processor = new DefaultRetryProcessor();
    var tryResCount = processor.Retry(save, retryCount);
    ClassicAssert.AreEqual(resErrorCount, tryResCount.Errors.Count());
}
```

- `[TestCase]` used for parameterized tests with primitive values
- `[TestCase]` with enum values also supported:
```csharp
[Test]
[TestCase(TestErrorSetMatch.NoMatch, true)]
[TestCase(TestErrorSetMatch.FirstParam, false)]
public void Should_IncludeErrorSet_With_TwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
```

## Assertion Patterns

**Legacy ClassicAssert (most common):**
```csharp
ClassicAssert.AreEqual(expected, actual);
ClassicAssert.IsTrue(condition);
ClassicAssert.IsFalse(condition);
ClassicAssert.Null(value);
ClassicAssert.NotNull(value);
```

**Modern Assert.That (newer tests):**
```csharp
Assert.That(PolicyStatus.NotExecuted.Status.Status, Is.EqualTo(0));
Assert.That(tryCatchResult.Error, Is.Not.Null);
Assert.That(tryCatchResult.IsError, Is.True);
Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
```

## Mocking

**Framework:** NSubstitute 5.3.0

**Patterns:**
```csharp
using NSubstitute;

// Create substitute for interface
var subsProcessor = Substitute.For<IRetryProcessor>();
subsProcessor.Retry(Arg.Any<Action>(), rci).Returns(new PolicyResult());

// Verify call was made
subsProcessor.Received(1).Retry(Arg.Any<Action>(), rci);
```

**What to Mock:**
- Processor interfaces (`IRetryProcessor`, `IFallbackProcessor`, `IPolicyProcessor`)
- Error processors (`IErrorProcessor`)

**What NOT to Mock:**
- Policy classes themselves (create real instances)
- PolicyResult (create real instances and assert on them)
- Internal processor implementations (use real instances or `InternalsVisibleTo`)

**InternalsVisibleTo for testing:**
```xml
<!-- In src/PoliNorError.csproj -->
<AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
    <_Parameter1>PoliNorError.Tests</_Parameter1>
</AssemblyAttribute>
<AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
    <_Parameter1>DynamicProxyGenAssembly2</_Parameter1>
</AssemblyAttribute>
```

This allows:
1. Tests to access `internal` members directly
2. NSubstitute/Castle to proxy `internal` interfaces and classes

## Test Helpers and Utilities

**Error-throwing function libraries:**
- `tests/ErrorWithInnerExcThrowingFuncs.cs` — static methods that throw `TestExceptionWithInnerException` or plain `Exception`:
  - `ActionWithInner()`, `Action()`, `FunWithInnerWithMsg(string)`, `AsyncFuncWithInner(CancellationToken)`
  - Contains nested `TestExceptionWithInnerException` and `TestInnerException` classes
- `tests/ErrorWithInnerExcThrowingFuncs.TestExceptionWithInnerException` — custom exception with inner exception support

**Fake/mock objects:**
- `tests/RetryObjectsToTest.cs`:
  - `FakeDelayProvider` — `IDelayProvider` that records call count and optionally cancels
  - `FakeRetryDelay` — `RetryDelay` that returns `TimeSpan.Zero` and records attempts
  - `LinearRetryDelayThatStoreTime` — wraps `LinearRetryDelay` and stores computed delay
  - `DelayProviderThatAlreadyCanceled` — simulates cancellation during delay
  - `DelayProviderThatFailed` — throws `InvalidOperationException`
  - `CancelableActions` — static methods for creating cancellation scenarios

**Predicate helpers:**
- `tests/PredicateFuncsForTests.cs` — reusable predicates for PolicyResult:
  - `GenericPredicate(PolicyResult<int> pr)` — checks error messages
  - `Predicate(PolicyResult pr)` — checks error messages

**Cancellation helpers:**
- `tests/TaskWaitingDelegates.cs` — factory methods for delegates that trigger cancellation during `Task.Wait`/`Task.WaitAll`
- `tests/CancellationTests` (referenced via `using static`) — cancellation test infrastructure

**Test enums:**
- `tests/FallbackTypeForTests.cs` — enum for fallback policy type selection in parameterized tests
- `tests/TestHandlingForInnerError.cs` — enum/config for inner error test scenarios
- `tests/TestHandlingWithErrorSet.cs` — enum for error set match scenarios
- `tests/TestPolicyResultHandlerSyncType.cs` — enum for sync/async handler type

**Registration test interface:**
- `tests/IErrorProcessorRegistration.cs` — common interface `IErrorProcessorRegistration` with multiple implementations for testing error processor registration across different policy types:
  - `CatchBlockHandlerErrorProcessorRegistration`
  - `BulkErrorProcessorErrorProcessorRegistration`
  - `PolicyProcessorErrorProcessorRegistration`
  - `PolicyDelegateCollectionErrorProcessorRegistration`
  - `PolicyCollectionErrorProcessorRegistration`

**Task waiting delegates:**
- `tests/TaskWaitingDelegates.cs` — helpers for testing cancellation propagation through sync-over-async patterns

## Test Categories/Types

**Unit Tests:**
- All tests in the `tests/` directory are unit tests
- No integration tests detected
- No E2E tests detected
- Tests exercise policies, processors, and extensions in isolation

**Test coverage areas (from file names):**
- Processor tests: `DefaultRetryProcessorTests.cs`, `DefaultRetryProcessorAsyncTests.cs`, `DefaultFallbackProcessorTests.cs`, `DefaultFallbackProcessorAsyncTests.cs`, `SimplePolicyProcessorTests.cs`
- Policy tests: `RetryPolicyTests.cs`, `FallbackPolicyTests.cs`, `SimplePolicyTests.cs`, `TryCatchTests.cs`
- Extension tests: `DelegateExtensionsRetryTests.cs`, `DelegateExtensionsFallbackTests.cs`, `DelegateExtensionsSimpleTests.cs`
- Error handling: `ErrorProcessorTests.cs`, `ExceptionFilterTests.cs`, `CatchBlockHandlersTests.cs`, `ErrorSetTests.cs`
- Result handling: `PolicyResultTests.cs`, `PolicyResultExtensionsTests.cs`, `PolicyResultHandlerTests.cs`
- Cancellation: `CancellationTests.cs`, `PolicyProcessorHandleCanceledTests.cs`
- Collections: `PolicyCollectionTests.cs`, `PolicyDelegateCollectionTests.cs`
- Pipeline: `PipelineFuncBuilderTests.cs`, `PipelineFuncExtensionsTests.cs`
- Delay: `RetryDelay.Tests/RetryDelayTests.cs`, `RetryDelay.Tests/RetryDelayJitteredTests.cs`, `DelayProviderTests.cs`
- Typed error processors: `SyncErrorProcessorTests.cs`, `SyncTypedErrorProcessorTests.cs`, `TypedErrorProcessorRegistrationTests.cs`

## Code Coverage

**Current Coverage (from `CODE_COVERAGE.md`):**
- **Line coverage:** 87.6% (5625 of 6419 coverable lines)
- **Branch coverage:** 82.6% (907 of 1098 branches)
- **Parser:** Cobertura format
- **Generated:** 24.03.2024

**Coverage tooling:**
- ReportGenerator used to produce the coverage report (visible from HTML formatting in `CODE_COVERAGE.md`)
- No `.runsettings` or coverage configuration file detected in the repository
- No CI pipeline for automated coverage runs detected (`.github/dependabot.yml` only — no workflow files)

**Notable coverage gaps (from CODE_COVERAGE.md):**
- `PoliNorError.ApplyFuncs` — 25%
- `PoliNorError.CollectionExtensions` — 0%
- `PoliNorError.DefaultErrorsToStringAggregator` — 0%
- `PoliNorError.ExceptionDelegatesHelper` — 0%
- `PoliNorError.InconsistencyPolicyException` — 0%
- `PoliNorError.SynchronizedList<T>` — 26.7%
- `PoliNorError.TaskExtensions` — 43.3%
- `PoliNorError.PolicyDelegateCollectionBase<T>` — 43.7%
- `PoliNorError.IWithPolicyBaseExtensions` — 53.3%
- `PoliNorError.RetryPolicyCustomErrorSaverRegistration` — 8.8%
- `PoliNorError.RetryProcessorCustomErrorSaverRegistration` — 35.7%

## How to Run Tests

**Prerequisites:**
- .NET Framework 4.7.2 SDK/targeting pack installed
- NuGet packages restored (`nuget restore` or via Visual Studio)

**From Visual Studio:**
- Open `PoliNorError.sln`
- Build solution (Ctrl+Shift+B)
- Run all tests (Ctrl+R, A) or use Test Explorer

**From command line:**
```bash
# Restore packages
nuget restore PoliNorError.sln

# Build
msbuild PoliNorError.sln /p:Configuration=Debug

# Run tests
dotnet test tests/PoliNorError.Tests.csproj --configuration Debug

# Run specific test class
dotnet test tests/PoliNorError.Tests.csproj --filter "ClassName~RetryPolicyTests"

# Run specific test
dotnet test tests/PoliNorError.Tests.csproj --filter "FullyQualifiedName~Should_Retry_WhenZeroOrOneRetry"
```

**Notes:**
- Test project targets .NET Framework 4.7.2 (not .NET Core/5+), so `dotnet test` requires appropriate SDK
- Platform target is x64 for Debug configuration
- No CI/CD workflow detected — tests are run manually or in a local build pipeline

---

*Testing analysis: 2026-06-13*
