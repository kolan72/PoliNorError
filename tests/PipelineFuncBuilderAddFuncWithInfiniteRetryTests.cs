using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class PipelineFuncBuilderAddFuncWithInfiniteRetryTests
	{
		[Test]
		public void Should_AddStepWithInfiniteRetryPolicy_WhenCalledWithFunction()
		{
			// Arrange
			int func1(string s) => s.Length;
			int func2(int i) => i * 2;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
		}

		[Test]
		public void Should_RetryUntilSuccess_WhenAddedStepFailsInitially()
		{
			// Arrange
			int callCount = 0;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				if (callCount < 5)
				{
					throw new InvalidOperationException($"Attempt {callCount} failed");
				}
				return i * 2;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(callCount, Is.EqualTo(5));
		}

		[Test]
		public void Should_RetryMultipleTimes_WhenAddedStepKeepsFailing()
		{
			// Arrange
			int callCount = 0;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				if (callCount < 10)
				{
					throw new InvalidOperationException($"Attempt {callCount} failed");
				}
				return i * 3;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(15));
			Assert.That(callCount, Is.EqualTo(10));
		}

		[Test]
		public void Should_AcceptRetryDelay_WhenProvided()
		{
			// Arrange
			int func1(string s) => s.Length;
			int func2(int i) => i * 2;
			var retryDelay = new LinearRetryDelay(TimeSpan.FromMilliseconds(10));

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2, retryDelay)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
		}

		[Test]
		public void Should_WorkWithoutRetryDelay_WhenNotProvided()
		{
			// Arrange
			int func1(string s) => s.Length;
			int func2(int i) => i * 2;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2, retryDelay: null)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
		}

		[Test]
		public void Should_ApplyRetryDelay_WhenAddedStepFailsAndDelayProvided()
		{
			// Arrange
			int callCount = 0;
			var startTime = DateTime.UtcNow;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}

			var retryDelay = new LinearRetryDelay(TimeSpan.FromMilliseconds(50));

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2, retryDelay)
				.Build();

			var result = pipeline("test", CancellationToken.None);
			var elapsed = DateTime.UtcNow - startTime;

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(callCount, Is.EqualTo(3));
			Assert.That(elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(90));
		}

		[Test]
		public void Should_ChainMultipleSteps_WhenAddingAfterInfiniteRetryStep()
		{
			// Arrange
			int callCount = 0;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}
			string func3(int i) => $"Result: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.AddFunc(func3)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("Result: 8"));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_WorkWithDifferentTypes_WhenTransformingData()
		{
			// Arrange
			int callCount = 0;
			int func1(string s) => s.Length;
			string func2(int i)
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
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("5"));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_WorkWithComplexTypes_WhenProcessingData()
		{
			// Arrange
			int callCount = 0;
			int func1(string s) => s.Length;
			System.Collections.Generic.List<int> func2(int i)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return new System.Collections.Generic.List<int> { i, i * 2, i * 3 };
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result.Count, Is.EqualTo(3));
			Assert.That(result.Result[0], Is.EqualTo(4));
			Assert.That(result.Result[1], Is.EqualTo(8));
			Assert.That(result.Result[2], Is.EqualTo(12));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_PassCancellationToken_WhenExecutingPipeline()
		{
			// Arrange
			int func1(string s) => s.Length;
			int func2(int i) => i * 2;

			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				// Act
				var result = pipeline("test", cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Result, Is.EqualTo(8));
			}
		}

		[Test]
		public void Should_WorkWithOnError_WhenErrorHandlerAdded()
		{
			// Arrange
			int callCount = 0;
			Exception capturedException = null;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.OnError((ex, _) => capturedException = ex)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(callCount, Is.EqualTo(3));
			Assert.That(capturedException, Is.Not.Null);
			Assert.That(capturedException, Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Should_ExecuteMultipleTimes_WhenPipelineCalledRepeatedly()
		{
			// Arrange
			int totalCalls = 0;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				totalCalls++;
				if (totalCalls % 3 != 0)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}

			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			// Act
			var result1 = pipeline("test", CancellationToken.None);
			var result2 = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result1.IsFailed, Is.False);
			Assert.That(result1.Result, Is.EqualTo(8));
			Assert.That(result2.IsFailed, Is.False);
			Assert.That(result2.Result, Is.EqualTo(10));
			Assert.That(totalCalls, Is.EqualTo(6));
		}

		[Test]
		public void Should_PreserveInputValue_ThroughRetries()
		{
			// Arrange
			int callCount = 0;
			int capturedInput = 0;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				capturedInput = i;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(capturedInput, Is.EqualTo(4));
			Assert.That(callCount, Is.EqualTo(3));
		}

		[Test]
		public void Should_RetryWithExponentialDelay_WhenExponentialRetryDelayProvided()
		{
			// Arrange
			int callCount = 0;
			var startTime = DateTime.UtcNow;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				if (callCount < 3)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}

			var retryDelay = new ExponentialRetryDelay(TimeSpan.FromMilliseconds(10));

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2, retryDelay)
				.Build();

			var result = pipeline("test", CancellationToken.None);
			var elapsed = DateTime.UtcNow - startTime;

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(callCount, Is.EqualTo(3));
			Assert.That(elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(25));
		}

		[Test]
		public void Should_WorkWithValueTypes_WhenProcessingPrimitives()
		{
			// Arrange
			int callCount = 0;
			int func1(int i) => i + 10;
			double func2(int i)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 1.5;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<int, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(22.5));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_WorkWithReferenceTypes_WhenProcessingObjects()
		{
			// Arrange
			int callCount = 0;
			string func1(string s) => s.ToUpper();
			System.Text.StringBuilder func2(string s)
			{
				callCount++;
				if (callCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return new System.Text.StringBuilder(s);
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, string>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result.ToString(), Is.EqualTo("TEST"));
			Assert.That(callCount, Is.EqualTo(2));
		}

		[Test]
		public void Should_ChainAfterOtherPolicySteps_WhenBuildingComplexPipeline()
		{
			// Arrange
			int retryCallCount = 0;
			int infiniteRetryCallCount = 0;

			int func1(string s) => s.Length;
			int func2(int i)
			{
				retryCallCount++;
				if (retryCallCount < 2)
				{
					throw new InvalidOperationException("Retry me");
				}
				return i * 2;
			}
			int func3(int i)
			{
				infiniteRetryCallCount++;
				if (infiniteRetryCallCount < 3)
				{
					throw new InvalidOperationException("Retry me more");
				}
				return i + 10;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithRetry(func2, retryCount: 3)
				.AddFuncWithInfiniteRetry(func3)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(18));
			Assert.That(retryCallCount, Is.EqualTo(2));
			Assert.That(infiniteRetryCallCount, Is.EqualTo(3));
		}

		[Test]
		public void Should_ReturnCorrectType_WhenTransformingThroughMultipleSteps()
		{
			// Arrange
			int func1(string s) => s.Length;
			string func2(int i) => i.ToString();
			bool func3(string s) => s.Length > 0;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.AddFunc(func3)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.True);
		}

		[Test]
		public void Should_HandleFirstStepSuccess_WhenNoRetriesNeeded()
		{
			// Arrange
			int callCount = 0;
			int func1(string s) => s.Length;
			int func2(int i)
			{
				callCount++;
				return i * 2;
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith<string, int>(func1)
				.AddFuncWithInfiniteRetry(func2)
				.Build();

			var result = pipeline("test", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(8));
			Assert.That(callCount, Is.EqualTo(1));
		}
	}
}
