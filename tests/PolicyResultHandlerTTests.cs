using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace PoliNorError.Tests
{
	internal class PolicyResultHandlerTTests
	{
		[Test]
		public void Should_PolicyResult_Handled_By_Handle_Method_If_SyncHandlersAdded()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddPolicyResultHandlerInner((PolicyResult<int> __, CancellationToken _) => i++);

			retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_SyncPolicyResultHandlerT_Even_If_NoError()
		{
			void action(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandlerInner<RetryPolicy, int>(action);
			var res = retryPolicy.Handle(() =>1);
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_PolicyResult_Handled_By_Handle_And_HandleAsync_Method_If_ASyncHandlersAdded(bool asnc)
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner(async (PolicyResult<int> __, CancellationToken _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandlerInner(async (PolicyResult<int> __) => { await Task.Delay(1); i++; });

			if (asnc)
			{
				await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			}
			else
			{
				retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			}

			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Handled_By_HandleAsync_If_SyncHandler_And_ASyncHandler_Added()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner(async (PolicyResult<int> __, CancellationToken _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandlerInner((PolicyResult<int> __) => i++);

			await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Handled_By_HandleAsync_If_Only_SyncHandlers_Added()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddPolicyResultHandlerInner((PolicyResult<int> __) => i++);

			await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Can_Be_SetFailed_By_ASyncPolicyResultHandlerT_Even_If_NoError()
		{
			async Task func(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); await Task.Delay(1); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandlerInner<RetryPolicy, int>(func);
			var res = await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); return 1; });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
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

			Assert.AreEqual(0, result.LastPolicyResult.PolicyResultHandlingErrors.Count());
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

			Assert.AreEqual(0, result.LastPolicyResult.PolicyResultHandlingErrors.Count());
		}

		[Test]
		public void Should_PolicyResultHandlerCollection_AddHandlers()
		{
			int genericHandlersCount = 1;

			var handlers = new PolicyResultHandlerCollection();
			handlers.AddHandler<int>(async (_) => await Task.Delay(1));
			Assert.AreEqual(genericHandlersCount++, handlers.GenericHandlers.Count);

			handlers.AddHandler<int>(async (_, __) => await Task.Delay(1));
			Assert.AreEqual(genericHandlersCount++, handlers.GenericHandlers.Count);

			handlers.AddHandler<int>((_) => Expression.Empty());
			Assert.AreEqual(genericHandlersCount++, handlers.GenericHandlers.Count);

			handlers.AddHandler<int>((_, __) => Expression.Empty());
			Assert.AreEqual(genericHandlersCount, handlers.GenericHandlers.Count);
		}

		[Test]
		public void Should_PolicyResultHandlerCollectionIndex_Be_Correct_When_Adding_NonGeneric_And_Generic_Handlers()
		{
			int counter = 0;
			var handlers = new PolicyResultHandlerCollection();
			handlers.AddHandler<int>(async (_) => await Task.Delay(1));
			Assert.AreEqual(counter++, handlers.GenericHandlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler(async (_) => await Task.Delay(1));
			Assert.AreEqual(counter++, handlers.Handlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler(async (_) => await Task.Delay(1));
			Assert.AreEqual(counter++, handlers.Handlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler<int>(async (_) => await Task.Delay(1));
			Assert.AreEqual(counter++, handlers.GenericHandlers.LastOrDefault().CollectionIndex);
			handlers.AddHandler((_) => { });
			Assert.AreEqual(counter++, handlers.Handlers.LastOrDefault().CollectionIndex);

			var allHandlersIndexes = handlers.GenericHandlers.Select(h => h.CollectionIndex)
									.Concat(handlers.Handlers.Select(h => h.CollectionIndex));

			Assert.IsTrue(allHandlersIndexes.GroupBy(x => x).All(x => x.Count() == 1));

			Assert.AreEqual(counter, allHandlersIndexes.Count());
		}
	}
}
