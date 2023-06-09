# PoliNorError  
[![nuget](https://img.shields.io/nuget/v/PoliNorError)](https://www.nuget.org/packages/PoliNorError/)

![alt text](https://github.com/kolan72/kolan72.github.io/blob/master/images/PoliNorError.png?raw=true)  

PoliNorError is a library that provides error handling capabilities through Retry and Fallback policies. The library has a specific focus on handling potential exceptions within the catch block and offers various configuration options.
Heavily inspired by  [Polly](https://github.com/App-vNext/Polly).

## Key Features
- Implements two commonly used resiliency patterns - Retry and Fallback
- Put emphasize on error handling within the catch block
- Extensibility: error handling within the catch block can be extended by error processors 
- Simplicity: one policy type for sync and async, and a generic and not generic delegate
- Composability: policies and delegates can be composed into a single `PolicyDelegateCollection`
- Flexible filters can be set for errors that should be handled
- Policy can be wrapped by other policy
- Func and Action delegates can be called in a resilient manner
- Convenient API with methods with minimum of optional parameters.
- High Test Coverage.
- Targets .NET Standard 2.0+

## Usage
The term "handling delegate" refers to the process of handling errors that occur when a delegate is executed. 
The types of delegates that can be handled include:
- `Action`
- `Func<T>`
- `Func<CancellationToken, Task>`
- `Func<CancellationToken, Task<T>>`

Handling delegate is performed through the use of policy processors, which are classes that implement policy-specific interfaces inherited from the `IPolicyProcessor` interface.  
A policy is a wrapper for the policy processor that adapts it to the `IPolicyBase` interface with `Handle` and `HandleAsync` methods for handling delegates.  
For retries using default retry policy processor:
```csharp
var result = RetryProcessor
			.CreateDefault()
			.WithWait(TimeSpan.FromMilliseconds(300))
			.Retry(() => DoSomethingRepeatedly()), 5);
```
With [`RetryPolicy`](#retrypolicy), more complex case:
```csharp
var result = await new RetryPolicy(5)
	                 .ExcludeError<DbEntityValidationException>()
			 .WithWait((retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
			 .WithPolicyResultHandler((pr) =>
							{ 
								if (pr.IsCanceled) 
									logger.Error("The operation was canceled.");
							}
						 )
			.HandleAsync(async (ct) => await dbContext.SaveChangesAsync(ct), token);
```
For fallback using default fallback policy processor:
```csharp
var result = FallbackProcessor
                           .CreateDefault()
                           .ForError<ObjectDisposedException>()
                           .WithErrorProcessorOf((ex) => logger.Error(ex.Message))
                           .Fallback(someDisposableObj.SomeMethod, 
                                    (_) => new SomeFallbackObj().SomeFallbackMethod());
```
With [`FallbackPolicy`](#fallbackpolicy):
```csharp
var result = new FallbackPolicy()
                             .ForError<ObjectNotFoundException>()
                             .WithFallbackFunc<Email>(() => UserManager.GetGuestEmail())
                             .Handle(() => UserManager.GetUserEmail(userId));
```
Error handling within the policy processor's catch block can be extended by using [error processors](#error-processors), which are classes that implement the `IErrorProcessor` interface. For example, one of such classes - `DelayErrorProcessor` specify amount of delay time before continuing with the handling process.  
Errors handling allows for further specification through the use of [error filters](#error-filters).  
The results of handling are stored in the [PolicyResult](#policyresult) class.  
Using policy, you can extra handle `PolicyResult` by [`PolicyResult` handlers](#policyresult-handlers) after policy processor processing.  
A policy can be combined with a delegate in the [`PolicyDelegate`](#policydelegate) class. The `PolicyDelegate` object, in turn, can be added to the [`PolicyDelegateCollection`](#policydelegatecollection). In this case, each delegate will be handled according to its policy.  
The classes `PolicyResult`, `PolicyDelegate`, `PolicyDelegateCollection` and some other handling-related classes have corresponding generic versions.

### PolicyResult
Handling begins when an exception occurs during the execution of the delegate. At first, exception will be stored in the `Errors` property (for retry-related classes, this is by default and can be customized).  
Later on, the policy processor will try to handle delegate and store results in the `PolicyResult` properties.  
The most crucial property is the `IsFailed` property. If it equals `true`, the delegate was not able to be handled.  
It can happen due to these reasons:
-   The error filter conditions are not satisfied (the  `ErrorFilterUnsatisfied`  property will also be set to `true`).
-   Policy-specific conditions failed (for example, the number of permitted retries is exceeded for retry processor).
-   A critical error has occurred within the catch block, specifically related to the calling of the saving error delegate for  `RetryPolicy`  or calling the fallback delegate for  `FallbackPolicy` (the  `IsCritical`  property of the  `CatchBlockException`  object will also be set to  `true`).
 -   Cancellation occurs after the first call of the delegate you handle.

In case the  `CancellationToken` passed as a parameter in handling method is canceled, the  `IsCanceled`  property will be set to  `true`. But  cancellation may occur before the first call of the delegate. However, the `IsFailed` property still equals `false` because no delegate was executed.  To ensure that the policy or policy processor has handled  executed delegate successfully, check the  `IsSuccess`  property, that equals  `true`  only if the  `IsFailed`  and  `IsCanceled`  properties are both  `false`.  
Check the  `IsOk`  property to ensure that there were no errors in handling delegate at all. But an error may still happen in `PolicyResult` handlers. In this case it will be stored in the `HandleResultErrors` property, but it will not affect the`IsOk` or other `PolicyResult` properties.  
If an error occurs within the catch block, it will be stored in the  `CatchBlockErrors`  property that is collection of the `CatchBlockException`  objects.  
For generic delegates, a return value will be stored in the  `Result`  property if handling was successful. 

### Error processors
You can add an error processor to any policy or policyprocessor class by using the `WithErrorProcessor` or `WithErrorProcessorOf` methods.  
In the common case, an error processor is a class that implements the `IErrorProcessor` interface. You can create an error processor from delegates with the argument of the `Exception` type by using overloads of the `WithErrorProcessorOf` method.  
The whole list of such error processor delegates:
- `Action<Exception>`
- `Action<Exception, CancellationToken>`
- `Func<Exception, Task>`
- `Func<Exception, CancellationToken, Task>`

Error processors are handled one by one by the `BulkErrorProcessor` class. To customize this behavior, you could implement the `IBulkErrorProcessor` interface and use one of the policy or policy processor class constructors.

### Error filters
You can specify error filter for policy or policy processor:
```csharp
 var result = new FallbackPolicy()
                                .ForError<SqlException>()
                                .ExcludeError<SqlException>(ex => ex.Number == 1205)
                                .WithFallbackAction(() => cmd2.ExecuteNonQuery())
                                .Handle(() => cmd1.ExecuteNonQuery());
```
Error can be handled if any conditions specified by `ForError` is satisfied and all conditions specified by `ExcludeError` is unsatisfied.
There are no limitations on the number of filter conditions for both types. If no filter is set, all errors will be handled .
If filter conditions are unsatisfied, error handling break and set both the `IsFailed` and `ErrorFilterUnsatisfied` properies to `true`.

### PolicyResult handlers

When handling delegate by policy you can add so-called `PolicyResult` handlers using one of the method named  `WithPolicyResultHandler` or `WithPolicyResultErrorsHandler`.
 These methods accept delegate as a parameter and can extra handle `PolicyResult`:

- `Action<PolicyResult>`
- `Action<PolicyResult, CancellationToken>`
- `Action<IEnumerable<Exception>>`
- `Action<IEnumerable<Exception>, CancellationToken>`
- `Func<PolicyResult, Task>`
- `Func<PolicyResult, CancellationToken, Task>`
- `Func<IEnumerable<Exception>, Task>`
- `Func<IEnumerable<Exception>, CancellationToken, Task>`

For example:
```csharp
var result = await new RetryPolicy(5)
                            .WithPolicyResultHandler((pr) => { if (pr.IsOk) logger.Info("There were no errors.");})
                            .HandleAsync(async (ct) => await dbContext.SaveChangesAsync(ct), token);
```
The  `PolicyResult` handler can't change the `Result` property of `PolicyResult<T>`.

### RetryPolicy
`RetryPolicy` can be customized of your implementation of `IRetryProcessor` interface.  
The most crucial parameter for `RetryPolicy` setup is the count of retries.  
You can also specify the delay time before next retry with `WithWait(TimeSpan)` method, or use one of the overloads with Func, returning TimeSpan, for example:
```csharp
            var policy = new RetryPolicy(5)
                                    .WithWait((retryAttempt, ex) =>
                                    {
                                        logger.Error(ex.Message);
                                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                                    });
```
To retry infinitely, until it succeeds, use the`InfiniteRetries` method:
```csharp
            var possibleResult = await RetryPolicy
                                    .InfiniteRetries()
                                    .WithWait(TimeSpan.FromSeconds(1))
                                    .HandleAsync(async (ct) => 
                                                    await DoSomethingNearlyImpossibleAsync(ct), token);
```
These methods create the `DelayErrorProcessor` object behind the scene.  
The `WithWait` method also has overload that accept the `DelayErrorProcessor` argument. This method allows you to customize the delay behavior by inheriting from the  `DelayErrorProcessor` class.  

Faulted retries errors saving is configuring by `Action<PolicyResult, Exception>` parameter of one of the constructors and by default save errors in `PolicyResult.Errors` collection. For huge numbers of retries, memory-related exceptions may occur, and the handling process will be interrupted. You can pass your own delegate to avoid this.  

For testing purposes there is a `RetryPolicy` constructor that has `Action<RetryCountInfoOptions>` parameter.  

### FallbackPolicy
`FallbackPolicy` can be customized of your implementation of `IFallbackProcessor` interface.  
You can setup this policy for different return types:
```csharp
  var userFallbackPolicy = new FallbackPolicy()
                                        .WithFallbackFunc<User>(() => UserManager.GetGuestUser())
                                        .WithFallbackFunc<Email>(() => UserManager.GetGuestEmail());
```
And use it for handling delegates:
```csharp
    //Somewhere in your code:
    //resultUser will contain guest user in the Result property if any error occurs 
    var resultUser = userFallbackPolicy.Handle(() => UserManager.GetUser(userName));
     
    //resultEmail will contain guest email in the Result property if any error occurs 
    var resultEmail = userFallbackPolicy.Handle(() => UserManager.GetUserEmail(userId));                                   
```
The whole list of methods, accepting fallback delegate as an argument:

-    `WithFallbackAction`
-    `WithAsyncFallbackFunc`
-    `WithFallbackFunc<T>`
-    `WithAsyncFallbackFunc<T>`

If you try to handle a generic func delegate without a corresponding fallback delegate being set, the default value of type will be returned.

If an exception occurs during the calling of the fallback delegate, the `IsFailed` property of the `PolicyResult` object will be set to `true` and this exception will be wrapped in the `CatchBlockException` exception with  the `IsCritical` property  equal to `true`.

### Policy wrap
For wrap policy by other policy use `WrapPolicy` method, for example:
```csharp
	    var wrapppedPolicy = new RetryPolicy(3).ExcludeError<BrokerUnreachableException>();
            var fallBackPolicy = new FallbackPolicy()
                                            .WithFallbackFunc(() => connectionFactory2.CreateConnection());
            fallBackPolicy.WrapPolicy(wrapppedPolicy);
            var polResult = fallBackPolicy.Handle(() => connectionFactory1.CreateConnection());
```
Results of handling a wrapped policy are stored in the `WrappedPolicyResults` property.

### PolicyDelegate
A `PolicyDelegate` just pack delegate with a policy into a single object.
You can create `PolicyDelegate` object from policy and delegate:
```csharp
var policyDelegate = new RetryPolicy(1).ToPolicyDelegate(() => query.GetData());
```
For  handling delegate by the `PolicyDelegate` simply call `Handle` or `HandleAsync` method:
```csharp
var result = policyDelegate.Handle();
```

### PolicyDelegateCollection
You can create `PolicyDelegateCollection(<T>)` by using `Create` method. Having policy delegates or even policies only - by using `FromPolicyDelegates`, `FromPolicies` methods respectevly.  
With these methods
- `WithPolicy` (create the `PolicyDelegate` with a policy but without a delegate)
- `WithPolicyDelagate`
- `WithPolicyAndDelegate`
- `AndDelegate` (set delegate to last `PolicyDelegate` object in the collection)
- `WithCommonDelegate` (set the same delegate to elements that have already been added to the collection)

 or specific policy-related extensions methods and their overloads:
- `WithRetry`
- `WithWaitAndRetry`
- `WithInfiniteRetry`
- `WithWaitAndInfiniteRetry`
- `WithFallback`

you can further construct a collection in a fluent manner and call `HandleAll` or `HandleAllAsync` method.
Handling is smart - it checks the synchronicity type of all delegates in collection and calls the appropriate method behind the scene, which calls delegates in sync or async manner or in the miscellaneous way.  

You can establish a common `PolicyResult` handler for the entire collection by using either the `WithCommonResultHandler` or `WithCommonResultErrorsHandler` method. 
These methods require the same delegates types as `PolicyResult` handlers.  

This is an example of how to add retry policies and delegates with common `PolicyResult` handler:
```csharp
var result = PolicyDelegateCollection.Create()
                        .WithWaitAndRetry(3, TimeSpan.FromSeconds(2))
                        .AndDelegate(() => connectionFactory1.CreateConnection())
                        .WithRetry(5)
                        .AndDelegate(() => connectionFactory2.CreateConnection())
                        .WithCommonResultHandler((pr) =>
							{ 
								if (pr.IsCanceled) 
									logger.Error("The operation was canceled.");
							}
						 )
                        .HandleAll();
```
You can use ExcludeError and ForError methods to set filters on the entire collection:
```csharp
var result = PolicyDelegateCollection<int>.Create()
                         .WithRetry(5)
                         .WithFallback(() => cmd2.ExecuteNonQuery())
                         .WithCommonDelegate(() => cmd1.ExecuteNonQuery())
                         .ForError<SqlException>((ex) => ex.Number == 1205)
                         .HandleAll();
```
The process of handling policydelegates in collection will only continue if there has been no cancellation and the current policy handling has been unsuccessful (i.e. `IsFailed` of `PolicyResult` equals to `true`).  

Results of handling are stored in `PolicyDelegateCollectionResult(<T>)` that implements `IEnumerable<PolicyHandledResult(<T>)>` interface. The `PolicyHandledResult(<T>)` class in turn is just a wrapper around `PolicyResult` that contains `PolicyHandledInfo` class with information about policy and `MethodInfo` of delegate.  

The `PolicyDelegatesUnused` property contains a collection of policydelegates that were not handled due to the reasons described above.  

Check the `Status` property to find out how the last used policy handled the delegate.

For some purpurses  throw a special `PolicyDelegateCollectionHandleException` exception if the last policy in the collection fails may be useful. You can do it with the  `WithThrowOnLastFailed` method.

### Calling Func and Action delegates in a resilient manner
There are delegate extension methods that allow delegates to be called in a resilient manner.
Each method calls corresponding policy method behind the scene.
It is also possible to customize the calling by error processor delegates.
For example, with error processor from `Func<Exception, CancellationToken, Task>`:
```csharp
        Func<Exception, CancellationToken, Task> errorSaveFunc =  async(ex, ct) 
                                                                            => await errorSaver.SaveAsync(ex, ct);
        Task<PolicyResult<string>> GetUserEmailWithFallbackAsync(
                                                                Func<CancellationToken, Task<string>> emailFunc, CancellationToken token)
        {
            return emailFunc.InvokeWithFallbackAsync(async(ct) => 
                                                                await UserManager.GetFallbackEmailAsync(ct), 
                                                                errorSaveFunc, token);
        }
        //Somewhere in your code
        public async Task SendUserEmailAsync(CancellationToken token)
        {
            var res = await GetUserEmailWithFallbackAsync(async(ct) => await UserManager.GetUserEmailAsync(ct), token);
            if (res.IsSuccess)
                await emailSender.SendEmailAsync(res.Result, token);
            }
        }
```
If you want to use cancellation possibility for not cancelable error processor method use `RetryErrorProcessor` or `FallbackErrorProcessor` helper classes, for example :

```csharp
          Action action = () => SendEmail("someuser@somedomain.com");
	  Action<Exception> actionError = (ex) => logger.Error(ex.Message));
	  action.InvokeWithRetry(retryCount, 
			                     RetryErrorProcessor.From(actionError, 
			                                              ConvertToCancelableFuncType.Cancelable)
			                     );
```

Full list of extensions methods:

- `InvokeWithRetry(Async)`
- `InvokeWithWaitAndRetry(Async)`
- `InvokeWithRetryInfinite(Async)`
- `InvokeWithWaitAndRetryInfinite(Async)`
- `InvokeWithFallback(Async)`


### Usage recommendations
For simple use cases, you could use policy processors. If your case involves more complexity and requires wrapping other policy or handling of policy results, consider using a suitable policy or packing policy with a delegate in the `PolicyDelegate` object.
In certain scenarios, for example, where a large number of retries are required, the `PolicyDelegateCollection` may be useful.

### Nuances of using the library
All default policy processor classes that implement `IPolicyProcessor`  will handle the `OperationCanceledException` exception in a policy-specific way if the token is different from the one passed as argument. Otherwise, only the `PolicyResult`s properties `IsFailed` and `IsCanceled` will be set to `true` and handling will exit.  

Some library methods accept delegate argument that are not cancelable, but can still be canceled. Such methods also have an extra `ConvertToCancelableFuncType` argument type that shows how cancellation will be performed.
The default value of `ConvertToCancelableFuncType` as a method argument is the `Precancelable`, means that the delegate will not be executed if the token has already been canceled. If its equals `Cancelable`, a new task that supports cancellation will be used.  

Please note that `PolicyDelegateCollection.WithCommon...` methods  set the common delegate only for items that have already been added to the collection, not for new ones.  

For very large retry count memory-related error may occur. You can set "n-Time infinite" handling by creating `PolicyDelegateCollection` from `RetryPolicy` with max no-error retry count defined by experiment:

```csharp
var result = await PolicyDelegateCollection
                                    .FromOneClonedPolicy(new RetryPolicy(maxOfNoErrorRetries), nTimeInfinite)
                                    .WithCommonDelegate(funcForRetry)
                                    .HandleAllAsync(token);
```
In theory, it can support up to maxOfNoErrorRetries * int.MaxValue maximum number of retries.
Please don't forget about enabling `gcAllowVeryLargeObjects` setting in the config file.
