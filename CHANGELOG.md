## 2.0.0-alpha  

- Introduce `SimplePolicy` and `SimplePolicyProcessor` classes
- Drop the  `PolicyDelegateCollection(<T>).WithCommonResultErrorsHandler` method
- Alter the name of the policy `WithPolicyResultHandler` method to `AddPolicyResultHandler`
- Change the name of the `PolicyDelegateCollection(<T>).WithCommonDelegate` methods to `SetCommonDelegate`
- Policy `ForError` methods were renamed to `IncludeError`
- Rename `PolicyDelegateCollection(<T>).WithCommonResultHandler` to `AddPolicyResultHandlerForAll`
- Rename `PolicyDelegateCollection(<T>).ForError` methods to `IncludeErrorForAll`
- `PolicyDelegateCollection(<T>).ExcludeError` methods was renamed to `ExcludeErrorForAll`
- Get rid of `RetryErrorProcessor`, `FallbackErrorProcessor` classes.
- Rename the `PolicyHandledResult` and `PolicyHandledInfo` classes (now `PolicyDelegateResult` and `PolicyDelegateInfo` respectively) and the related classes.
- The name of the `PolicyDelegateCollection(T>)` creation methods is started with the `Create` prefix.


## 1.0.4  

- Reduce the number of public classes and methods that are made internal.

## 1.0.3

- Fix not correct IntelliSense order for RetryPolicy ctors (Issue #1)
- Fix bug with empty HandleResultErrors (Issue #2)