using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework.Legacy;
using PoliNorError.Extensions.PolicyResultHandling;

namespace PoliNorError.Tests
{
	internal class PolicyResultHandlerTests
	{
		[Test]
		public void Should_PolicyResult_Handled_By_Handle_Method_If_MiscHandlerAdded()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddHandlerForPolicyResult(async (__, _) => { await Task.Delay(1); i++; })
					   .AddHandlerForPolicyResult(async (_) => { await Task.Delay(1); i++; })
					   .AddHandlerForPolicyResult((__, _) => i++)
					   ;
			retryPolicy.Handle(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(3, i);
		}

		[Test]
		public void Should_HandleResult_If_Token_Canceled()
		{
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddHandlerForPolicyResult(async (__, _) => { await Task.Delay(1); i++; })
					   .AddHandlerForPolicyResult(async (_) => { await Task.Delay(1); i++; });

			var res2 = retryPolicy.Handle(() => throw new Exception("Handle"), cancelSource.Token);
			ClassicAssert.AreEqual(typeof(OperationCanceledException), res2.PolicyResultHandlingErrors.FirstOrDefault().InnerException.GetType());
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_Work_HandleResultAsync_MiscHandlers()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			int m = 0;
			retryPolicy =  retryPolicy.AddHandlerForPolicyResult((__, _) => i++)
									  .AddHandlerForPolicyResult((_) => i++)
									  .AddHandlerForPolicyResult(async (__, _) => { await Task.Delay(1); m++; })
									  .AddHandlerForPolicyResult((__, _) => i++);

			await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });

			ClassicAssert.AreEqual(3, i);
			ClassicAssert.AreEqual(1, m);
		}

		[Test]
		public void Should_HandleResultError_NotAffect_NoError()
		{
			void action(PolicyResult _) { throw new Exception(); }
			var retryPolicy = new RetryPolicy(1).AddHandlerForPolicyResult(action);
			var res = retryPolicy.Handle(() => { });
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.IsTrue(res.PolicyResultHandlingErrors.Count() == 1);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_SyncPolicyResultHandler_Even_If_NoError()
		{
			void action(PolicyResult pr) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddHandlerForPolicyResult(action);
			var res = retryPolicy.Handle(() => { });
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_AsyncPolicyResultHandler_Even_If_NoError()
		{
			async Task func(PolicyResult pr) { await Task.Delay(1); pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddHandlerForPolicyResult(func);
			var res = retryPolicy.Handle(() => { });
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		public void Should_HandleResult_HasError_For_AggregateException()
		{
			var retryPolicy = new RetryPolicy(1).AddHandlerForPolicyResult((_) => Task.FromException(new Exception()));
			var res = retryPolicy.Handle(() => { });
			ClassicAssert.IsTrue(res.PolicyResultHandlingErrors.Count() == 1);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_PolicyResult_Handler_Call_PolicyResult_SetFailed_Method_Even_Token_Have_Already_Canceled_And_The_Handling_Was_Successful(bool sync)
		{
			using (var cts = new CancellationTokenSource())
			{
				PolicyResult res = null;
				var simplePolicy = new SimplePolicy();
				if (sync)
				{
					simplePolicy.AddPolicyResultHandler((pr) => pr.SetFailed());
					res = simplePolicy.Handle(() => cts.Cancel(), cts.Token);
				}
				else
				{
					simplePolicy.AddPolicyResultHandler(async (pr) => { await Task.Delay(1); pr.SetFailed(); });
					res = await simplePolicy.HandleAsync(async (_) => { await Task.Delay(1); cts.Cancel(); }, cts.Token);
				}

				ClassicAssert.IsTrue(res.IsFailed);
			}
		}

		[Test]
		public void Should_PolicyResultHandlerCollection_AddHandlers()
		{
			int handlersCount = 1;

			var handlers = new PolicyResultHandlerCollection();

			handlers.AddHandler(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(handlersCount++, handlers.Handlers.Count);

			handlers.AddHandler(async (_, __) => await Task.Delay(1));
			ClassicAssert.AreEqual(handlersCount++, handlers.Handlers.Count);

			handlers.AddHandler((_) => Expression.Empty());
			ClassicAssert.AreEqual(handlersCount++, handlers.Handlers.Count);

			handlers.AddHandler((_, __) => Expression.Empty());
			ClassicAssert.AreEqual(handlersCount, handlers.Handlers.Count);
		}

		[Test]
		[TestCase(TestPolicyResultHandlerSyncType.Sync, true)]
		[TestCase(TestPolicyResultHandlerSyncType.Misc, true)]
		[TestCase(TestPolicyResultHandlerSyncType.Async, true)]
		[TestCase(TestPolicyResultHandlerSyncType.Sync, false)]
		[TestCase(TestPolicyResultHandlerSyncType.Misc, false)]
		[TestCase(TestPolicyResultHandlerSyncType.Async, false)]
		public async Task Should_More_Than_One_Exception_In_Handler_Be_Stored_In_PolicyResultHandlingErrors(TestPolicyResultHandlerSyncType syncType, bool syncHandling)
		{
			var policy = new SimplePolicy();
			void action(PolicyResult _) => throw new Exception("TestSync");
			Task fn(PolicyResult _) => throw new Exception("TestAsync");
			switch (syncType)
			{
				case TestPolicyResultHandlerSyncType.Sync:
					policy.AddHandlerForPolicyResult(action)
						  .AddHandlerForPolicyResult(action);
					break;
				case TestPolicyResultHandlerSyncType.Misc:
					policy.AddHandlerForPolicyResult(action)
						  .AddHandlerForPolicyResult(fn);
					break;
				case TestPolicyResultHandlerSyncType.Async:
					policy.AddHandlerForPolicyResult(fn)
						  .AddHandlerForPolicyResult(fn);
					break;
			}
			PolicyResult result = null;
			if (syncHandling)
			{
				result = policy.Handle(() => { });
			}
			else
			{
				result = await policy.HandleAsync(async (_) => await Task.Delay(1));
			}
			ClassicAssert.AreEqual(2, result.PolicyResultHandlingErrors.Count());
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_HandlingIndex_For_PolicyResultHandlingException_Be_Correct(bool syncHandling)
		{
			void genericAction(PolicyResult<int> _) => Expression.Empty();
			void action(PolicyResult _) => throw new Exception("TestSync");

			var policy = new SimplePolicy()
						.AddHandlerForPolicyResult<SimplePolicy, int>(genericAction)
						.AddPolicyResultHandler(action);
			PolicyResult result = null;
			if (syncHandling)
			{
				result = policy.Handle(() => { });
			}
			else
			{
				result = await policy.HandleAsync(async (_) => await Task.Delay(1));
			}
			ClassicAssert.AreEqual(1, result.PolicyResultHandlingErrors.Count());
			ClassicAssert.AreEqual(1, result.PolicyResultHandlingErrors.FirstOrDefault().HandlerIndex);
		}
	}
}
