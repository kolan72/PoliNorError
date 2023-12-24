using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal partial class PolicyCollectionWrapUpTests
	{
		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		public void Should_PolicyCollection_WrapUp_For_Func_Has_Correct_Exception_In_PolicyResult(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var result = CreatePolicyCollectionToTest()
												.WrapUp(new SimplePolicy(), throwOnWrappedCollectionFailed)
												.OuterPolicy
												.Handle<int>(() => throw new IndexOutOfRangeException("Test"));

			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.CollectionError)
			{
				ClassicAssert.AreEqual(typeof(PolicyDelegateCollectionException<int>), result.Errors.FirstOrDefault()?.GetType());
				ClassicAssert.AreEqual(7, ((PolicyDelegateCollectionException<int>)result.Errors.FirstOrDefault()).InnerExceptions.Count());
			}
			else
			{
				ClassicAssert.AreEqual(typeof(IndexOutOfRangeException), result.Errors.FirstOrDefault()?.GetType());
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_PolicyCollection_WrapUp_For_Func_Has_Correct_PolicyResult_When_NoException(bool empty)
		{
			var collection = PolicyCollection.Create();
			if (!empty)
			{
				collection.WithRetry(2).WithRetry(3);
			}

			var result = collection.WrapUp(new SimplePolicy())
												.OuterPolicy
												.Handle(() => 1);
			ClassicAssert.IsFalse(result.Errors.Any());
			ClassicAssert.IsNotNull(result.WrappedPolicyResults);
			ClassicAssert.AreEqual(empty ? 0 : 1, result.Result);
		}

		[Test]
		public void Should_PolicyCollection_WrapUp_For_Func_Has_Correct_Exception_When_NoException_And_SetFailed_In_Handler()
		{
			var result = CreatePolicyCollectionToTest()
											.AddPolicyResultHandlerForAll<int>(pr => pr.SetFailed())
											.WrapUp(new SimplePolicy())
											.OuterPolicy
											.Handle(() => 1);

			ClassicAssert.AreEqual(typeof(PolicyResultHandlerFailedException), result.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFuncT_Has_Correct_Exception_In_PolicyResult(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var result = await CreatePolicyCollectionToTest()
												.WrapUp(new SimplePolicy(), throwOnWrappedCollectionFailed)
												.OuterPolicy
												.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new IndexOutOfRangeException("Test"); });

			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.CollectionError)
			{
				ClassicAssert.AreEqual(typeof(PolicyDelegateCollectionException<int>), result.Errors.FirstOrDefault()?.GetType());
				ClassicAssert.AreEqual(7, ((PolicyDelegateCollectionException<int>)result.Errors.FirstOrDefault()).InnerExceptions.Count());
			}
			else
			{
				ClassicAssert.AreEqual(typeof(IndexOutOfRangeException), result.Errors.FirstOrDefault()?.GetType());
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFuncT_Has_Correct_PolicyResult_When_NoException(bool empty)
		{
			var collection = PolicyCollection.Create();
			if (!empty)
			{
				collection.WithRetry(2).WithRetry(3);
			}

			var result = await collection.WrapUp(new SimplePolicy())
												.OuterPolicy
												.HandleAsync(async (_) => { await Task.Delay(1); return 1; });
			ClassicAssert.IsFalse(result.Errors.Any());
			ClassicAssert.IsNotNull(result.WrappedPolicyResults);
			ClassicAssert.AreEqual(empty ? 0 : 1, result.Result);
		}

		[Test]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFuncT_Has_Correct_Exception_When_NoException_And_SetFailed_In_Handler()
		{
			var result = await CreatePolicyCollectionToTest()
											.AddPolicyResultHandlerForAll<int>(pr => pr.SetFailed())
											.WrapUp(new SimplePolicy())
											.OuterPolicy
											.HandleAsync(async (_) => { await Task.Delay(1); return 1; });

			ClassicAssert.AreEqual(typeof(PolicyResultHandlerFailedException), result.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFuncT_Has_Correct_PolicyResult_When_Exception()
		{
			var collection = PolicyCollection.Create()
							.WithRetry(1)
							.WithRetry(2);

			var result = await collection.WrapUp(new SimplePolicy())
							.OuterPolicy
							.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test"); });
			ClassicAssert.IsTrue(result.WrappedPolicyResults.Any());
		}

		[Test]
		public void Should_PolicyCollection_WrapUp_For_FuncT_Has_Correct_PolicyResult_When_Exception()
		{
			var collection = PolicyCollection.Create()
							.WithRetry(1)
							.WithRetry(2);

			var result = collection.WrapUp(new SimplePolicy())
							.OuterPolicy
							.Handle<int>(() => throw new Exception("Test"));
			ClassicAssert.IsTrue(result.WrappedPolicyResults.Any());
		}
	}
}
