# PolicyComposition

This folder implements **policy composition**: grouping many policies into a single
`PolicyCollection` that applies them to **one common delegate**.

`PolicyCollection` is not a policy itself (`IPolicyBase`). It is an ordered
`IEnumerable<IPolicyBase>` with a fluent builder surface. Handling a delegate goes
through every policy in the collection, one by one, the same way a
`PolicyDelegateCollection` handles a sequence of policy+delegate pairs — except that
here **the delegate is supplied once**, at handle time.

## Contents

| File | Responsibility |
|---|---|
| `PolicyCollection.cs` | Core type: creation, `WithPolicy`, filters, result handlers, conversion, wrapping |
| `PolicyCollection.WithPolicy.cs` | Append-only shorthands: `WithRetry`, `WithWaitAndRetry`, `WithInfiniteRetry`, `WithWaitAndInfiniteRetry`, `WithFallback`, `WithSimple` |
| `PolicyCollection.HandleDelegate.cs` | `HandleDelegate` / `HandleDelegateAsync` — run the collection against a common delegate |
| `PolicyCollectionErrorProcessorRegistration.cs` | `WithErrorProcessorOf` / `WithErrorProcessor` — error processors for the **last** policy |
| `PolicyCollectionErrorProcessorRegistration.ForInnerError.cs` | `WithInnerErrorProcessorOf<TException>` — last policy, typed on inner exceptions |

## Mental model

```
PolicyCollection.Create()
    .WithRetry(2)                 // policy #1
    .WithFallback(...)            // policy #2
    .HandleDelegate(commonAction) // both policies see the SAME action
```

Handling semantics (shared with `PolicyDelegateCollection`):

```
Policy_1_Handle —— Failed ——> Policy_2_Handle —— Failed ——> ..
        |                            |
        | —— Success_Or_Canceled ——> Exit
```

If a policy's `PolicyResult.IsFailed` is `false` (or handling was canceled), the
collection stops. Otherwise the next policy runs. The outcome is a
`PolicyDelegateCollectionResult(<T>)` with `PolicyDelegateResults`,
`PolicyDelegatesUnused`, `LastPolicyResult`, `Result`, `IsFailed`, `IsCanceled`,
`IsSuccess`, and `LastPolicyResultFailedReason`.

## Creating a collection

```csharp
// One policy, added n times (the SAME instance — see Subtle points)
var c1 = PolicyCollection.Create(new RetryPolicy(2), 3);

// Several policies
var c2 = PolicyCollection.Create(retryPolicy, fallbackPolicy, simplePolicy);

// From an enumerable
var c3 = PolicyCollection.Create(policies);

// Fluent, in-place — the recommended style for library policies
var c4 = PolicyCollection.Create()
    .WithRetry(2)
    .WithFallback(() => GetFallbackValue());
```

`WithPolicy(IPolicyBase)` (and `WithPolicy(Func<IPolicyBase>)`) appends an existing
policy. All `WithRetry`/`WithFallback`/`WithSimple` shorthands ultimately call it.

## Handling a common delegate

```csharp
var result = PolicyCollection.Create()
    .WithRetry(2)
    .WithFallback(() => File.ReadAllLines(tempPath))
    .AddPolicyResultHandlerForAll<string[]>(pr =>
        pr.Errors.ForEach(ex => logger.Error(ex.Message)))
    .HandleDelegate(() => File.ReadAllLines(filePath));

if (result.IsSuccess)
{
    result.Result.ToList().ForEach(Console.WriteLine);
}
```

Overloads cover `Action`, `Func<CancellationToken, Task>`, `Func<T>`, and
`Func<CancellationToken, Task<T>>`, plus async variants with a `configAwait`
parameter. A `null` delegate produces a failed result with
`PolicyResultFailedReason.DelegateIsNull` instead of throwing.

## Building reusable handlers

```csharp
// IPolicyDelegateCollectionHandler(<T>) — injectable Handle/HandleAsync
IPolicyDelegateCollectionHandler<string[]> handler =
    collection.BuildCollectionHandlerFor(() => File.ReadAllLines(path));

// Same thing, via an explicit PolicyDelegateCollection
var pdCollection = collection.ToPolicyDelegateCollection(() => File.ReadAllLines(path));
```

