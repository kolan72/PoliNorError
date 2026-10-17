using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	// Regression coverage for the src/Collections refactor:
	// - forAsync no longer derived from configureAwait (configAwait: true parity)
	// - single-pass enumeration of delegate sources
	// - cancellation marker + token-check semantics in the shared loop cores
	// - empty-collection handling after removal of the upfront Any() short-circuit
	// - FlexSyncEnumerable IReadOnlyCollection<T> (O(1) Count)
	// - ArgumentNullException guards on handler ctors and WithPolicyAndDelegate
	internal class PolicyDelegatesHandlerRegressionTests
	{
		[Test]
		public async Task Should_HandleAllAsync_MixedCollection_Produce_Same_Outcome_For_Both_ConfigAwait_Values()
		{
			// RetryPolicy with a throwing delegate yields a genuinely failed result,
			// so the collection short-circuit does not stop before the second delegate.
			var failingSync = new RetryPolicy(1).ToPolicyDelegate((Action)(() => { throw new Exception("boom"); }));
			async Task succeedingAsyncTask(CancellationToken _) { await Task.Delay(1); }
			var succeedingAsync = new SimplePolicy().ToPolicyDelegate(succeedingAsyncTask);

			var collection = PolicyDelegateCollection.Create(failingSync, succeedingAsync);

			var resFalse = await collection.HandleAllAsync(false);
			var resTrue = await collection.HandleAllAsync(true);

			foreach (var res in new[] { resFalse, resTrue })
			{
				Assert.That(res.PolicyDelegateResults.Count(), Is.EqualTo(2));
				Assert.That(res.IsSuccess, Is.True);
				Assert.That(res.LastPolicyResult.IsSuccess, Is.True);
			}
		}

		[Test]
		public async Task Should_HandleAllAsync_T_MixedCollection_Produce_Same_Outcome_For_Both_ConfigAwait_Values()
		{
			Func<int> failingFunc = () => throw new InvalidOperationException("boom");
			var collection = PolicyDelegateCollection<int>.Create(new RetryPolicy(1), failingFunc)
				.WithSimple()
				.AndDelegate(() => 42);

			var resFalse = await collection.HandleAllAsync(false);
			var resTrue = await collection.HandleAllAsync(true);

			foreach (var res in new[] { resFalse, resTrue })
			{
				Assert.That(res.PolicyDelegateResults.Count(), Is.EqualTo(2));
				Assert.That(res.IsSuccess, Is.True);
				Assert.That(res.Result, Is.EqualTo(42));
			}
		}

		[Test]
		public void Should_HandleWhenAllReallySync_Enumerate_Source_Exactly_Once()
		{
			void Act()
			{
				//Empty for tests
			}
			var policyDelegate = new SimplePolicy().ToPolicyDelegate(Act);
			var counting = new CountingEnumerable<PolicyDelegate>(new[] { policyDelegate });

			var (results, _) = PolicyDelegatesHandler.HandleWhenAllReallySync(counting);

			Assert.That(counting.GetEnumeratorCallCount, Is.EqualTo(1));
			Assert.That(results.Count(), Is.EqualTo(1));
		}

		[Test]
		public void Should_GetHandleType_Enumerate_Source_Exactly_Once()
		{
			void Act()
			{
				//Empty for tests
			}
			var policyDelegate = new SimplePolicy().ToPolicyDelegate(Act);
			var counting = new CountingEnumerable<PolicyDelegateBase>(new PolicyDelegateBase[] { policyDelegate });

			var handleType = counting.GetHandleType();

			Assert.That(handleType, Is.EqualTo(PolicyDelegateHandleType.Sync));
			Assert.That(counting.GetEnumeratorCallCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_HandleAllForceSync_Return_Canceled_State_With_Partial_Results_When_Token_Cancelled_Between_Delegates()
		{
			using (var cts = new CancellationTokenSource())
			{
				// Cancels the token and fails, so the loop reaches the between-delegates
				// cancellation check instead of short-circuiting on a successful result.
				void First() { cts.Cancel(); throw new Exception("boom"); }
				void Second()
				{
					//Empty for tests
				}

				var first = new RetryPolicy(1).ToPolicyDelegate(First);
				var second = new SimplePolicy().ToPolicyDelegate(Second);

				var (results, state) = PolicyDelegatesHandler.HandleAllForceSync(new[] { first, second }, cts.Token);

				Assert.That(state.IsCanceled, Is.True);
				Assert.That(results.Count(), Is.EqualTo(1));
			}
		}

		[Test]
		public void Should_HandleAsyncAsSyncSafely_Return_Canceled_Marker_When_Token_Already_Cancelled()
		{
			using (var cts = new CancellationTokenSource())
			{
				async Task NeverRun(CancellationToken _) { await Task.Yield(); }
				var policyDelegate = new SimplePolicy().ToPolicyDelegate(NeverRun);
				cts.Cancel();

				var (policyResult, isCanceled) = policyDelegate.HandleAsyncAsSyncSafely(cts.Token);

				Assert.That(isCanceled, Is.True);
				Assert.That(policyResult, Is.Null);
			}
		}

		[Test]
		public void Should_HandleAll_On_Empty_Collection_Yield_Non_Terminal_Default_State()
		{
			var collection = PolicyDelegateCollection.Create();

			var syncRes = collection.HandleAll();
			var asyncRes = collection.HandleAllAsync(false).GetAwaiter().GetResult();

			foreach (var res in new[] { syncRes, asyncRes })
			{
				Assert.That(res.PolicyDelegateResults, Is.Empty);
				Assert.That(res.IsFailed, Is.False);
				Assert.That(res.IsSuccess, Is.False);
				Assert.That(res.IsCanceled, Is.False);
			}
		}

		[Test]
		public void Should_HandleAll_On_Empty_Typed_Collection_Yield_Non_Terminal_Default_State()
		{
			var collection = PolicyDelegateCollection<int>.Create();

			var syncRes = collection.HandleAll();
			var asyncRes = collection.HandleAllAsync(false).GetAwaiter().GetResult();

			foreach (var res in new[] { syncRes, asyncRes })
			{
				Assert.That(res.PolicyDelegateResults, Is.Empty);
				Assert.That(res.IsFailed, Is.False);
				Assert.That(res.IsSuccess, Is.False);
				Assert.That(res.IsCanceled, Is.False);
			}
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_Cache_LastPolicyResult_Access()
		{
			var success = PolicyResult.ForSync();
			var failed = PolicyResult.ForSync();
			failed.SetFailedInner();

			var results = new List<PolicyDelegateResult>
			{
				new PolicyDelegateResult(success, null, null),
				new PolicyDelegateResult(failed, null, null)
			};
			var collectionResult = new PolicyDelegateCollectionResult(results, new List<PolicyDelegate>());

			var firstAccess = collectionResult.LastPolicyResult;
			var secondAccess = collectionResult.LastPolicyResult;

			ClassicAssert.AreSame(failed, firstAccess);
			ClassicAssert.AreSame(firstAccess, secondAccess);
		}

		[Test]
		public void Should_PolicyDelegateCollectionResultT_Expose_Cached_Result()
		{
			var success = new PolicyResult<int>();
			success.SetResult(10);

			var results = new List<PolicyDelegateResult<int>>
			{
				new PolicyDelegateResult<int>(success, null, null)
			};
			var collectionResult = new PolicyDelegateCollectionResult<int>(results, new List<PolicyDelegate<int>>());

			Assert.That(collectionResult.IsSuccess, Is.True);
			Assert.That(collectionResult.Result, Is.EqualTo(10));
			ClassicAssert.AreSame(success, collectionResult.LastPolicyResult);
		}

		[Test]
		public void Should_FlexSyncEnumerable_Expose_Count_Via_ReadOnlyCollection()
		{
			var syncEnumerable = new FlexSyncEnumerable<int>();
			ClassicAssert.IsInstanceOf<IReadOnlyCollection<int>>(syncEnumerable);
			Assert.That(syncEnumerable.Count, Is.EqualTo(0));
			syncEnumerable.Add(1);
			syncEnumerable.Add(2);
			Assert.That(syncEnumerable.Count, Is.EqualTo(2));

			var asyncEnumerable = new FlexSyncEnumerable<int>(true);
			ClassicAssert.IsInstanceOf<IReadOnlyCollection<int>>(asyncEnumerable);
			Assert.That(asyncEnumerable.Count, Is.EqualTo(0));
			asyncEnumerable.Add(1);
			Assert.That(asyncEnumerable.Count, Is.EqualTo(1));
		}

		[Test]
		public void Should_CollectionHandler_Ctor_Throw_On_Null_Collection()
		{
			Assert.Throws<ArgumentNullException>(() => new PolicyDelegateCollectionHandler(null));
			Assert.Throws<ArgumentNullException>(() => new PolicyDelegateCollectionHandler<int>(null));
		}

		[Test]
		public void Should_WithPolicyAndDelegate_Throw_On_Null_Policy()
		{
			var collection = PolicyDelegateCollection.Create(new SimplePolicy(), () => { });
			Assert.Throws<ArgumentNullException>(() => collection.WithPolicyAndDelegate(null, () => { }));

			var collectionT = PolicyDelegateCollection<int>.Create(new SimplePolicy(), () => 1);
			Assert.Throws<ArgumentNullException>(() => collectionT.WithPolicyAndDelegate<int>(null, () => 1));
		}

		private class CountingEnumerable<T> : IEnumerable<T>
		{
			private readonly IEnumerable<T> _inner;

			public CountingEnumerable(IEnumerable<T> inner) => _inner = inner;

			public int GetEnumeratorCallCount { get; private set; }

			public IEnumerator<T> GetEnumerator()
			{
				GetEnumeratorCallCount++;
				return _inner.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
