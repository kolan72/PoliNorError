using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.HandleErrorPolicyBase;

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

		private class TestHandlerRunner : IHandlerRunner
		{
			public int CollectionIndex => throw new NotImplementedException();

			public bool SyncRun => throw new NotImplementedException();

			public void Run(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
			public Task RunAsync(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
		}
	}
}
