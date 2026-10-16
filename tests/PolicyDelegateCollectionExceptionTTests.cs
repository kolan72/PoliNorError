using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyDelegateCollectionExceptionTTests
	{
		[Test]
		public void Should_GetResults_Return_AllResultValues_In_Order()
		{
			var results = CreateTestResults(1, 2, 3);
			var exception = new PolicyDelegateCollectionException<int>(results);

			var getResults = exception.GetResults().ToList();

			Assert.That(getResults, Has.Count.EqualTo(3));
			Assert.That(getResults[0], Is.EqualTo(1));
			Assert.That(getResults[1], Is.EqualTo(2));
			Assert.That(getResults[2], Is.EqualTo(3));
		}

		[Test]
		public void Should_GetResults_Return_Default_For_FailedResultWithoutValue()
		{
			var polResult = new PolicyResult<int>();
			polResult.AddError(new Exception("Fail"));

			var policy = new RetryPolicy(1);
			var policyDelegate = policy.ToPolicyDelegate(() => 42);
			var handledResult = new PolicyDelegateResult<int>(polResult, policy.PolicyName, policyDelegate.GetMethodInfo());

			var exception = new PolicyDelegateCollectionException<int>(new[] { handledResult });

			var getResults = exception.GetResults().ToList();

			Assert.That(getResults, Has.Count.EqualTo(1));
			Assert.That(getResults[0], Is.EqualTo(default(int)));
		}

		[Test]
		public void Should_GetResults_ReturnCorrectValues_WhenResultHasMixedSuccessAndFailure()
		{
			var polResult1 = new PolicyResult<int>();
			polResult1.SetResult(10);
			polResult1.AddError(new Exception("Error1"));

			var polResult2 = new PolicyResult<int>();
			polResult2.SetResult(20);

			var polResult3 = new PolicyResult<int>();
			polResult3.AddError(new Exception("Error3"));

			var policy = new RetryPolicy(1);
			var policyDelegate = policy.ToPolicyDelegate(() => 42);

			var handledResults = new List<PolicyDelegateResult<int>>
			{
				new PolicyDelegateResult<int>(polResult1, policy.PolicyName, policyDelegate.GetMethodInfo()),
				new PolicyDelegateResult<int>(polResult2, policy.PolicyName, policyDelegate.GetMethodInfo()),
				new PolicyDelegateResult<int>(polResult3, policy.PolicyName, policyDelegate.GetMethodInfo()),
			};

			var exception = new PolicyDelegateCollectionException<int>(handledResults);

			var getResults = exception.GetResults().ToList();

			Assert.That(getResults, Has.Count.EqualTo(3));
			Assert.That(getResults[0], Is.EqualTo(10));
			Assert.That(getResults[1], Is.EqualTo(20));
			Assert.That(getResults[2], Is.EqualTo(default(int)));
		}

		[Test]
		public void Should_GetResults_Be_Lazyly_Evaluated()
		{
			var results = CreateTestResults(1, 2, 3);
			var exception = new PolicyDelegateCollectionException<int>(results);

			Assert.That(IsLazyValueCreated(exception), Is.False);

			exception.GetResults();

			Assert.That(IsLazyValueCreated(exception), Is.True);
		}

		[Test]
		public void Should_GetResults_EvaluateOnlyOnce()
		{
			var results = CreateTestResults(1, 2, 3, 4, 5);
			var exception = new PolicyDelegateCollectionException<int>(results);

			_ = exception.GetResults();
			var first = GetInternalResults(exception);
			_ = exception.GetResults();
			var second = GetInternalResults(exception);

			Assert.That(first, Is.SameAs(second));
		}

		[Test]
		public void Should_GetResults_ReturnSameInstance_OnMultipleCalls()
		{
			var results = CreateTestResults(1, 2);
			var exception = new PolicyDelegateCollectionException<int>(results);

			var first = exception.GetResults();
			var second = exception.GetResults();

			Assert.That(first, Is.SameAs(second));
		}

		[Test]
		public void Should_GetResults_ThreadSafe_ConcurrentCallsReturnConsistentResults()
		{
			var results = CreateTestResults(10, 20, 30, 40, 50);
			var exception = new PolicyDelegateCollectionException<int>(results);

			int threadCount = 10;
			var barrier = new Barrier(threadCount);
			var allResults = new List<int>[threadCount];

			var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
			{
				barrier.SignalAndWait();
				allResults[t] = exception.GetResults().ToList();
			})).ToArray();

			Task.WaitAll(tasks);

			for (int t = 0; t < threadCount; t++)
			{
				Assert.That(allResults[t], Has.Count.EqualTo(5));
				Assert.That(allResults[t][0], Is.EqualTo(10));
				Assert.That(allResults[t][1], Is.EqualTo(20));
				Assert.That(allResults[t][2], Is.EqualTo(30));
				Assert.That(allResults[t][3], Is.EqualTo(40));
				Assert.That(allResults[t][4], Is.EqualTo(50));
			}
		}

		[Test]
		public void Should_GetResults_ThreadSafe_AllCallsSeeSameArray()
		{
			var results = CreateTestResults(1, 2);
			var exception = new PolicyDelegateCollectionException<int>(results);

			int threadCount = 10;
			var barrier = new Barrier(threadCount);
			var instances = new IEnumerable<int>[threadCount];

			var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
			{
				barrier.SignalAndWait();
				instances[t] = exception.GetResults();
			})).ToArray();

			Task.WaitAll(tasks);

			for (int t = 1; t < threadCount; t++)
			{
				Assert.That(instances[t], Is.SameAs(instances[0]));
			}
		}

		[Test]
		public void Should_GetResults_ThreadSafe_RacingFirstAccess_AllThreadsGetSameValue()
		{
			var results = CreateTestResults(100, 200, 300);
			var exception = new PolicyDelegateCollectionException<int>(results);

			int threadCount = 20;
			var barrier = new Barrier(threadCount);
			var collected = new IReadOnlyList<int>[threadCount];

			var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
			{
				barrier.SignalAndWait();
				collected[t] = exception.GetResults() as IReadOnlyList<int>;
			})).ToArray();

			Task.WaitAll(tasks);

			var reference = collected[0];
			Assert.That(reference, Is.Not.Null);
			for (int t = 1; t < threadCount; t++)
			{
				Assert.That(collected[t], Is.SameAs(reference));
			}
		}

		[Test]
		public void Should_InnerExceptions_ContainAllErrors_FromAllResults()
		{
			var polResult1 = new PolicyResult<int>();
			polResult1.SetResult(1);
			polResult1.AddError(new Exception("ErrorA"));

			var polResult2 = new PolicyResult<int>();
			polResult2.SetResult(2);
			polResult2.AddError(new Exception("ErrorB"));
			polResult2.AddError(new Exception("ErrorC"));

			var policy = new RetryPolicy(1);
			var policyDelegate = policy.ToPolicyDelegate(() => 42);

			var handledResults = new List<PolicyDelegateResult<int>>
			{
				new PolicyDelegateResult<int>(polResult1, policy.PolicyName, policyDelegate.GetMethodInfo()),
				new PolicyDelegateResult<int>(polResult2, policy.PolicyName, policyDelegate.GetMethodInfo()),
			};

			var exception = new PolicyDelegateCollectionException<int>(handledResults);

			Assert.That(exception.InnerExceptions.Count(), Is.EqualTo(3));
			Assert.That(exception.InnerExceptions.Select(e => e.Message), Has.Member("ErrorA"));
			Assert.That(exception.InnerExceptions.Select(e => e.Message), Has.Member("ErrorB"));
			Assert.That(exception.InnerExceptions.Select(e => e.Message), Has.Member("ErrorC"));
		}

		[Test]
		public void Should_Message_ContainPolicyInfo_ForEachResult()
		{
			var polResult1 = new PolicyResult<int>();
			polResult1.SetResult(1);
			polResult1.AddError(new Exception("Test1"));

			var polResult2 = new PolicyResult<int>();
			polResult2.SetResult(2);
			polResult2.AddError(new Exception("Test2"));

			var policy = new RetryPolicy(1);
			var policyDelegate = policy.ToPolicyDelegate(() => 42);

			var handledResults = new List<PolicyDelegateResult<int>>
			{
				new PolicyDelegateResult<int>(polResult1, policy.PolicyName, policyDelegate.GetMethodInfo()),
				new PolicyDelegateResult<int>(polResult2, policy.PolicyName, policyDelegate.GetMethodInfo()),
			};

			var exception = new PolicyDelegateCollectionException<int>(handledResults);

			Assert.That(exception.Message, Does.Contain("Test1"));
			Assert.That(exception.Message, Does.Contain("Test2"));
			Assert.That(exception.Message, Does.Contain("RetryPolicy"));
		}

		[Test]
		public void Should_GetResults_WithSingleResult()
		{
			var results = CreateTestResults(42);
			var exception = new PolicyDelegateCollectionException<int>(results);

			var getResults = exception.GetResults().ToList();

			Assert.That(getResults, Has.Count.EqualTo(1));
			Assert.That(getResults[0], Is.EqualTo(42));
		}

		[Test]
		public void Should_GetResults_WhenCalledThroughBaseType()
		{
			var results = CreateTestResults(7, 8);
			PolicyDelegateCollectionException baseException = new PolicyDelegateCollectionException<int>(results);

			Assert.That(baseException, Is.InstanceOf<PolicyDelegateCollectionException<int>>());

			var typedResults = ((PolicyDelegateCollectionException<int>)baseException).GetResults().ToList();
			Assert.That(typedResults, Has.Count.EqualTo(2));
			Assert.That(typedResults[0], Is.EqualTo(7));
			Assert.That(typedResults[1], Is.EqualTo(8));
		}

		[Test]
		public void Should_GetResults_BeLazy_WhenMessageAccessedBefore()
		{
			var results = CreateTestResults(10, 20);
			var exception = new PolicyDelegateCollectionException<int>(results);

			_ = exception.Message;
			_ = exception.Message;

			Assert.That(IsLazyValueCreated(exception), Is.False);

			var getResults = exception.GetResults().ToList();

			Assert.That(IsLazyValueCreated(exception), Is.True);
			Assert.That(getResults, Has.Count.EqualTo(2));
			Assert.That(getResults[0], Is.EqualTo(10));
			Assert.That(getResults[1], Is.EqualTo(20));
		}

		[Test]
		public void Should_GetResults_ReturnsIReadOnlyList()
		{
			var results = CreateTestResults(100, 200);
			var exception = new PolicyDelegateCollectionException<int>(results);

			IEnumerable<int> getResults = exception.GetResults();

			Assert.That(getResults, Is.Not.Null);
			Assert.That(getResults, Is.InstanceOf<IReadOnlyList<int>>());
		}

		[Test]
		public void Should_GetResults_CorrectValues_WhenPolicyResultHasNoErrors()
		{
			var polResult = new PolicyResult<int>();
			polResult.SetResult(99);

			var policy = new RetryPolicy(1);
			var policyDelegate = policy.ToPolicyDelegate(() => 99);

			var exception = new PolicyDelegateCollectionException<int>(
				new[] { new PolicyDelegateResult<int>(polResult, policy.PolicyName, policyDelegate.GetMethodInfo()) });

			var getResults = exception.GetResults().ToList();

			Assert.That(getResults, Has.Count.EqualTo(1));
			Assert.That(getResults[0], Is.EqualTo(99));
		}

		[Test]
		public void Should_GetResults_CorrectValues_WhenMultiplePolicyResultHasNoErrors()
		{
			var polResults = new List<PolicyDelegateResult<int>>();
			var policy = new RetryPolicy(1);

			for (int i = 0; i < 4; i++)
			{
				var polResult = new PolicyResult<int>();
				polResult.SetResult(i * 100);
				polResults.Add(
					new PolicyDelegateResult<int>(polResult, policy.PolicyName, policy.ToPolicyDelegate(() => i).GetMethodInfo()));
			}

			var exception = new PolicyDelegateCollectionException<int>(polResults);
			var getResults = exception.GetResults().ToList();

			Assert.That(getResults, Has.Count.EqualTo(4));
			Assert.That(getResults, Is.EqualTo(new[] { 0, 100, 200, 300 }));
		}

		[Test]
		public void Should_LazyNotCreated_BeforeAnyAccess()
		{
			var results = CreateTestResults(1, 2);
			var exception = new PolicyDelegateCollectionException<int>(results);

			Assert.That(IsLazyValueCreated(exception), Is.False);
		}

		[Test]
		public void Should_GetResults_ThreadSafe_BarrierRacingSingleEvaluation()
		{
			var results = CreateTestResults(1, 2, 3, 4);
			var exception = new PolicyDelegateCollectionException<int>(results);

			int threadCount = 16;
			var barrier = new Barrier(threadCount);
			var snapshots = new bool[threadCount];

			var tasks = Enumerable.Range(0, threadCount).Select(t => Task.Run(() =>
			{
				barrier.SignalAndWait();
				snapshots[t] = IsLazyValueCreated(exception);
				_ = exception.GetResults();
			})).ToArray();

			Task.WaitAll(tasks);

			Assert.That(exception.GetResults().ToList(), Has.Count.EqualTo(4));
		}

		private static bool IsLazyValueCreated(PolicyDelegateCollectionException<int> exception)
		{
			var field = typeof(PolicyDelegateCollectionException<int>)
				.GetField("_results", BindingFlags.NonPublic | BindingFlags.Instance);
			var lazy = (Lazy<IReadOnlyList<int>>)field.GetValue(exception);
			return lazy.IsValueCreated;
		}

		private static IReadOnlyList<int> GetInternalResults(PolicyDelegateCollectionException<int> exception)
		{
			var field = typeof(PolicyDelegateCollectionException<int>)
				.GetField("_results", BindingFlags.NonPublic | BindingFlags.Instance);
			var lazy = (Lazy<IReadOnlyList<int>>)field.GetValue(exception);
			return lazy.Value;
		}

		private static List<PolicyDelegateResult<int>> CreateTestResults(params int[] values)
		{
			var policy = new RetryPolicy(1);
			var policyDelegate = policy.ToPolicyDelegate(() => 0);
			var results = new List<PolicyDelegateResult<int>>();

			foreach (var value in values)
			{
				var polResult = new PolicyResult<int>();
				polResult.SetResult(value);
				polResult.AddError(new Exception($"Error{value}"));
				results.Add(new PolicyDelegateResult<int>(polResult, policy.PolicyName, policyDelegate.GetMethodInfo()));
			}

			return results;
		}
	}
}
