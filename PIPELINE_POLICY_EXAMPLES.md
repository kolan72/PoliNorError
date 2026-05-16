# PipelineFunc Policy Support - Examples

This document demonstrates how to use different policies with `PipelineFuncBuilder`.

## Overview

The `PipelineFuncBuilder` now supports using different policies (SimplePolicy, RetryPolicy, FallbackPolicy) for each step in the pipeline. This allows you to build resilient data processing pipelines with sophisticated error handling.

## Basic Usage (Backward Compatible)

The existing API continues to work without any changes:

```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.Length)
    .AddFunc(x => x * 2)
    .Build();

var result = pipeline("hello", CancellationToken.None);
// result.Result == 10
```

## Using RetryPolicy

### AddFuncWithRetry

Add a step that automatically retries on failure:

```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.Length)
    .AddFuncWithRetry(
        x => CallExternalApi(x),  // May fail temporarily
        retryCount: 3)
    .Build();
```

### StartWithRetry

Start the pipeline with a retry policy:

```csharp
var pipeline = PipelineFuncBuilder
    .StartWithRetry(
        (string s) => FetchDataFromApi(s),
        retryCount: 5,
        retryDelay: RetryDelay.Exponential(TimeSpan.FromSeconds(1)))
    .AddFunc(x => ProcessData(x))
    .Build();
```

### Infinite Retries

For operations that must eventually succeed:

```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.ToUpper())
    .AddFuncWithInfiniteRetry(
        x => SendToQueue(x),
        retryDelay: RetryDelay.Linear(TimeSpan.FromSeconds(5)))
    .Build();
```

## Using FallbackPolicy

### AddFuncWithFallback

Provide a fallback value when an operation fails:

```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.Length)
    .AddFuncWithFallback(
        x => GetFromCache(x),
        fallback: () => GetDefaultValue())
    .Build();
```

### Fallback with CancellationToken

```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => ParseJson(s))
    .AddFuncWithFallback(
        x => ValidateData(x),
        fallback: (ct) => GetDefaultValidation(ct))
    .Build();
```

### StartWithFallback

```csharp
var pipeline = PipelineFuncBuilder
    .StartWithFallback(
        (string s) => LoadConfigFromFile(s),
        fallback: () => GetDefaultConfig())
    .AddFunc(x => ApplyConfig(x))
    .Build();
```

## Using Custom Policies

### Pass Any Policy

You can pass any `IPolicyBase` implementation:

```csharp
var customRetryPolicy = new RetryPolicy(3)
    .IncludeError<HttpRequestException>()
    .WithWait(TimeSpan.FromSeconds(2));

var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.Trim())
    .AddFunc(x => CallApi(x), customRetryPolicy)
    .Build();
```

### Complex Policy Configuration

```csharp
var retryPolicy = new RetryPolicy(5)
    .IncludeError<TimeoutException>()
    .IncludeError<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.ServiceUnavailable)
    .WithWait(RetryDelay.Exponential(TimeSpan.FromSeconds(1)));

var fallbackPolicy = new FallbackPolicy()
    .WithFallbackFunc(() => CachedData())
    .IncludeError<Exception>();

var pipeline = PipelineFuncBuilder
    .StartWith((string url) => url.ToLower(), retryPolicy)
    .AddFunc(x => FetchData(x), retryPolicy)
    .AddFunc(x => ParseData(x), fallbackPolicy)
    .Build();
```

## Mixing Policies in a Pipeline

Different steps can use different policies:

```csharp
var pipeline = PipelineFuncBuilder
    // Step 1: Simple policy (default)
    .StartWith((string s) => s.Trim())
    
    // Step 2: Retry policy for external API call
    .AddFuncWithRetry(
        x => CallExternalApi(x),
        retryCount: 3)
    
    // Step 3: Fallback policy for validation
    .AddFuncWithFallback(
        x => ValidateResponse(x),
        fallback: () => DefaultValidation())
    
    // Step 4: Custom retry policy with specific error handling
    .AddFunc(
        x => SaveToDatabase(x),
        new RetryPolicy(5).IncludeError<DbException>())
    
    .Build();
```

## Real-World Example: Data Processing Pipeline

