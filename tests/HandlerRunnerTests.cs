using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Policy;

namespace PoliNorError.Tests
{
	internal class HandlerRunnerTests
	{
		[Test]
		public void Should_HandlerRunnersCollection_Work_If_Both_Empty()
		{
			Assert.AreEqual(HandlerRunnerSyncType.None, HandlerRunnersCollection.FromSyncAndNotSync(new List<IHandlerRunner>(), new List<IHandlerRunner>()).MapToSyncType());
		}

		[Test]
		public void Should_HandlerRunnersCollection_Work_If_Sync()
		{
			var syncList = new List<IHandlerRunner>();
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

		[Test]
		public void Should_SyncHandlerRunnerT_Run_Correctly_After_Create()
		{
			bool flag = false;
			void act(PolicyResult<int> prt, CancellationToken _) { if (prt.Result == 10) flag = true; }
			var runner = SyncHandlerRunnerT.Create<int>(act, 1);
			var pr = new PolicyResult<int>();
			pr.SetResult(10);
			runner.Run(pr);
			Assert.IsTrue(flag);
		}

		[Test]
		public async Task Should_ASyncHandlerRunnerT_RunAsync_Correctly_After_Create()
		{
			bool flag = false;
			async Task func(PolicyResult<int> prt, CancellationToken _) { await Task.Delay(1) ; if (prt.Result == 10) flag = true; }
			var runner = ASyncHandlerRunnerT.Create<int>(func, 1);
			var pr = new PolicyResult<int>();
			pr.SetResult(10);
			await runner.RunAsync(pr);
			Assert.IsTrue(flag);
		}

		private class TestHandlerRunner : IHandlerRunner
		{
			public int CollectionIndex => throw new NotImplementedException();

			public bool SyncRun => throw new NotImplementedException();

			public void Run(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
			public Task RunAsync(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
		}
	}
}
