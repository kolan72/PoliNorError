using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PoliNorError.Tests
{
	internal class PolicyResultErrorsCapacityTests
	{
		[Test]
		public void Should_FlexSyncEnumerable_DefaultCtor_NotPreallocate()
		{
			ClassicAssert.AreEqual(0, GetBackingList(new FlexSyncEnumerable<Exception>()).Capacity);
			ClassicAssert.AreEqual(0, GetBackingList(new FlexSyncEnumerable<Exception>(true)).Capacity);
		}

		[Test]
		[TestCase(1)]
		[TestCase(32)]
		[TestCase(1024)]
		public void Should_FlexSyncEnumerable_SyncCtor_PreserveSpecifiedCapacity(int capacity)
		{
			var enumerable = new FlexSyncEnumerable<Exception>(false, capacity);

			ClassicAssert.AreEqual(0, CountItems(enumerable));
			ClassicAssert.AreEqual(capacity, GetBackingList(enumerable).Capacity);
		}

		[Test]
		[TestCase(1)]
		[TestCase(32)]
		[TestCase(1024)]
		public void Should_FlexSyncEnumerable_AsyncCtor_PreserveSpecifiedCapacity(int capacity)
		{
			var enumerable = new FlexSyncEnumerable<Exception>(true, capacity);
			ClassicAssert.AreEqual(capacity, GetBackingList(enumerable).Capacity);

			enumerable.Add(new Exception("a"));
			enumerable.Add(new Exception("b"));
			ClassicAssert.AreEqual(2, CountItems(enumerable));
		}

		[Test]
		public void Should_SynchronizedList_DefaultCtor_NotPreallocate()
		{
			ClassicAssert.AreEqual(0, GetSynchronizedListBackingList(new SynchronizedList<int>()).Capacity);
		}

		[Test]
		[TestCase(1)]
		[TestCase(64)]
		public void Should_SynchronizedList_CapacityCtor_PreserveSpecifiedCapacity(int capacity)
		{
			var list = new SynchronizedList<int>(capacity);
			ClassicAssert.AreEqual(0, list.Count);
			ClassicAssert.AreEqual(capacity, GetSynchronizedListBackingList(list).Capacity);

			list.Add(1);
			list.Add(2);
			ClassicAssert.AreEqual(2, list.Count);
		}

		[Test]
		public void Should_PolicyResult_DefaultCtor_NotPreallocate()
		{
			ClassicAssert.AreEqual(0, GetErrorsBackingList(new PolicyResult()).Capacity);
			ClassicAssert.AreEqual(0, GetErrorsBackingList(new PolicyResult(true)).Capacity);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_PolicyResult_Ctor_PreserveErrorCapacity(bool forAsync)
		{
			var policyResult = new PolicyResult(forAsync, 64);

			ClassicAssert.AreEqual(0, policyResult.Errors.Count());
			ClassicAssert.AreEqual(64, GetErrorsBackingList(policyResult).Capacity);
		}

		[Test]
		public void Should_PolicyResultT_Ctor_PreserveErrorCapacity()
		{
			var policyResult = new PolicyResult<int>(false, 16);

			ClassicAssert.AreEqual(16, GetErrorsBackingList(policyResult).Capacity);
		}

		[Test]
		public void Should_PolicyResult_Factories_PreserveErrorCapacity()
		{
			ClassicAssert.AreEqual(8, GetErrorsBackingList(PolicyResult.ForSync(8)).Capacity);
			ClassicAssert.AreEqual(8, GetErrorsBackingList(PolicyResult.ForNotSync(8)).Capacity);
			ClassicAssert.AreEqual(8, GetErrorsBackingList(PolicyResult.InitByConfigureAwait(true, 8)).Capacity);
			ClassicAssert.AreEqual(8, GetErrorsBackingList(PolicyResult.InitByConfigureAwait(false, 8)).Capacity);
			ClassicAssert.AreEqual(8, GetErrorsBackingList(PolicyResult<int>.ForSync(8)).Capacity);
			ClassicAssert.AreEqual(8, GetErrorsBackingList(PolicyResult<int>.ForNotSync(8)).Capacity);
		}

		[Test]
		[TestCase(1, 2)]
		[TestCase(5, 6)]
		[TestCase(7, 8)]
		[TestCase(8, 16)]
		[TestCase(1023, 1024)]
		[TestCase(2000, 8)]
		public void Should_LimitedRetry_PreallocateErrorsListCapacity(int retryCount, int expectedCapacity)
		{
			var processor = new DefaultRetryProcessor();
			var retryCountInfo = retryCount > 1024
				//Avoid actually performing thousands of retries: the rule rejects the first error.
				? RetryCountInfo.Limited(retryCount, opt => opt.CanRetryInner = _ => false)
				: RetryCountInfo.Limited(retryCount);

			var result = processor.Retry(() => throw new Exception("test"), retryCountInfo);

			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(expectedCapacity, GetErrorsBackingList(result).Capacity);
		}

		[Test]
		public void Should_LimitedRetryT_PreallocateErrorsListCapacity()
		{
			var processor = new DefaultRetryProcessor();

			var result = processor.Retry<int>(() => throw new Exception("test"), RetryCountInfo.Limited(5));

			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(6, GetErrorsBackingList(result).Capacity);
		}

		[Test]
		public void Should_SuccessfulRetry_HavePreallocatedErrorsCapacity()
		{
			var processor = new DefaultRetryProcessor();

			var result = processor.Retry(() => { }, RetryCountInfo.Limited(5));

			ClassicAssert.IsTrue(result.IsSuccess);
			ClassicAssert.AreEqual(0, result.Errors.Count());
			ClassicAssert.AreEqual(6, GetErrorsBackingList(result).Capacity);
		}

		[Test]
		public void Should_InfiniteRetry_PreallocateErrorsListCapacity()
		{
			var processor = new DefaultRetryProcessor();
			var count = 0;
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var result = processor.Retry(() =>
				{
					if (++count == 10)
					{
						cancelTokenSource.Cancel();
						throw new OperationCanceledException(cancelTokenSource.Token);
					}
					throw new Exception("test");
				}, RetryCountInfo.Infinite(), cancelTokenSource.Token);

				ClassicAssert.IsTrue(result.IsCanceled);
				ClassicAssert.AreEqual(9, result.Errors.Count());
				//9 errors exceed the preallocated cap of 8, so the backing list grew once: 8 => 16.
				ClassicAssert.AreEqual(16, GetErrorsBackingList(result).Capacity);
			}
		}

		[Test]
		public async Task Should_InfiniteRetryAsync_PreallocateErrorsListCapacity()
		{
			var processor = new DefaultRetryProcessor();
			var count = 0;
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var result = await processor.RetryAsync(async cancellationToken =>
				{
					if (++count == 10)
					{
						cancelTokenSource.Cancel();
						throw new OperationCanceledException(cancelTokenSource.Token);
					}
					await Task.Delay(1, cancellationToken);
					throw new Exception("test");
				}, RetryCountInfo.Infinite(), false, cancelTokenSource.Token);

				ClassicAssert.IsTrue(result.IsCanceled);
				ClassicAssert.AreEqual(9, result.Errors.Count());
				//9 errors exceed the preallocated cap of 8, so the backing list grew once: 8 => 16.
				ClassicAssert.AreEqual(16, GetErrorsBackingList(result).Capacity);
			}
		}

		private static int CountItems<T>(FlexSyncEnumerable<T> enumerable)
		{
			var count = 0;
			foreach (var _ in enumerable)
			{
				count++;
			}
			return count;
		}

		private static List<T> GetBackingList<T>(FlexSyncEnumerable<T> enumerable)
		{
			var wrapper = GetPrivateField(enumerable, "_inner");
			var inner = GetPrivateField(wrapper, "_inner");
			if (inner is List<T> list)
			{
				return list;
			}
			var synchronizedList = inner as SynchronizedList<T>;
			Assert.That(synchronizedList, Is.Not.Null, "Expected SynchronizedList wrapper for the async FlexSyncEnumerable.");
			return (List<T>)GetPrivateField(synchronizedList, "_list");
		}

		private static List<Exception> GetErrorsBackingList(PolicyResult policyResult)
		{
			var errorsField = typeof(PolicyResult).GetField("_errors", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.That(errorsField, Is.Not.Null, "PolicyResult._errors field was not found.");
			var flexErrors = (FlexSyncEnumerable<Exception>)errorsField.GetValue(policyResult);
			return GetBackingList(flexErrors);
		}

		private static List<int> GetSynchronizedListBackingList(SynchronizedList<int> synchronizedList)
		{
			return (List<int>)GetPrivateField(synchronizedList, "_list");
		}

		private static object GetPrivateField(object obj, string name)
		{
			var field = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.That(field, Is.Not.Null, "Field '{0}' was not found on {1}.".Replace("{0}", name).Replace("{1}", obj.GetType().Name));
			return field.GetValue(obj);
		}
	}
}
