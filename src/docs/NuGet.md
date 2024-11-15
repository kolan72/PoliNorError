PoliNorError is a library that provides error handling capabilities through Retry and Fallback policies or processors.  
Also provides SimplePolicy and TryCatch mimicking.  
The library can also handle potential exceptions within the catch block and offers various configuration options.

```csharp
var result = 
	new RetryPolicy(2)

	.IncludeError<SomeException>()

	.WithErrorProcessorOf((Exception ex, ProcessingErrorInfo pi) =>
		logger.LogError(ex, 
		"Policy processed exception on {Attempt} attempt:", 
		(pi as RetryProcessingErrorInfo).RetryCount + 1))

	.AddPolicyResultHandler((PolicyResult pr) =>
		logger.LogWarning(
		"{Errors} exceptions were thrown.",
		pr.Errors.Count()))

	.Handle(ActionThatCanThrowSomeException);
```