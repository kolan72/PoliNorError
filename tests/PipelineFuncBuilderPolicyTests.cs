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
		public void Should_AddFunc_WithCustomPolicy_UseProvidedPolicy()
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

			var retryPolicy = new RetryPolicy(5); // 5 retries

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func0)
				.AddFunc(func1, retryPolicy)
				.Build();

			var result = pipeline(10, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(2));
			Assert.That(callCount, Is.EqualTo(2)); // Initial + 1 retry
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
	}
}
