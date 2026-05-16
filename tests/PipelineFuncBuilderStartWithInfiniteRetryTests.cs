using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class PipelineFuncBuilderStartWithInfiniteRetryTests
	{
		[Test]
		public void Should_ExecuteSuccessfully_WhenFunctionDoesNotThrow()
		{
			// Arrange
			int func(string s) => s.Length;
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			// Act
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		[Test]
		public void Should_RetryUntilSuccess_WhenFunctionFailsInitially()
		{
			// Arrange
			int callCount = 0;
			int func(string s)
			{
				callCount++;
				if (callCount < 5)
				{
					throw new InvalidOperationException($"Attempt {callCount} failed");
				}
				return s.Length;
			}

			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			// Act
			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(5)); // Initial + 4 retries
		}

		[Test]
		public void Should_RetryMultipleTimes_WhenFunctionKeepsFailing()
		{
			// Arrange
			int callCount = 0;
			int func(string s)
			{
				callCount++;
				if (callCount < 10)
				{
					throw new InvalidOperationException($"Attempt {callCount} failed");
				}
				return s.Length;
			}

			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			// Act
			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(10)); // Initial + 9 retries
		}

		[Test]
		public void Should_AcceptRetryDelay_WhenProvided()
		{
			// Arrange
			int func(string s) => s.Length;
			var retryDelay = new LinearRetryDelay(TimeSpan.FromMilliseconds(10));

			// Act
			var builder = PipelineFuncBuilder.StartWithInfiniteRetry((Func<string, int>)func, retryDelay);
			var pipeline = builder.Build();
			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
		}

		[Test]
		public void Should_WorkWithoutRetryDelay_WhenNotProvided()
		{
			// Arrange
			int func(string s) => s.Length;

			// Act
			var builder = PipelineFuncBuilder.StartWithInfiniteRetry((Func<string, int>)func, retryDelay: null);
			var pipeline = builder.Build();
			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
		}

		[Test]
		public void Should_ApplyRetryDelay_WhenFunctionFailsAndDelayProvided()
		{
			// Arrange
			int callCount = 0;
			var startTime = DateTime.UtcNow;
			int func(string s)
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return s.Length;
			}

			var retryDelay = new LinearRetryDelay(TimeSpan.FromMilliseconds(50));
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func, retryDelay)
				.Build();

			// Act
			var result = pipeline("test", CancellationToken.None);
			var elapsed = DateTime.UtcNow - startTime;

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(3));
			// Should have at least 2 delays of 50ms each (100ms total)
			Assert.That(elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(90));
		}

		[Test]
		public void Should_ChainWithAddFunc_WhenBuildingPipeline()
		{
			// Arrange
			int callCount = 0;
			int func1(string s)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return s.Length;
			}

			string func2(int i) => $"Length: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func1)
				.AddFunc(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("Length: 4"));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_WorkWithDifferentInputTypes()
		{
			// Arrange
			int callCount = 0;
			string func(int i)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i.ToString();
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<int, string>)func)
				.Build();

			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("42"));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_WorkWithComplexTypes()
		{
			// Arrange
			int callCount = 0;
			int func(System.Collections.Generic.List<string> list)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return list.Count;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<System.Collections.Generic.List<string>, int>)func)
				.Build();

			var input = new System.Collections.Generic.List<string> { "a", "b", "c" };
			var result = pipeline(input, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(3));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_PassCancellationToken_WhenExecutingPipeline()
		{
			// Arrange
			int func(string s) => s.Length;
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				// Act
				var result = pipeline("test", cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Result, Is.EqualTo(4));
			}
		}

		[Test]
		public void Should_HandleNullInput_WhenFunctionAcceptsNull()
		{
			// Arrange
			int callCount = 0;
			int func(string s)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return s?.Length ?? 0;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			var result = pipeline(null, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(0));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_WorkWithOnError_WhenErrorHandlerAdded()
		{
			// Arrange
			int callCount = 0;
			Exception capturedException = null;
			int func(string s)
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.OnError((ex, _) => capturedException = ex)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(3));
			Assert.That(capturedException, Is.Not.Null);
			Assert.That(capturedException, Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_ExecuteMultipleTimes_WhenCalledRepeatedly()
		{
			// Arrange
			int totalCalls = 0;
			int func(string s)
			{
				totalCalls++;
				if (totalCalls % 3 != 0) // Fail 2 out of 3 times
				{
					throw new InvalidOperationException("Retry me");
				}
				return s.Length;
			}

			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			// Act
			var result1 = pipeline("test", CancellationToken.None);
			var result2 = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result1.IsFailed, Is.False);
			Assert.That(result1.Result, Is.EqualTo(4));
			Assert.That(result2.IsFailed, Is.False);
			Assert.That(result2.Result, Is.EqualTo(5));
			Assert.That(totalCalls, Is.EqualTo(6)); // 3 calls for each execution
		}

		[Test]
		public void Should_PreserveInputValue_ThroughRetries()
		{
			// Arrange
			int callCount = 0;
			string capturedInput = null;
			int func(string s)
			{
				callCount++;
				capturedInput = s;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return s.Length;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWithInfiniteRetry((Func<string, int>)func)
				.Build();

			var result = pipeline("test-input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(10));
			Assert.That(capturedInput, Is.EqualTo("test-input"));
			Assert.That(callCount, Is.EqualTo(3));
		}
	}
}
