using NUnit.Framework;
using System;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal static class TestHandlingForInnerError
	{
		internal static void IncludeInnerErrorInPolicy<T>(IWithInnerErrorFilter<T> policy, bool withInnerError, bool? satisfyFilterFunc) where T: IWithInnerErrorFilter<T>
		{
			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policy.IncludeInnerError<TestInnerException>();
			}
			else
			{
				policy.IncludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}
		}

		internal static void ExcludeInnerErrorFromPolicy<T>(IWithInnerErrorFilter<T> policy, bool withInnerError, bool? satisfyFilterFunc) where T : IWithInnerErrorFilter<T>
		{
			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policy.ExcludeInnerError<TestInnerException>();
			}
			else
			{
				policy.ExcludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}
		}

		internal static void HandlePolicyWithIncludeInnerErrorFilter(IPolicyBase policyBase, Action actionToHandle, bool withInnerError, bool? satisfyFilterFunc)
		{
			var result = policyBase.Handle(actionToHandle);
			PolicyResultAsserts.AfterHandlingWithIncludeInnerErrorFilter(result, withInnerError, satisfyFilterFunc);
		}

		internal static void HandlePolicyWithExcludeInnerErrorFilter(IPolicyBase policyBase, Action actionToHandle, bool withInnerError, bool? satisfyFilterFunc)
		{
			var result = policyBase.Handle(actionToHandle);
			PolicyResultAsserts.AfterHandlingWithExcludeInnerErrorFilter(result, withInnerError, satisfyFilterFunc);
		}

		internal static Action GetAction(bool withInnerError, bool? satisfyFilterFunc)
		{
			if (withInnerError)
			{
				if (satisfyFilterFunc == true)
				{
					return ((Action<string>)ActionWithInnerWithMsg).Apply("Test");
				}
				else if (satisfyFilterFunc == false)
				{
					return ((Action<string>)ActionWithInnerWithMsg).Apply("Test2");
				}
				else
				{
					return ActionWithInner;
				}
			}
			else
			{
				return Action;
			}
		}

		internal static class PolicyResultAsserts
		{
			internal static void AfterHandlingWithIncludeInnerErrorFilter(PolicyResult result, bool withInnerError, bool? satisfyFilterFunc)
			{
				if (withInnerError)
				{
					if (satisfyFilterFunc == true)
					{
						Assert.That(result.ErrorFilterUnsatisfied, Is.False);
					}
					else if (satisfyFilterFunc == false)
					{
						Assert.That(result.ErrorFilterUnsatisfied, Is.True);
					}
					else
					{
						Assert.That(result.ErrorFilterUnsatisfied, Is.False);
					}
				}
				else
				{
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
			}

			internal static void AfterHandlingWithExcludeInnerErrorFilter(PolicyResult result, bool withInnerError, bool? satisfyFilterFunc)
			{
				if (withInnerError)
				{
					if (satisfyFilterFunc == true)
					{
						Assert.That(result.ErrorFilterUnsatisfied, Is.True);
					}
					else if (satisfyFilterFunc == false)
					{
						Assert.That(result.ErrorFilterUnsatisfied, Is.False);
					}
					else
					{
						Assert.That(result.ErrorFilterUnsatisfied, Is.True);
					}
				}
				else
				{
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
			}
		}
	}
}
