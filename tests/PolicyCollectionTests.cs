using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyCollectionTests
	{
		[Test]
		public void Should_With_Policy_Add_In_Collection()
		{
			var policyCollection = PolicyCollection.Create().WithPolicy(new RetryPolicy(1));
			Assert.AreEqual(1, policyCollection.Count());
		}

		[Test]
		public void Should_WithRetry_Work()
		{
			var policyCollection = PolicyCollection.Create().WithRetry(1);
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			Assert.AreEqual(1, ((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.RetryCount);
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithWaitAndRetry_Work(int wayToInitDelay)
		{
			var policyCollection = wayToInitDelay == 0 ? PolicyCollection.Create().WithWaitAndRetry(1, TimeSpan.FromSeconds(1))
													   : PolicyCollection.Create().WithWaitAndRetry(1, (_, __) => TimeSpan.FromSeconds(1));
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			Assert.AreEqual(1, ((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.RetryCount);
			Assert.AreEqual(1, policyCollection.FirstOrDefault().PolicyProcessor.Count());
			Assert.IsInstanceOf<DelayErrorProcessor>(policyCollection.FirstOrDefault().PolicyProcessor.FirstOrDefault());
		}

		[Test]
		public void Should_WithInfiniteRetry_Work()
		{
			var policyCollection = PolicyCollection.Create().WithInfiniteRetry();
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			Assert.IsTrue(((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.IsInfinite);
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithWaitAndInfiiniteRetry_Work(int wayToInitDelay)
		{
			var policyCollection = wayToInitDelay == 0 ? PolicyCollection.Create().WithWaitAndInfiniteRetry(TimeSpan.FromSeconds(1))
													   : PolicyCollection.Create().WithWaitAndInfiniteRetry((_, __) => TimeSpan.FromSeconds(1));
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			Assert.IsTrue(((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.IsInfinite);
			Assert.AreEqual(1, policyCollection.FirstOrDefault().PolicyProcessor.Count());
			Assert.IsInstanceOf<DelayErrorProcessor>(policyCollection.FirstOrDefault().PolicyProcessor.FirstOrDefault());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithAction_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(() => { })
															 : PolicyCollection.Create().WithFallback((_) => { });
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			Assert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasFallbackAction());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithAsyncFunc_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(async () => await Task.Delay(1))
															 : PolicyCollection.Create().WithFallback(async (_) => await Task.Delay(1));
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			Assert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasAsyncFallbackFunc());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithFuncT_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(() => 1)
															 : PolicyCollection.Create().WithFallback((_) => 1);
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			Assert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasFallbackFunc<int>());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithAsyncFuncT_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(async () => { await Task.Delay(1); return 1; })
															 : PolicyCollection.Create().WithFallback(async (_) => { await Task.Delay(1); return 1; });
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			Assert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasAsyncFallbackFunc<int>());
		}

		[Test]
		public void Should_WithSimple_Work()
		{
			var policyCollection = PolicyCollection.Create().WithSimple();
			Assert.AreEqual(1, policyCollection.Count());
			Assert.IsInstanceOf<SimplePolicy>(policyCollection.FirstOrDefault());
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandlerForAll_Set_Handler_For_Only_Elements_Have_Already_Been_Added(bool sync)
		{
			var policyCollection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			int i = 0;
			if (sync)
			{
				void action1(PolicyResult _, CancellationToken __) { i++; }
				policyCollection.AddPolicyResultHandlerForAll(action1);
			}
			else
			{
				async Task func1(PolicyResult _, CancellationToken __) { i++; await Task.Delay(1); }
				policyCollection.AddPolicyResultHandlerForAll(func1);
			}

			policyCollection.WithRetry(1).WithRetry(1);
			int m = 0;
			if (sync)
			{
				void action2(PolicyResult _, CancellationToken __) { m++; }
				policyCollection.AddPolicyResultHandlerForAll(action2);
			}
			else
			{
				async Task func2(PolicyResult _, CancellationToken __) { m++; await Task.Delay(1); }
				policyCollection.AddPolicyResultHandlerForAll(func2);
			}
			var polDelegates = policyCollection.ToPolicyDelegateCollection(() => throw new Exception("Test"));

			await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
			Assert.AreEqual(4, m);
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_IncludeErrorForAll_Work(string errorParamName)
		{
			var policyDelegateCollection = PolicyCollection
							.Create()
							.WithRetry(1)
							.WithRetry(1)
							.IncludeErrorForAll<ArgumentNullException>()
							.ToPolicyDelegateCollection(() => throw new ArgumentNullException(errorParamName));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			Assert.IsFalse(handleRes.PolicyDelegateResults.Any(pr => pr.Result.ErrorFilterUnsatisfied));
			Assert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeErrorForAll_Work()
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorForAll(ex => ex.Message == "Test1")
						.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			Assert.IsFalse(handleRes.PolicyDelegateResults.Any(pr => pr.Result.ErrorFilterUnsatisfied));
			Assert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_ExcludeErrorForAll_Work(string errorParamName)
		{
			var policyDelegateCollection = PolicyCollection
							.Create()
							.WithFallback(() => { })
							.WithRetry(1)
							.ExcludeErrorForAll<ArgumentNullException>()
							.ToPolicyDelegateCollection(() => throw new ArgumentNullException(errorParamName));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			Assert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			Assert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForAll_Work()
		{
			var policyDelegateCollection = PolicyCollection
							.Create()
							.WithFallback(() => { })
							.WithRetry(1)
							.ExcludeErrorForAll(ex => ex.Message == "Test1")
							.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			Assert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			Assert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_HandleDelegate_Work(bool generic)
		{
			var polCollection = PolicyCollection
								 .Create()
								 .WithRetry(1)
								 .WithRetry(1);

			int resultCount = 0;

			if (generic)
			{
				var res = polCollection.HandleDelegate<int>(() => throw new Exception("Test1"));
				resultCount = res.Count();
			}
			else
			{
				var res = polCollection.HandleDelegate(() => throw new Exception("Test1"));
				resultCount = res.Count();
			}

			Assert.AreEqual(2, resultCount);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public async Task Should_HandleDelegateAsync_Work(bool generic)
		{
			var polCollection = PolicyCollection
							 .Create()
							 .WithRetry(1)
							 .WithRetry(1);

			int resultCount = 0;

			if (generic)
			{
				var res = await polCollection.HandleDelegateAsync<int>(async (_) => {await Task.Delay(1); throw new Exception("Test1");});
				resultCount = res.Count();
			}
			else
			{
				var res = await polCollection.HandleDelegateAsync(async (_) => {await Task.Delay(1); throw new Exception("Test1");});
				resultCount = res.Count();
			}

			Assert.AreEqual(2, resultCount);
		}
	}
}
