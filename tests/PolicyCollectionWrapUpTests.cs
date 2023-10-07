using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal partial class PolicyCollectionWrapUpTests
	{
		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		public void Should_PolicyCollection_WrapUp_For_Action_Has_Correct_Exception_In_PolicyResult(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var result = CreatePolicyCollectionToTest()
												.WrapUp(new SimplePolicy(), throwOnWrappedCollectionFailed)
												.OuterPolicy
												.Handle(() => throw new IndexOutOfRangeException("Test"));

			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.CollectionError)
			{
				Assert.AreEqual(typeof(PolicyDelegateCollectionException), result.Errors.FirstOrDefault()?.GetType());
				Assert.AreEqual(7, ((PolicyDelegateCollectionException)result.Errors.FirstOrDefault()).InnerExceptions.Count());
			}
			else
			{
				Assert.AreEqual(typeof(IndexOutOfRangeException), result.Errors.FirstOrDefault()?.GetType());
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_PolicyCollection_WrapUp_For_Action_Has_Correct_PolicyResult_When_NoException(bool empty)
		{
			var collection = PolicyCollection.Create();
			if (!empty)
			{
				collection.WithRetry(2).WithRetry(3);
			}

			var result = collection.WrapUp(new SimplePolicy())
												.OuterPolicy
												.Handle(() => { });
			Assert.IsFalse(result.Errors.Any());
			Assert.IsNotNull(result.WrappedPolicyResults);
		}

		[Test]
		public void Should_PolicyCollection_WrapUp_For_Action_Has_Correct_Exception_When_NoException_And_SetFailed_In_Handler()
		{
			var result = CreatePolicyCollectionToTest()
											.AddPolicyResultHandlerForAll(pr => pr.SetFailed())
											.WrapUp(new SimplePolicy())
											.OuterPolicy
											.Handle(() => { });

			Assert.AreEqual(typeof(PolicyResultHandlerFailedException), result.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFunc_Has_Correct_Exception_In_PolicyResult(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var result = await CreatePolicyCollectionToTest()
												.WrapUp(new SimplePolicy(), throwOnWrappedCollectionFailed)
												.OuterPolicy
												.HandleAsync(async(_) => { await Task.Delay(1); throw new IndexOutOfRangeException("Test"); });

			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.CollectionError)
			{
				Assert.AreEqual(typeof(PolicyDelegateCollectionException), result.Errors.FirstOrDefault()?.GetType());
				Assert.AreEqual(7, ((PolicyDelegateCollectionException)result.Errors.FirstOrDefault()).InnerExceptions.Count());
			}
			else
			{
				Assert.AreEqual(typeof(IndexOutOfRangeException), result.Errors.FirstOrDefault()?.GetType());
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFunc_Has_Correct_PolicyResult_When_NoException(bool empty)
		{
			var collection = PolicyCollection.Create();
			if (!empty)
			{
				collection.WithRetry(2).WithRetry(3);
			}

			var result = await collection.WrapUp(new SimplePolicy())
												.OuterPolicy
												.HandleAsync(async (_) => await Task.Delay(1));
			Assert.IsFalse(result.Errors.Any());
			Assert.IsNotNull(result.WrappedPolicyResults);
		}

		[Test]
		public async Task Should_PolicyCollection_WrapUp_For_AsyncFunc_Has_Correct_Exception_When_NoException_And_SetFailed_In_Handler()
		{
			var result = await CreatePolicyCollectionToTest()
											.AddPolicyResultHandlerForAll(pr => pr.SetFailed())
											.WrapUp(new SimplePolicy())
											.OuterPolicy
											.HandleAsync(async (_) => await Task.Delay(1));

			Assert.AreEqual(typeof(PolicyResultHandlerFailedException), result.Errors.FirstOrDefault()?.GetType());
		}

		private PolicyCollection CreatePolicyCollectionToTest() => PolicyCollection.Create().WithRetry(2).WithRetry(3);
	}
}