`ToPolicyDelegateCollection` pairs **every policy in the collection** with the
given common delegate, producing a `PolicyDelegateCollection(<T>)` that can handle
other delegates later.

## Configuring the collection

### Filters

| Method family | Target |
|---|---|
| `IncludeErrorForAll` / `ExcludeErrorForAll` | Every policy **already in** the collection |
| `IncludeErrorForLast` / `ExcludeErrorForLast` | Last policy only |
| `IncludeErrorSet` / `ExcludeErrorSet` | Last policy only (`TException1,TException2` or `IErrorSet`) |
| `IncludeInnerError` / `ExcludeInnerError` | Last policy only, inner-exception filters |

### PolicyResult handlers

| Method family | Target |
|---|---|
| `AddPolicyResultHandlerForAll(<T>)` | Every policy already in the collection; `excludeLastPolicy: true` skips the last |
| `AddPolicyResultHandlerForLast(<T>)` | Last policy only |
| `SetPolicyResultFailedIf(<T>)` | Last policy only — sets `IsFailed` from a predicate |

Handler overloads accept `Action<PolicyResult(<T>)>`, token-based actions, and
`Func<PolicyResult(<T>), Task>` (with optional `CancellationType`).

### Error processors

`WithErrorProcessorOf(...)` and `WithErrorProcessor(...)` (in
`PolicyCollectionErrorProcessorRegistration`) add a processor to the **last**
policy's `PolicyProcessor`. The same is true for
`WithInnerErrorProcessorOf<TException>(...)`. Unlike `IncludeErrorForAll`, there is
no `ForAll` variant on the collection itself — for all-policy processors, add them
to each policy, or use `IncludeErrorForAll`-style filters plus per-policy setup.

### "ForAll only sees what is already there"

`...ForAll(...)` methods iterate the collection's current list. Policies added
**later** do not receive filters or handlers already applied. Configure first,
then append — or re-apply after appending. Calling any of these methods on an
empty collection has no effect.

## Wrapping the whole collection

```csharp
// WrapUp returns an OuterPolicyRegistrar — use .OuterPolicy to get the wrapper
var outer = PolicyCollection.Create()
    .WithRetry(2)
    .WrapUp(new SimplePolicy())
    .OuterPolicy;

// Then is a shorthand that returns the wrapper policy directly
var outer2 = PolicyCollection.Create()
    .WithRetry(2)
    .Then(new FallbackPolicy().WithFallbackAction(() => { }));
```

`WrapUp`/`Then` accept `ThrowOnWrappedCollectionFailed`:

| Value | Behavior when the last policy in the collection fails |
|---|---|
| `LastError` (default) | The last policy's unprocessed error is rethrown (`PolicyResult.UnprocessedError` semantics) |
| `CollectionError` | `PolicyDelegateCollectionException(<T>)` aggregating all errors; `PolicyResultHandlerFailedException` if the last failure came from a result handler |
| `None` | Rejected — `ArgumentException` |

The wrapper policy catches that rethrown exception in its own processor. With a
`SimplePolicy` wrapper this means the outer `PolicyResult` has `IsFailed = false`,
`NoError = false`, populated `Errors` and `WrappedPolicyResults` — the inner failure
is *recorded*, not re-failed. With a `RetryPolicy` wrapper, retries re-execute the
whole collection; with a `FallbackPolicy`, the fallback recovers.

Results of the inner collection are exposed on the outer result via
`WrappedPolicyResults`.

## Comparison

### vs. `PolicyDelegateCollection`

Both compose policies and both walk them sequentially until success/cancellation.
The difference is **what is bound when**.

| | `PolicyCollection` | `PolicyDelegateCollection(<T>)` |
|---|---|---|
| Unit of composition | Policies only (`IPolicyBase`) | Policy **+ its own delegate** (`PolicyDelegate(<T>)`) |
| Delegate binding | One common delegate, supplied at `HandleDelegate` / `BuildCollectionHandlerFor` | Each element carries its own delegate (`AndDelegate`, `WithPolicyAndDelegate`, or shorthand `With...` + `AndDelegate`) |
| Result type | `PolicyDelegateCollectionResult(<T>)` (same) | `PolicyDelegateCollectionResult(<T>)` |
| When it shines | "Same operation, many retry/fallback strategies" | "Different operations tried in order" |
| Conversion | `ToPolicyDelegateCollection(commonDelegate)` pairs each policy with the common delegate | Not applicable — delegates are already per-element |

