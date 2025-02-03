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
		((RetryProcessingErrorInfo)pi).RetryCount + 1))

	.AddPolicyResultHandler((PolicyResult pr) =>
		logger.LogWarning(
		"{Errors} exceptions were thrown.",
		pr.Errors.Count()))

	.Handle(ActionThatCanThrowSomeException);


var simplePolicyResult = new SimplePolicy()

	.WithErrorContextProcessorOf<int>((ex, pi) =>
		loggerTest.LogError(ex,
		"Delegate call with parameter value {Param} failed with an exception.", 
		pi.Param))
		
	//If the random value variable is zero,
	//the previous line of code
	//logs an error with {Param} set to zero.
	.Handle((i) => 5 / i, random);
```