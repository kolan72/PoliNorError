using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class PipelineFuncBuilderPolicyTests
	{
		[Test]
		public void Should_AddFunc_WithRetryPolicy_RetryOnFailure()
		{
			// Arrange
			int callCount = 0;

			string func0(int i) => i.ToString();

			int func1(string s)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Simulated failure");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func0)
				.AddFuncWithRetry(func1, 5)
				.Build();

			var result = pipeline(10, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(2));
			Assert.That(callCount, Is.EqualTo(2)); // Initial + 1 retry
		}

		[Test]
		public void Should_AddFunc_WithFallbackPolicy_UseFallbackOnFailure()
		{
			// Arrange

			string func0(int i) => i.ToString();

			int func1(string _) => throw new InvalidOperationException("Always fails");
			int fallback() => 42;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func0)
				.AddFuncWithFallback(func1, fallback)
				.Build();

			var result = pipeline(1, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(42));
		}

		[Test]
		public void Should_UseFallbackWithCancellationToken_WhenTokenIsNotCanceled()
		{
			// Arrange
			bool tokenWasCanceled = false;
			int func1(string s) => s.Length;
			int func2(int _) => throw new InvalidOperationException("Main failed");
			int fallback(CancellationToken ct)
			{
				tokenWasCanceled = ct.IsCancellationRequested;
				return 555;
			}

			using (var cts = new CancellationTokenSource())
			{
				// Act
				var pipeline = PipelineFuncBuilder
					.StartWith<string, int>(func1)
					.AddFuncWithFallback(func2, fallback)
					.Build();

				var result = pipeline("test", cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Result, Is.EqualTo(555));
				Assert.That(tokenWasCanceled, Is.False);
			}
		}

		[Test]
		public void Should_PassCancellationTokenToFallback_WhenFallbackIsInvoked()
		{
			// Arrange
			CancellationToken capturedToken = default;
			int func1(string s) => s.Length;
			int func2(int _) => throw new InvalidOperationException("Main function failed");
			int fallback(CancellationToken ct)
			{
				capturedToken = ct;
				return 100;
			}

			using (var cts = new CancellationTokenSource())
			{
				// Act
				var pipeline = PipelineFuncBuilder
					.StartWith<string, int>(func1)
					.AddFuncWithFallback(func2, fallback)
					.Build();

				var result = pipeline("test", cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Result, Is.EqualTo(100));
				Assert.That(capturedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_WorkWithDifferentTypes_WhenTransformingData()
		{
			// Arrange
			int func1(string s) => s.Length;
			string func2(int _) => throw new InvalidOperationException("Main failed");
			string fallback(CancellationToken _) => "fallback-value";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithFallback(func2, fallback)
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("fallback-value"));
		}

		[Test]
		public void Should_NotInvokeFallback_WhenMainFunctionSucceeds()
		{
			// Arrange
			bool fallbackCalled = false;
			int func1(string s) => s.Length;
			int func2(int i) => i * 2;
			int fallback(CancellationToken _)
			{
				fallbackCalled = true;
				return -1;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithFallback(func2, fallback)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(fallbackCalled, Is.False);
		}

		[Test]
		public void Should_StartWithRetry_ApplyRetryToFirstStep()
		{
			// Arrange
			int callCount = 0;
			int func1(string s)
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Simulated failure");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithRetry((Func<string, int>)func1, retryCount: 5)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(3)); // Initial + 2 retries
		}

		[Test]
		public void Should_StartWithFallback_ApplyFallbackToFirstStep()
		{
			// Arrange
			int func1(string _) => throw new InvalidOperationException("Always fails");
			int fallback() => 99;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithFallback((Func<string, int>)func1, fallback)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(99));
		}

		[Test]
		public void Should_MixPolicies_InSamePipeline()
		{
			// Arrange
			int retryCount = 0;
			int func1(string s)
			{
				retryCount++;
				if (retryCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return s.Length;
			}

			int func2(int _) => throw new InvalidOperationException("Use fallback");
			int fallback() => 100;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithRetry((Func<string, int>)func1, retryCount: 3)
				.AddFuncWithFallback(func2, fallback)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(100)); // Fallback value
			Assert.That(retryCount, Is.EqualTo(2)); // func1 retried once
		}

		[Test]
		public void Should_BackwardCompatible_WithExistingCode()
		{
			// Arrange - This is how existing code works
			int func1(string s) => s.Length;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.AddFunc(x => x * 2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
		}

		#region PolicyName Tests

		[Test]
		public void Should_StartWith_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "StartPolicy";
			string func1(int i) => i.ToString();

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1, polName)
				.Build();

			var result = pipeline(3, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("3"));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWith_UsePolicyTypeName_WhenPolicyNameIsNotProvided()
		{
			// Arrange
			string func1(int i) => i.ToString();

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.Build();

			var result = pipeline(3, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo("SimplePolicy"));
		}

		[Test]
		public void Should_StartWithRetry_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "NamedRetryPolicy";
			int callCount = 0;
			int func1(string s)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Simulated failure");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithRetry((Func<string, int>)func1, retryCount: 5, retryDelay: null, policyName: polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(2)); // Initial + 1 retry
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWithRetry_UsePolicyTypeName_WhenPolicyNameIsNotProvided()
		{
			// Arrange
			int func1(string s) => s.Length;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithRetry((Func<string, int>)func1, retryCount: 3)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo("RetryPolicy"));
		}

		[Test]
		public void Should_StartWithInfiniteRetry_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "NamedInfiniteRetryPolicy";
			int callCount = 0;
			int func1(string s)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Simulated failure");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func1, retryDelay: null, policyName: polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(2)); // Initial + 1 retry
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWithFallback_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "NamedFallbackPolicy";
			int func1(string _) => throw new InvalidOperationException("Always fails");
			int fallback() => 42;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithFallback((Func<string, int>)func1, fallback, polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(42));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWithFallback_WithCancellationToken_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "NamedCtFallbackPolicy";
			int func1(string _) => throw new InvalidOperationException("Always fails");
			int fallback(CancellationToken ct)
			{
				Assert.That(ct.IsCancellationRequested, Is.False);
				return 555;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithFallback((Func<string, int>)func1, fallback, polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(555));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWith_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenFunctionThrows()
		{
			// Arrange
			const string polName = "StartFailPolicy";
			var expected = new InvalidOperationException("boom");
			int func1(string _) => throw expected;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1, polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWithRetry_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenRetriesAreExhausted()
		{
			// Arrange
			const string polName = "RetryFailPolicy";
			var expected = new InvalidOperationException("boom");
			int func1(string _) => throw expected;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithRetry((Func<string, int>)func1, retryCount: 2, retryDelay: null, policyName: polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWithInfiniteRetry_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenTokenIsAlreadyCanceled()
		{
			// Arrange
			const string polName = "InfiniteRetryFailPolicy";
			int func1(string _) => throw new InvalidOperationException("boom");
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func1, retryDelay: null, policyName: polName)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				// Act
				var result = pipeline("test", cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.True);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.Result, Is.EqualTo(default(int)));
				Assert.That(result.FailedPolicyResult, Is.Not.Null);
				Assert.That(result.FailedPolicyResult.IsCanceled, Is.True);
				Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
			}
		}

		[Test]
		public void Should_StartWithFallback_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenFallbackThrows()
		{
			// Arrange
			const string polName = "FallbackFailPolicy";
			int func1(string _) => throw new InvalidOperationException("Main fails");
			int fallback() => throw new InvalidOperationException("Fallback fails");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithFallback((Func<string, int>)func1, fallback, polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_StartWithFallback_WithCancellationToken_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenFallbackThrows()
		{
			// Arrange
			const string polName = "CtFallbackFailPolicy";
			int func1(string _) => throw new InvalidOperationException("Main fails");
			int fallback(CancellationToken _) => throw new InvalidOperationException("Fallback fails");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithFallback((Func<string, int>)func1, fallback, polName)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFunc_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "AddFuncPolicy";
			string func1(int i) => i.ToString();
			string func2(string s) => $"len:{s.Length}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFunc(func2, polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("len:1"));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithRetry_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "AddRetryPolicy";
			int callCount = 0;
			string func1(int i) => i.ToString();
#pragma warning disable S4144 // Methods should not have identical implementations
			int func2(string s)
#pragma warning restore S4144 // Methods should not have identical implementations
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Simulated failure");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithRetry(func2, retryCount: 5, retryDelay: null, policyName: polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(1));
			Assert.That(callCount, Is.EqualTo(2)); // Initial + 1 retry
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithInfiniteRetry_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "AddInfiniteRetryPolicy";
			int callCount = 0;
			string func1(int i) => i.ToString();
#pragma warning disable S4144 // Methods should not have identical implementations
			int func2(string s)
#pragma warning restore S4144 // Methods should not have identical implementations
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Simulated failure");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithInfiniteRetry(func2, retryDelay: null, policyName: polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(1));
			Assert.That(callCount, Is.EqualTo(3)); // Initial + 2 retries
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithFallback_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "AddFallbackPolicy";
			string func1(int i) => i.ToString();
			int func2(string _) => throw new InvalidOperationException("Always fails");
			int fallback() => 42;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithFallback(func2, fallback, polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(42));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithFallback_WithCancellationToken_SetPolicyName_WhenPolicyNameIsProvided()
		{
			// Arrange
			const string polName = "AddCtFallbackPolicy";
			string func1(int i) => i.ToString();
			int func2(string _) => throw new InvalidOperationException("Always fails");
			int fallback(CancellationToken ct)
			{
				Assert.That(ct.IsCancellationRequested, Is.False);
				return 555;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithFallback(func2, fallback, polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(555));
			Assert.That(result.SucceededPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFunc_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenFunctionThrows()
		{
			// Arrange
			const string polName = "AddFuncFailPolicy";
			string func1(int i) => i.ToString();
			int func2(string _) => throw new InvalidOperationException("boom");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFunc(func2, polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithRetry_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenRetriesAreExhausted()
		{
			// Arrange
			const string polName = "AddRetryFailPolicy";
			string func1(int i) => i.ToString();
			int func2(string _) => throw new InvalidOperationException("boom");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithRetry(func2, retryCount: 2, retryDelay: null, policyName: polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithInfiniteRetry_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenTokenIsCanceled()
		{
			// Arrange
			const string polName = "AddInfiniteRetryFailPolicy";
			using (var cts = new CancellationTokenSource())
			{
				string func1(int i)
				{
					// Cancel the token so the infinite retry step aborts
					cts.Cancel();
					return i.ToString();
				}
				int func2(string _) => throw new InvalidOperationException("boom");

				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, string>)func1)
					.AddFuncWithInfiniteRetry(func2, retryDelay: null, policyName: polName)
					.Build();

				// Act
				var result = pipeline(5, cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.True);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.Result, Is.EqualTo(default(int)));
				Assert.That(result.FailedPolicyResult, Is.Not.Null);
				Assert.That(result.FailedPolicyResult.IsCanceled, Is.True);
				Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
			}
		}

		[Test]
		public void Should_AddFuncWithFallback_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenFallbackThrows()
		{
			// Arrange
			const string polName = "AddFallbackFailPolicy";
			string func1(int i) => i.ToString();
			int func2(string _) => throw new InvalidOperationException("Main fails");
			int fallback() => throw new InvalidOperationException("Fallback fails");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithFallback(func2, fallback, polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		[Test]
		public void Should_AddFuncWithFallback_WithCancellationToken_SetFailedPolicyName_WhenPolicyNameIsProvided_WhenFallbackThrows()
		{
			// Arrange
			const string polName = "AddCtFallbackFailPolicy";
			string func1(int i) => i.ToString();
			int func2(string _) => throw new InvalidOperationException("Main fails");
			int fallback(CancellationToken _) => throw new InvalidOperationException("Fallback fails");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.AddFuncWithFallback(func2, fallback, polName)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.Result, Is.EqualTo(default(int)));
			Assert.That(result.FailedPolicyResult, Is.Not.Null);
			Assert.That(result.FailedPolicyResult.NoError, Is.False);
			Assert.That(result.FailedPolicyResult.PolicyName, Is.EqualTo(polName));
		}

		#endregion
	}
}
