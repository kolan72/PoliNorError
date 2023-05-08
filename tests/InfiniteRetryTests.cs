using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class InfiniteRetryTests
	{
		private const int _numOfRetriesAfterLastRetry = 0;
		[Test]
		[TestCase(1, 2)]
		[TestCase(2, 2)]
		[TestCase(3, 2)]
		public void Should_NRetries_Infinite_Work(int numOfRetries, int nTimeInfinite)
		{
			void func() { throw TestExceptionHolder.TestException; }

			var collection = PolicyDelegateCollection.FromOneClonedPolicy(new RetryPolicy(numOfRetries, (opt) => opt.StartTryCount = int.MaxValue - 1), nTimeInfinite).WithCommonDelegate(func);
			var res = collection.HandleAll();
			Assert.IsTrue(res.Count() == nTimeInfinite);
			Assert.IsTrue(res.PolicyHandledResults.FirstOrDefault().Result.Errors.Count() == _numOfRetriesAfterLastRetry + 1);
			Assert.IsTrue(res.Count(ph=>ph.Result.IsFailed) == nTimeInfinite);
		}

		[Test]
		[TestCase(1, 2)]
		[TestCase(2, 2)]
		[TestCase(3, 2)]
		public void Should_ForGeneric_NRetries_Infinite_Work(int numOfRetries, int nTimeInfinite)
		{
			int func() { throw TestExceptionHolder.TestException; }
			var collection = PolicyDelegateCollection<int>.FromOneClonedPolicy(new RetryPolicy(numOfRetries, (opt) => opt.StartTryCount = int.MaxValue - 1), nTimeInfinite).WithCommonDelegate(func);
			var res = collection.HandleAll();
			Assert.IsTrue(res.Count() == nTimeInfinite);
			Assert.IsTrue(res.PolicyHandledResults.FirstOrDefault().Result.Errors.Count() == _numOfRetriesAfterLastRetry + 1);
		}

		[Test]
		[TestCase(1, 2)]
		[TestCase(2, 2)]
		[TestCase(3, 2)]
		public async Task Should_NRetries_InfiniteAsync_Work(int numOfRetries, int nTimeInfinite)
		{
			async Task func(CancellationToken _) { await Task.Delay(1); throw TestExceptionHolder.TestException; }

			var collection = PolicyDelegateCollection.FromOneClonedPolicy(new RetryPolicy(numOfRetries, (opt) => opt.StartTryCount = int.MaxValue - 1), nTimeInfinite).WithCommonDelegate(func);
			var res = await  collection.HandleAllAsync();
			Assert.IsTrue(res.Count() == nTimeInfinite);
			Assert.IsTrue(res.PolicyHandledResults.FirstOrDefault().Result.Errors.Count() == _numOfRetriesAfterLastRetry + 1);
			Assert.IsTrue(res.Count(ph => ph.Result.IsFailed) == nTimeInfinite);
		}

		[Test]
		[TestCase(1, 2)]
		[TestCase(2, 2)]
		[TestCase(3, 2)]
		public async Task Should_ForGeneric_NRetries_InfiniteAsync_Work(int numOfRetries, int nTimeInfinite)
		{
			async Task<int> func(CancellationToken _) { await Task.Delay(1); throw TestExceptionHolder.TestException; }

			var collection = PolicyDelegateCollection<int>.FromOneClonedPolicy(new RetryPolicy(numOfRetries, (opt) => opt.StartTryCount = int.MaxValue - 1), nTimeInfinite).WithCommonDelegate(func);
			var res = await collection.HandleAllAsync();
			Assert.IsTrue(res.Count() == nTimeInfinite);
			Assert.IsTrue(res.PolicyHandledResults.FirstOrDefault().Result.Errors.Count() == _numOfRetriesAfterLastRetry + 1);
			Assert.IsTrue(res.Count(ph => ph.Result.IsFailed) == nTimeInfinite);
		}
	}

	internal static class TestExceptionHolder
	{
		public static Exception TestException => new Exception("Test");
	}
}