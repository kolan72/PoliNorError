using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class SimplePolicyProcessorCreateFailingOnErrorTests
	{
		[Test]
		public void Should_Fail_PolicyResult_When_ActionWithParam_Throws_For_FailingOnError_Processor()
		{
			Action<int> throwingAction = _ => throw new InvalidOperationException();

			var result = SimplePolicyProcessor.CreateFailingOnError().Execute(throwingAction, 1);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public void Should_Fail_PolicyResult_When_Action_Throws_For_FailingOnError_Processor()
		{
			Action throwingAction = () => throw new InvalidOperationException();

			var result = SimplePolicyProcessor.CreateFailingOnError().Execute(throwingAction);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public void Should_Fail_PolicyResult_When_FuncWithParam_Throws_For_FailingOnError_Processor()
		{
			Func<int, int> throwingFunc = _ => throw new InvalidOperationException();

			var result = SimplePolicyProcessor.CreateFailingOnError().Execute(throwingFunc, 1);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public void Should_Fail_PolicyResult_When_Func_Throws_For_FailingOnError_Processor()
		{
			Func<int> throwingFunc = () => throw new InvalidOperationException();

			var result = SimplePolicyProcessor.CreateFailingOnError().Execute(throwingFunc);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public async Task Should_Fail_PolicyResult_When_AsyncActionWithParam_Throws_For_FailingOnError_Processor()
		{
			Func<int, CancellationToken, Task> throwingFunc = async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); };

			var result = await SimplePolicyProcessor.CreateFailingOnError().ExecuteAsync(throwingFunc, 1, CancellationToken.None);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public async Task Should_Fail_PolicyResult_When_AsyncAction_Throws_For_FailingOnError_Processor()
		{
			Func<CancellationToken, Task> throwingFunc = async _ => { await Task.Delay(1); throw new InvalidOperationException(); };

			var result = await SimplePolicyProcessor.CreateFailingOnError().ExecuteAsync(throwingFunc);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public async Task Should_Fail_PolicyResult_When_AsyncFuncWithParam_Throws_For_FailingOnError_Processor()
		{
			Func<int, CancellationToken, Task<int>> throwingFunc = async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); };

			var result = await SimplePolicyProcessor.CreateFailingOnError().ExecuteAsync<int, int>(throwingFunc, 1, CancellationToken.None);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public async Task Should_Fail_PolicyResult_When_AsyncFunc_Throws_For_FailingOnError_Processor()
		{
			Func<CancellationToken, Task<int>> throwingFunc = async _ => { await Task.Delay(1); throw new InvalidOperationException(); };

			var result = await SimplePolicyProcessor.CreateFailingOnError().ExecuteAsync(throwingFunc);

			AssertFailedByProcessedError(result);
		}

		[Test]
		public void Should_Keep_PolicyResult_Successful_When_Nothing_Throws_For_FailingOnError_Processor()
		{
			Action action = () => { };

			var result = SimplePolicyProcessor.CreateFailingOnError().Execute(action);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));
		}

		[Test]
		public async Task Should_Keep_AsyncPolicyResult_Successful_When_Nothing_Throws_For_FailingOnError_Processor()
		{
			Func<CancellationToken, Task> func = async _ => await Task.Delay(1);

			var result = await SimplePolicyProcessor.CreateFailingOnError().ExecuteAsync(func);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.NoError, Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));
		}

		[Test]
		public void Should_Leave_PolicyResult_Not_Failed_When_Action_Throws_For_CreateDefault_Processor()
		{
			Action throwingAction = () => throw new InvalidOperationException();

			var defaultResult = SimplePolicyProcessor.CreateDefault().Execute(throwingAction);
			var failingResult = SimplePolicyProcessor.CreateFailingOnError().Execute(throwingAction);

			Assert.That(defaultResult.IsFailed, Is.False);
			Assert.That(defaultResult.IsSuccess, Is.True);
			Assert.That(defaultResult.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));

			Assert.That(failingResult.IsFailed, Is.True);
			Assert.That(failingResult.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
		}

		[Test]
		public void Should_Run_ErrorProcessor_And_Fail_PolicyResult_For_FailingOnError_Processor()
		{
			var processedCount = 0;
			var processor = SimplePolicyProcessor.CreateFailingOnError()
						.WithErrorProcessorOf((Exception _, CancellationToken __) => { processedCount++; });

			var result = processor.Execute(() => throw new InvalidOperationException());

			Assert.That(processedCount, Is.EqualTo(1));
			AssertFailedByProcessedError(result);
		}

		private static void AssertFailedByProcessedError(PolicyResult result)
		{
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.IsSuccess, Is.False);
			Assert.That(result.IsCanceled, Is.False);
			Assert.That(result.NoError, Is.False);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
			Assert.That(result.Errors.Count(), Is.EqualTo(1));
			Assert.That(result.UnprocessedError, Is.TypeOf<InvalidOperationException>());
		}
	}
}