Use `PolicyCollection` when every policy should see the **same** work. Use
`PolicyDelegateCollection` when you want a cascade of **different** attempts
(connect to factory1, then factory2, ...). Converting a `PolicyCollection` into a
`PolicyDelegateCollection` reuses the policies against a fixed delegate.

### vs. `PipelineFuncBuilder`

`PipelineFuncBuilder` composes **functions**, not policies. Each step is
`Func<TPrev, TNext>` wrapped in a policy, and the pipeline produces a
`PipelineResult<TOut>`.

| | `PolicyCollection` | `PipelineFuncBuilder` |
|---|---|---|
| Composes | Policies over one delegate | Typed function steps (`TIn → ... → TOut`) |
| Delegate | Single common delegate | One function per step, chained by types |
| Per-step error config | Collection-level filters/handlers (ForAll/ForLast) | `ConfigureErrorProcessors` / `OnError` per step |
| Failure model | Sequential retry of the same delegate across policies | Pipeline stops at the failed step; remaining steps do not run |
| Result | `PolicyDelegateCollectionResult(<T>)` | `PipelineResult<TOut>` (`Result`, `IsFailed`, `IsCanceled`, `IsSuccess`) |
| Overlap | Both accept `IPolicyBase` per unit | Both accept `IPolicyBase` per unit |

Rule of thumb:

- **Same work, several resilience strategies** → `PolicyCollection`.
- **Several different works, try until one succeeds** → `PolicyDelegateCollection`.
- **Typed multi-step transformation with per-step policies** → `PipelineFuncBuilder`.

## Subtle points

1. **Filters and handlers mutate policies in place.** `IncludeErrorForAll`,
   `AddPolicyResultHandlerForAll`, `WithErrorProcessorOf`, etc. change the
   underlying policy objects. Create policies for the collection on the spot, or do
   not reuse them elsewhere. Prefer the `With...` shorthands, which build fresh
   policies.

2. **`Create(policy, n)` adds the same instance `n` times.** State on that policy
   (error filters, handlers, processors) is shared across all `n` slots. For
   independent copies, call `WithRetry(...)`/`WithPolicy(...)` `n` times instead.

3. **`...ForAll` is a snapshot.** It only affects policies already in the list.
   Order your fluent calls so configuration comes after the relevant
   `With...`/`WithPolicy` calls.

4. **`WithErrorProcessorOf` / `WithInnerErrorProcessorOf` target the last policy.**
   There is no collection-level `ForAll` processor registration. For a processor on
   every policy, add it per policy (or configure before appending).

5. **Empty collections are legal but inert.** Filter/handler methods do nothing.
   `HandleDelegate` on an empty collection succeeds with an empty result (all
   flags false, `LastPolicyResultFailedReason.None`). A **null** delegate is
   different: `IsFailed = true`, `FailedReason = DelegateIsNull`.

6. **`Then` throws `ArgumentNullException` for a null wrapper.** `WrapUp` does not
   null-check; a null wrapper fails later inside `OuterPolicyRegistrar`.

7. **Wrapped-collection failure mode is opt-in.** Default `LastError` surfaces the
   last policy's error. `CollectionError` aggregates. `None` is invalid for
   `WrapUp`/`Then`.

8. **Wrapper policies can "swallow" the collection failure.** A `SimplePolicy`
   wrapper records inner errors (`Errors`, `WrappedPolicyResults`) but reports
   `IsFailed = false` / `IsPolicySuccess = true`. Inspect `WrappedPolicyResults`
   (or use a `RetryPolicy` wrapper if you need outer failure).

9. **Handler execution order matters.** `PolicyResult` handlers run in the order
   they were added. Add a handler that calls `SetFailed`/`SetPolicyResultFailedIf`
   *before* handlers that read `IsFailed`.

10. **`WithFallback(..., onlyGenericFallbackForGenericDelegate: true)`** enforces
    type-safe fallbacks for generic delegates; the default `false` may yield
    `default(T)` on generic paths.

