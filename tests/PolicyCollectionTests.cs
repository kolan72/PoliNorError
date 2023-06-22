using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var policyCollection = wayToInitDelay == 0 ?  PolicyCollection.Create().WithWaitAndRetry(1, TimeSpan.FromSeconds(1))
													   :  PolicyCollection.Create().WithWaitAndRetry(1, (_, __) => TimeSpan.FromSeconds(1));
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
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(async () => {await Task.Delay(1); return 1;})
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
	}
}
