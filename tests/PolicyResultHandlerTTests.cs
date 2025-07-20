using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NUnit.Framework.Legacy;

namespace PoliNorError.Tests
{
	internal class PolicyResultHandlerTTests
	{
		[Test]
		public void Should_PolicyResult_Handled_By_Handle_Method_If_SyncHandlersAdded()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddHandlerForPolicyResult((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddHandlerForPolicyResult((PolicyResult<int> __, CancellationToken _) => i++);

			retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_SyncPolicyResultHandlerT_Even_If_NoError()
		{
			void action(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddHandlerForPolicyResult<RetryPolicy, int>(action);
			var res = retryPolicy.Handle(() =>1);
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_PolicyResult_Handled_By_Handle_And_HandleAsync_Method_If_ASyncHandlersAdded(bool asnc)
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddHandlerForPolicyResult(async (PolicyResult<int> __, CancellationToken _) => { await Task.Delay(1); i++; })
					   .AddHandlerForPolicyResult(async (PolicyResult<int> __) => { await Task.Delay(1); i++; });

			if (asnc)
			{
				await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			}
			else
			{
				retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			}

			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Handled_By_HandleAsync_If_SyncHandler_And_ASyncHandler_Added()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddHandlerForPolicyResult(async (PolicyResult<int> __, CancellationToken _) => { await Task.Delay(1); i++; })
					   .AddHandlerForPolicyResult((PolicyResult<int> __) => i++);

			await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Handled_By_HandleAsync_If_Only_SyncHandlers_Added()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddHandlerForPolicyResult((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddHandlerForPolicyResult((PolicyResult<int> __) => i++);

			await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Can_Be_SetFailed_By_ASyncPolicyResultHandlerT_Even_If_NoError()
		{
			async Task func(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); await Task.Delay(1); }
			var retryPolicy = new RetryPolicy(1).AddHandlerForPolicyResult<RetryPolicy, int>(func);
			var res = await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); return 1; });
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		public void Should_No_PolicyResultHandlingErrors_If_SyncPolicyResultHandler_For_OtherType_Than_PolicyResult_Result()
		{
			var result = PolicyCollection.Create()
					.WithRetry(2)
					.WithFallback(() => new List<string>() { "1", "2" })
					.AddPolicyResultHandlerForAll<List<string>>(_ => { })
					.AddPolicyResultHandlerForAll<int>(_ => { })
					.HandleDelegate<List<string>>(() => throw new Exception("Test"));

			ClassicAssert.AreEqual(0, result.LastPolicyResult.PolicyResultHandlingErrors.Count());
		}

		[Test]
		public async Task Should_No_PolicyResultHandlingErrors_If_ASyncPolicyResultHandler_For_OtherType_Than_PolicyResult_Result()
		{
			var result = await PolicyCollection.Create()
					.WithRetry(2)
					.WithFallback(() => new List<string>() { "1", "2" })
					.AddPolicyResultHandlerForAll<List<string>>(async (_, __) => await Task.Delay(1))
					.AddPolicyResultHandlerForAll<int>(async (_, __) => await Task.Delay(1))
					.HandleDelegateAsync<List<string>>(() => throw new Exception("Test"));

			ClassicAssert.AreEqual(0, result.LastPolicyResult.PolicyResultHandlingErrors.Count());
		}

		[Test]
		public void Should_PolicyResultHandlerCollection_AddHandlers()
		{
			int genericHandlersCount = 1;

			var handlers = new PolicyResultHandlerCollection();
			handlers.AddHandler<int>(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(genericHandlersCount++, handlers.GenericHandlers.Count);

			handlers.AddHandler<int>(async (_, __) => await Task.Delay(1));
			ClassicAssert.AreEqual(genericHandlersCount++, handlers.GenericHandlers.Count);

			handlers.AddHandler<int>((_) => Expression.Empty());
			ClassicAssert.AreEqual(genericHandlersCount++, handlers.GenericHandlers.Count);

			handlers.AddHandler<int>((_, __) => Expression.Empty());
			ClassicAssert.AreEqual(genericHandlersCount, handlers.GenericHandlers.Count);
		}

		[Test]
		public void Should_PolicyResultHandlerCollectionIndex_Be_Correct_When_Adding_NonGeneric_And_Generic_Handlers()
		{
			int counter = 0;
			var handlers = new PolicyResultHandlerCollection();
			handlers.AddHandler<int>(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(counter++, handlers.GenericHandlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(counter++, handlers.Handlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(counter++, handlers.Handlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler<int>(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(counter++, handlers.GenericHandlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler((_) => { });
			ClassicAssert.AreEqual(counter++, handlers.Handlers.LastOrDefault().CollectionIndex);

			var allHandlersIndexes = handlers.GenericHandlers.Select(h => h.CollectionIndex)
									.Concat(handlers.Handlers.Select(h => h.CollectionIndex));

			ClassicAssert.IsTrue(allHandlersIndexes.GroupBy(x => x).All(x => x.Count() == 1));

			ClassicAssert.AreEqual(counter, allHandlersIndexes.Count());
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
			void action(PolicyResult<int> _) => throw new Exception("TestSync");
			Task fn(PolicyResult<int> _) => throw new Exception("TestAsync");
			switch (syncType)
			{
				case TestPolicyResultHandlerSyncType.Sync:
					policy.AddHandlerForPolicyResult<SimplePolicy, int>(action)
						  .AddHandlerForPolicyResult<SimplePolicy, int>(action);
					break;
				case TestPolicyResultHandlerSyncType.Misc:
					policy.AddHandlerForPolicyResult<SimplePolicy, int>(action)
						  .AddHandlerForPolicyResult<SimplePolicy, int>(fn);
					break;
				case TestPolicyResultHandlerSyncType.Async:
					policy.AddHandlerForPolicyResult<SimplePolicy, int>(fn)
						  .AddHandlerForPolicyResult<SimplePolicy, int>(fn);
					break;
			}
			PolicyResult<int> result = null;
			if (syncHandling)
			{
				result = policy.Handle(() => 1);
			}
			else
			{
				result = await policy.HandleAsync(async (_) => { await Task.Delay(1); return 1; });
			}
			ClassicAssert.AreEqual(2, result.PolicyResultHandlingErrors.Count());
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_HandlingIndex_For_PolicyResultHandlingException_Be_Correct(bool syncHandling)
		{
			void action(PolicyResult _) => Expression.Empty();
			void genericAction(PolicyResult<int> _) => throw new Exception("TestSync");

			var policy = new SimplePolicy()
						.AddPolicyResultHandler(action)
						.AddHandlerForPolicyResult<SimplePolicy, int>(genericAction);
			PolicyResult<int> result = null;
			if (syncHandling)
			{
				result = policy.Handle(() => 1);
			}
			else
			{
				result = await policy.HandleAsync(async (_) => {await Task.Delay(1); return 1;});
			}
			ClassicAssert.AreEqual(1, result.PolicyResultHandlingErrors.Count());
			ClassicAssert.AreEqual(1, result.PolicyResultHandlingErrors.FirstOrDefault().HandlerIndex);
		}
	}
}
