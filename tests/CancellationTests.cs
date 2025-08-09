using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class CancellationTests
	{
		[Test]
		[TestCase(PolicyAlias.Fallback, true)]
		[TestCase(PolicyAlias.Fallback, false)]
		[TestCase(PolicyAlias.Retry, true)]
		[TestCase(PolicyAlias.Retry, false)]
		[TestCase(PolicyAlias.Simple, true)]
		[TestCase(PolicyAlias.Simple, false)]
		public async Task Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_HandleAsync_Throws_DueTo_InnerToken(PolicyAlias policyAlias, bool isProcessor)
		{
			PolicyResult result = null;
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				async Task funcToHandle(CancellationToken ct) => await CancelableActions.ActionThatCanceledOnOuterAndThrowOnInner(ct, cancelTokenSource);
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						if (isProcessor)
						{
							var rp = new DefaultRetryProcessor();
							result = await rp.RetryAsync(funcToHandle, 3, cancelTokenSource.Token);
						}
						else
						{
							var rp = new RetryPolicy(3);
							result = await rp.HandleAsync(funcToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Fallback:
						if (isProcessor)
						{
							var rp = new DefaultFallbackProcessor();
							result = await rp.FallbackAsync(funcToHandle, (_) => Task.CompletedTask, cancelTokenSource.Token);
						}
						else
						{
							var rp = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.CompletedTask);
							result = await rp.HandleAsync(funcToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Simple:
						if (isProcessor)
						{
							var sp = new SimplePolicyProcessor();
							result = await sp.ExecuteAsync(funcToHandle, token: cancelTokenSource.Token);
						}
						else
						{
							var sp = new SimplePolicy();
							result = await sp.HandleAsync(funcToHandle, cancelTokenSource.Token);
						}
						break;
					default:
						throw new NotImplementedException();
				}

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.UnprocessedError, Is.Null);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Retry, true)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Retry, true)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Retry, false)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Retry, false)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Fallback, true)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Fallback, true)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Fallback, false)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Fallback, false)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Simple, true)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Simple, true)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Simple, false)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Simple, false)]
		public void Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_Handle_Throws_DueTo_InnerToken(TestCancellationMode cancellationMode, PolicyAlias policyAlias, bool isProcessor)
		{
			PolicyResult result = null;
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Action actionToHandle = null;
				if (cancellationMode == TestCancellationMode.OperationCanceled)
				{
					actionToHandle = () => CancelableActions.SyncActionThatCanceledOnOuterAndThrowOnInner(cancelTokenSource.Token, cancelTokenSource);
				}
				else
				{
					actionToHandle = () => CancelableActions.SyncActionThatCanceledOnOuterAndThrowOnInnerAndThrowAgregateExc(cancelTokenSource.Token, cancelTokenSource);
				}
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						if (isProcessor)
						{
							var rp = new DefaultRetryProcessor();
							result = rp.Retry(actionToHandle, 3, cancelTokenSource.Token);
						}
						else
						{
							var rp = new RetryPolicy(3);
							result = rp.Handle(actionToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Fallback:
						if (isProcessor)
						{
							var rp = new DefaultFallbackProcessor();
							result = rp.Fallback(actionToHandle, (_) => { }, cancelTokenSource.Token);
						}
						else
						{
							var rp = new FallbackPolicy().WithFallbackAction((_) => { });
							result = rp.Handle(actionToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Simple:
						if (isProcessor)
						{
							var sp = new SimplePolicyProcessor();
							result = sp.Execute(actionToHandle, cancelTokenSource.Token);
						}
						else
						{
							var rp = new SimplePolicy();
							result = rp.Handle(actionToHandle, cancelTokenSource.Token);
						}
						break;
					default:
						throw new NotImplementedException();
				}
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.UnprocessedError, Is.Null);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Retry, true)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Retry, true)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Retry, false)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Retry, false)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Fallback, true)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Fallback, true)]
		[TestCase(TestCancellationMode.OperationCanceled, PolicyAlias.Fallback, false)]
		[TestCase(TestCancellationMode.Aggregate, PolicyAlias.Fallback, false)]
		public void Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_GenericHandle_Throws_DueTo_InnerToken(TestCancellationMode cancellationMode, PolicyAlias policyAlias, bool isProcessor)
		{
			PolicyResult result = null;
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Func<int> actionToHandle = null;
				if (cancellationMode == TestCancellationMode.OperationCanceled)
				{
					actionToHandle = () => CancelableActions.GenericSyncActionThatCanceledOnOuterAndThrowOnInner(cancelTokenSource.Token, cancelTokenSource);
				}
				else
				{
					actionToHandle = () => CancelableActions.GenericSyncActionThatCanceledOnOuterAndThrowOnInnerAndThrowAgregateExc(cancelTokenSource.Token, cancelTokenSource);
				}
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						if (isProcessor)
						{
							var rp = new DefaultRetryProcessor();
							result = rp.Retry(actionToHandle, 3, cancelTokenSource.Token);
						}
						else
						{
							var rp = new RetryPolicy(3);
							result = rp.Handle(actionToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Fallback:
						if (isProcessor)
						{
							var rp = new DefaultFallbackProcessor();
							result = rp.Fallback(actionToHandle, (_) => 1, cancelTokenSource.Token);
						}
						else
						{
							var rp = new FallbackPolicy().WithFallbackFunc((_) => 1);
							result = rp.Handle(actionToHandle, cancelTokenSource.Token);
						}
						break;
					default:
						throw new NotImplementedException();
				}
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.UnprocessedError, Is.Null);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		[TestCase(PolicyAlias.Retry, true)]
		[TestCase(PolicyAlias.Retry, false)]
		[TestCase(PolicyAlias.Fallback, true)]
		[TestCase(PolicyAlias.Fallback, false)]
		[TestCase(PolicyAlias.Simple, true)]
		[TestCase(PolicyAlias.Simple, false)]
		public async Task Should_Have_IsCancel_True_When_OuterSource_IsCanceled_And_GenericHandleAsync_Throws_DueTo_InnerToken(PolicyAlias policyAlias, bool isProcessor)
		{
			PolicyResult result = null;
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				async Task<int> funcToHandle(CancellationToken ct) => await CancelableActions.GenericActionThatCanceledOnOuterAndThrowOnInner(ct, cancelTokenSource);
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						if (isProcessor)
						{
							var rp = new DefaultRetryProcessor();
							result = await rp.RetryAsync(funcToHandle, 3, cancelTokenSource.Token);
						}
						else
						{
							var rp = new RetryPolicy(3);
							result = await rp.HandleAsync(funcToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Fallback:
						if (isProcessor)
						{
							var rp = new DefaultFallbackProcessor();
							result = await rp.FallbackAsync(funcToHandle, (_) => Task.FromResult(1), cancelTokenSource.Token);
						}
						else
						{
							var rp = new FallbackPolicy().WithAsyncFallbackFunc((_) => Task.FromResult(1));
							result = await rp.HandleAsync(funcToHandle, cancelTokenSource.Token);
						}
						break;
					case PolicyAlias.Simple:
						if (isProcessor)
						{
							var sp = new SimplePolicyProcessor();
							result = await sp.ExecuteAsync(funcToHandle, token: cancelTokenSource.Token);
						}
						else
						{
							var rp = new SimplePolicy();
							result = await rp.HandleAsync(funcToHandle, cancelTokenSource.Token);
						}
						break;
					default:
						throw new NotImplementedException();
				}
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.UnprocessedError, Is.Null);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		public enum TestCancellationMode
		{
			OperationCanceled,
			Aggregate
		}
	}
}
