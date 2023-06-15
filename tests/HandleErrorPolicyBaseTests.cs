using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PoliNorError.HandleErrorPolicyBase;
using System.Threading;
using System.Linq;

namespace PoliNorError.Tests
{
	internal class HandleErrorPolicyBaseTests
	{
		[Test]
		public void Should_Work_HandleResult_MiscHandlers_ForSyncHandling()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandler(async (__, _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandler((__, _) => i++)
					   ;
			retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(3, i);
		}

		[Test]
		public void Should_Work_HandleResult_AllAsyncHandlers_ForSyncHandling_If_Token_Canceled()
		{
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandler(async (__, _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });

			var res1 = retryPolicy.Handle<int>(() => throw new Exception("Handle"), cancelSource.Token);
			Assert.AreEqual(typeof(OperationCanceledException), res1.HandleResultErrors.FirstOrDefault().InnerException.GetType());

			var res2 = retryPolicy.Handle(() => throw new Exception("Handle"), cancelSource.Token);
			Assert.AreEqual(typeof(OperationCanceledException), res2.HandleResultErrors.FirstOrDefault().InnerException.GetType());
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_Work_HandleResultAsync_MiscHandlers()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy =  retryPolicy.AddPolicyResultHandler((__, _) => i++);
			retryPolicy = retryPolicy.AddPolicyResultHandler((_) => i++);
			retryPolicy.AddPolicyResultHandler((__, _) => i++);

			await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });

			Assert.AreEqual(3, i);
		}

		[Test]
		public void Should_HandleResultError_NotAffect_NoError()
		{
			void action(PolicyResult _) { throw new Exception(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler(action);
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.HandleResultErrors.Count() == 1);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_IsFailed_Even_If_NoError()
		{
			void action(PolicyResult pr) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler(action);
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
		}

		[Test]
		public void Should_HandleResult_HasError_For_AggregateException()
		{
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler((_) => Task.FromException(new Exception()));
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.HandleResultErrors.Count() == 1);
		}

		[Test]
		public void Should_HandlerRunnersCollection_Work_If_Both_Empty()
		{
			Assert.AreEqual(HandlerRunnerSyncType.None, HandlerRunnersCollection.FromSyncAndNotSync(new List<IHandlerRunner>(), new List<IHandlerRunner>()).MapToSyncType());
		}

		[Test]
		public void Should_HandlerRunnersCollection_Work_If_Sync()
		{
			var syncList = new List<IHandlerRunner>() ;
			var asyncList = new List<IHandlerRunner>();
			var collection = HandlerRunnersCollection.FromSyncAndNotSync(syncList, asyncList);
			syncList.Add(new TestHandlerRunner());

			Assert.AreEqual(HandlerRunnerSyncType.Sync, collection.MapToSyncType());
		}

		[Test]
		public void Should_HandlerRunnersCollection_Work_If_ForNotSync()
		{
			var syncList = new List<IHandlerRunner>();
			var asyncList = new List<IHandlerRunner>();
			var collection = HandlerRunnersCollection.FromSyncAndNotSync(syncList, asyncList);
			asyncList.Add(new TestHandlerRunner());

			Assert.AreEqual(HandlerRunnerSyncType.Async, collection.MapToSyncType());
		}

		[Test]
		public void Should_HandlerRunnersCollection_Work_If_Misc()
		{
			var syncList = new List<IHandlerRunner>();
			var asyncList = new List<IHandlerRunner>();
			var collection = HandlerRunnersCollection.FromSyncAndNotSync(syncList, asyncList);

			asyncList.Add(new TestHandlerRunner());
			syncList.Add(new TestHandlerRunner());

			Assert.AreEqual(HandlerRunnerSyncType.Misc, collection.MapToSyncType());
		}

		private class TestHandlerRunner : IHandlerRunner
        {
            public int Num => throw new NotImplementedException();

            public bool UseSync => throw new NotImplementedException();

            public void Run(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
            public Task RunAsync(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
        }
    }
}