```csharp
public class DataProcessor
{
    public Func<string, CancellationToken, PipelineResult<ProcessedData>> CreatePipeline()
    {
        var retryPolicy = new RetryPolicy(3)
            .WithWait(RetryDelay.Exponential(TimeSpan.FromSeconds(1)))
            .IncludeError<HttpRequestException>()
            .IncludeError<TimeoutException>();

        var fallbackPolicy = new FallbackPolicy()
            .WithFallbackFunc(() => GetCachedData());

        return PipelineFuncBuilder
            // Step 1: Validate input (no special policy needed)
            .StartWith((string input) => ValidateInput(input))
            
            // Step 2: Fetch data with retry
            .AddFunc(x => FetchFromApi(x), retryPolicy)
            
            // Step 3: Transform data with fallback
            .AddFuncWithFallback(
                x => TransformData(x),
                fallback: () => DefaultTransformation())
            
            // Step 4: Enrich data with retry
            .AddFuncWithRetry(
                x => EnrichWithMetadata(x),
                retryCount: 2)
            
            // Step 5: Save with infinite retry (must succeed)
            .AddFuncWithInfiniteRetry(
                x => SaveToStorage(x),
                retryDelay: RetryDelay.Linear(TimeSpan.FromSeconds(5)))
            
            .Build();
    }

    private string ValidateInput(string input) => 
        string.IsNullOrEmpty(input) ? throw new ArgumentException() : input;
    
    private RawData FetchFromApi(string input) => /* ... */;
    private TransformedData TransformData(RawData data) => /* ... */;
    private EnrichedData EnrichWithMetadata(TransformedData data) => /* ... */;
    private ProcessedData SaveToStorage(EnrichedData data) => /* ... */;
    private TransformedData DefaultTransformation() => /* ... */;
    private RawData GetCachedData() => /* ... */;
}
```

## Error Handling with OnError

You can still use `OnError` to log or handle errors at each step:

```csharp
var pipeline = PipelineFuncBuilder
    .StartWithRetry((string s) => FetchData(s), retryCount: 3)
    .OnError((ex, info) => 
    {
        _logger.LogError(ex, "Failed to fetch data for {Input}", info.Param);
    })
    .AddFuncWithFallback(
        x => ProcessData(x),
        fallback: () => DefaultData())
    .OnError(async (ex, info) => 
    {
        await _telemetry.TrackExceptionAsync(ex);
    })
    .Build();
```

## Benefits

1. **Resilience**: Automatic retries for transient failures
2. **Graceful Degradation**: Fallback values when operations fail
3. **Flexibility**: Mix different policies in the same pipeline
4. **Backward Compatible**: Existing code continues to work
5. **Type-Safe**: Full compile-time type checking
6. **Composable**: Build complex error handling strategies

## API Summary

### Factory Methods
- `PipelineFuncBuilder.StartWith<TIn, TOut>(func)` - Default SimplePolicy
- `PipelineFuncBuilder.StartWith<TIn, TOut>(func, policy)` - Custom policy
- `PipelineFuncBuilder.StartWithRetry<TIn, TOut>(func, retryCount, retryDelay?)` - RetryPolicy
- `PipelineFuncBuilder.StartWithInfiniteRetry<TIn, TOut>(func, retryDelay?)` - Infinite RetryPolicy
- `PipelineFuncBuilder.StartWithFallback<TIn, TOut>(func, fallbackFunc)` - FallbackPolicy

### Builder Methods
- `.AddFunc<TNext>(func)` - Default SimplePolicy
- `.AddFunc<TNext>(func, policy)` - Custom policy
- `.AddFuncWithRetry<TNext>(func, retryCount, retryDelay?)` - RetryPolicy
- `.AddFuncWithInfiniteRetry<TNext>(func, retryDelay?)` - Infinite RetryPolicy
- `.AddFuncWithFallback<TNext>(func, fallbackFunc)` - FallbackPolicy
- `.OnError(action)` - Error handling (sync)
- `.OnError(func)` - Error handling (async)
- `.Build()` - Build the pipeline

## Migration Guide

### Before (SimplePolicy only)
```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.Length)
    .AddFunc(x => x * 2)
    .Build();
```

### After (with RetryPolicy)
```csharp
var pipeline = PipelineFuncBuilder
    .StartWith((string s) => s.Length)
    .AddFuncWithRetry(x => x * 2, retryCount: 3)
    .Build();
```

No breaking changes - all existing code continues to work!
