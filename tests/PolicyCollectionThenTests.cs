using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyCollectionThenTests
	{
		[Test]
		public void Should_Then_ReturnWrapperPolicy_For_RetryWrappingCollection()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			var wrapper = new SimplePolicy();

			var result = collection.Then(wrapper);

			Assert.That(result, Is.SameAs(wrapper));
		}

		[Test]
		public void Should_Then_ReturnWrapperPolicy_For_FallbackWrappingCollection()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			var wrapper = new FallbackPolicy().WithFallbackAction(() => { });

			var result = collection.Then(wrapper);

			Assert.That(result, Is.SameAs(wrapper));
		}

		[Test]
		public void Should_Then_ThrowArgumentNullException_WhenWrapperPolicyIsNull()
		{
			var collection = PolicyCollection.Create().WithRetry(1);

			Assert.Throws<ArgumentNullException>(() => collection.Then<SimplePolicy>(null));
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_Action_When_NoError()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_Action_When_Error()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => throw new Exception("Test"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Any(), Is.True);
			//SimplePolicy handles the exception thrown by the failing collection,
			//so the outer result records the errors but is not failed.
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.False);
			Assert.That(polResult.Errors.Any(), Is.True);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_HandleT_When_NoError()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => 42);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(42));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_HandleT_When_Error()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle<int>(() => throw new Exception("Test"));

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Any(), Is.True);
			//SimplePolicy handles the exception thrown by the failing collection,
			//so the outer result records the errors but is not failed.
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.False);
			Assert.That(polResult.Errors.Any(), Is.True);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsync_When_NoError()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			async Task act(CancellationToken _) { await Task.Delay(1); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsync_When_Error()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			async Task act(CancellationToken _) { await Task.Delay(1); throw new Exception("Async error"); }

			var polResult = await outerPolicy.HandleAsync(act);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Any(), Is.True);
			//SimplePolicy handles the exception thrown by the failing collection,
			//so the outer result records the errors but is not failed.
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.False);
			Assert.That(polResult.Errors.Any(), Is.True);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsyncT_When_NoError()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 99; }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(polResult.Result, Is.EqualTo(99));
			Assert.That(polResult.IsFailed, Is.False);
		}

		[Test]
		public async Task Should_Then_ProduceWrappedPolicyResults_For_HandleAsyncT_When_Error()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var outerPolicy = collection.Then(new SimplePolicy());

			async Task<int> func(CancellationToken _) { await Task.Delay(1); throw new Exception("Async T error"); }

			var polResult = await outerPolicy.HandleAsync(func);

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Any(), Is.True);
			//SimplePolicy handles the exception thrown by the failing collection,
			//so the outer result records the errors but is not failed.
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.False);
			Assert.That(polResult.Errors.Any(), Is.True);
			Assert.That(polResult.IsPolicySuccess, Is.True);
		}

		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		public void Should_Then_Has_Correct_Exception_In_PolicyResult_For_Action(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var collection = PolicyCollection.Create().WithRetry(2).WithRetry(3);
			var result = collection
								.Then(new SimplePolicy(), throwOnWrappedCollectionFailed)
								.Handle(() => throw new IndexOutOfRangeException("Test"));

			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.CollectionError)
			{
				ClassicAssert.AreEqual(typeof(PolicyDelegateCollectionException), result.Errors.FirstOrDefault()?.GetType());
				ClassicAssert.AreEqual(7, ((PolicyDelegateCollectionException)result.Errors.FirstOrDefault()).InnerExceptions.Count());
			}
			else
			{
				ClassicAssert.AreEqual(typeof(IndexOutOfRangeException), result.Errors.FirstOrDefault()?.GetType());
			}
		}

		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		public async Task Should_Then_Has_Correct_Exception_In_PolicyResult_For_AsyncFunc(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var collection = PolicyCollection.Create().WithRetry(2).WithRetry(3);
			var result = await collection
								.Then(new SimplePolicy(), throwOnWrappedCollectionFailed)
								.HandleAsync(async (_) => { await Task.Delay(1); throw new IndexOutOfRangeException("Test"); });

			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.CollectionError)
			{
				ClassicAssert.AreEqual(typeof(PolicyDelegateCollectionException), result.Errors.FirstOrDefault()?.GetType());
				ClassicAssert.AreEqual(7, ((PolicyDelegateCollectionException)result.Errors.FirstOrDefault()).InnerExceptions.Count());
			}
			else
			{
				ClassicAssert.AreEqual(typeof(IndexOutOfRangeException), result.Errors.FirstOrDefault()?.GetType());
			}
		}

		[Test]
		[TestCase(ThrowOnWrappedCollectionFailed.LastError)]
		[TestCase(ThrowOnWrappedCollectionFailed.CollectionError)]
		public async Task Should_Then_Has_Correct_Exception_In_PolicyResult_For_AsyncFuncT(ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			var collection = PolicyCollection.Create().WithRetry(2).WithRetry(3);
			var result = await collection
								.Then(new SimplePolicy(), throwOnWrappedCollectionFailed)
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
		public void Should_Then_Has_Correct_PolicyResult_When_NoException_For_Action(bool empty)
		{
			var collection = PolicyCollection.Create();
			if (!empty)
			{
				collection.WithRetry(2).WithRetry(3);
			}

			var result = collection.Then(new SimplePolicy()).Handle(() => { });

			ClassicAssert.IsFalse(result.Errors.Any());
			ClassicAssert.IsNotNull(result.WrappedPolicyResults);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Then_Has_Correct_PolicyResult_When_NoException_For_AsyncFuncT(bool empty)
		{
			var collection = PolicyCollection.Create();
			if (!empty)
			{
				collection.WithRetry(2).WithRetry(3);
			}

			var result = await collection
								.Then(new SimplePolicy())
								.HandleAsync(async (_) => { await Task.Delay(1); return 1; });

			ClassicAssert.IsFalse(result.Errors.Any());
			ClassicAssert.IsNotNull(result.WrappedPolicyResults);
			ClassicAssert.AreEqual(empty ? 0 : 1, result.Result);
		}

		[Test]
		public void Should_Then_PreserveWrapperPolicyName_InResult()
		{
			const string wrapperName = "MyCollectionWrapper";
			var collection = PolicyCollection.Create().WithRetry(1);
			var outerPolicy = collection.Then(new SimplePolicy().WithPolicyName(wrapperName));

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.PolicyName, Is.EqualTo(wrapperName));
		}

		[Test]
		public void Should_Then_ProduceWrappedPolicyResults_For_Action_When_NoError_For_EmptyCollection()
		{
			var collection = PolicyCollection.Create();
			var outerPolicy = collection.Then(new SimplePolicy());

			var polResult = outerPolicy.Handle(() => { });

			Assert.That(polResult.WrappedPolicyResults, Is.Not.Null);
			Assert.That(polResult.WrappedPolicyResults.Count(), Is.EqualTo(0));
			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.NoError, Is.True);
		}

		[Test]
		public void Should_Then_FallbackRecoversCollectionFailure()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			var outerPolicy = collection.Then(new FallbackPolicy().WithFallbackAction(() => { }));

			var polResult = outerPolicy.Handle(() => throw new Exception("Collection failure"));

			Assert.That(polResult.IsFailed, Is.False);
			Assert.That(polResult.WrappedPolicyResults.Any(), Is.True);
			Assert.That(polResult.WrappedPolicyResults.First().Result.IsFailed, Is.True);
		}

		[Test]
		public void Should_Then_Work_Equivalently_To_WrapUp_OuterPolicy()
		{
			var collection1 = PolicyCollection.Create().WithRetry(1).WithRetry(2);
			var collection2 = PolicyCollection.Create().WithRetry(1).WithRetry(2);

			var viaThen = collection1.Then(new SimplePolicy());
			var viaWrapUp = collection2.WrapUp(new SimplePolicy()).OuterPolicy;

			var resultThen = viaThen.Handle(() => throw new Exception("Test"));
			var resultWrapUp = viaWrapUp.Handle(() => throw new Exception("Test"));

			Assert.That(resultThen.IsFailed, Is.EqualTo(resultWrapUp.IsFailed));
			Assert.That(resultThen.WrappedPolicyResults.Count(), Is.EqualTo(resultWrapUp.WrappedPolicyResults.Count()));
		}
	}
}
