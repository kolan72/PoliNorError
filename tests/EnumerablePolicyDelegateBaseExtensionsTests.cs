using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class EnumerablePolicyDelegateBaseExtensionsTests
	{
		[Test]
		public void Should_AddPolicyResultHandlerForLastInner_DoesNotThrow_If_Empty()
		{
			var polDelegateCollectionWithHandlerFromAction = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromAction.AddPolicyResultHandlerForLast((_) => { });
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromAction.HandleAll());

			var polDelegateCollectionWithHandlerFromActionWithCancelType = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromActionWithCancelType.AddPolicyResultHandlerForLast((_) => { }, CancellationType.Precancelable);
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromActionWithCancelType.HandleAll());

			var polDelegateCollectionWithHandlerFromActionWithToken = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromActionWithToken.AddPolicyResultHandlerForLast((_, __) => { });
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromActionWithToken.HandleAll());

			var polDelegateCollectionWithHandlerFromFunc = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromFunc.AddPolicyResultHandlerForLast(async (_) => await Task.Delay(1));
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFunc.HandleAllAsync());

			var polDelegateCollectionWithHandlerFromFuncWithCancelType = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromFuncWithCancelType.AddPolicyResultHandlerForLast(async (_) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFuncWithCancelType.HandleAllAsync());

			var polDelegateCollectionWithHandlerFromFuncWithToken = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromFuncWithToken.AddPolicyResultHandlerForLast(async (_, __) => await Task.Delay(1));
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFuncWithToken.HandleAllAsync());
		}

		[Test]
		public void Should_Generic_AddPolicyResultHandlerForLastInner_DoesNotThrow_If_Empty()
		{
			var polDelegateCollectionWithHandlerFromAction = PolicyDelegateCollection<int>.Create();
			polDelegateCollectionWithHandlerFromAction.AddPolicyResultHandlerForLast((PolicyResult<int> _) => { });
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromAction.HandleAll());

			var polDelegateCollectionWithHandlerFromActionWithCancelType = PolicyDelegateCollection<int>.Create();
			polDelegateCollectionWithHandlerFromActionWithCancelType.AddPolicyResultHandlerForLast((PolicyResult<int> _) => { }, CancellationType.Precancelable);
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromActionWithCancelType.HandleAll());

			var polDelegateCollectionWithHandlerFromActionWithToken = PolicyDelegateCollection<int>.Create();
			polDelegateCollectionWithHandlerFromActionWithToken.AddPolicyResultHandlerForLast((PolicyResult<int> _, CancellationToken __) => { });
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromActionWithToken.HandleAll());

			var polDelegateCollectionWithHandlerFromFunc = PolicyDelegateCollection<int>.Create();
			polDelegateCollectionWithHandlerFromFunc.AddPolicyResultHandlerForLast(async (PolicyResult<int> _) => await Task.Delay(1));
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFunc.HandleAllAsync());

			var polDelegateCollectionWithHandlerFromFuncWithCancelType = PolicyDelegateCollection<int>.Create();
			polDelegateCollectionWithHandlerFromFuncWithCancelType.AddPolicyResultHandlerForLast(async (PolicyResult<int> _) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFuncWithCancelType.HandleAllAsync());

			var polDelegateCollectionWithHandlerFromFuncWithToken = PolicyDelegateCollection<int>.Create();
			polDelegateCollectionWithHandlerFromFuncWithToken.AddPolicyResultHandlerForLast(async (PolicyResult<int> _, CancellationToken __) => await Task.Delay(1));
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFuncWithToken.HandleAllAsync());
		}

		[Test]
		[TestCase(TestType.PolicyDelegateCol)]
		[TestCase(TestType.PolicyDelegateColT)]
		public void Should_WithErrorProcessorOf_Not_Throw_For_EmptyCollection(TestType testType)
		{
			IErrorProcessorRegistration v = null;

			if (testType == TestType.PolicyDelegateCol)
			{
				v = new PolicyDelegateCollectionErrorProcessorRegistration(true);
			}
			else
			{
				v = new PolicyDelegateCollectionErrorProcessorRegistration<int>(true);
			}

			v.WithErrorProcessorOf((Exception _, CancellationToken __) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __, CancellationToken ___) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1));
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1));
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1));
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1));
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessor(new DefaultErrorProcessor());
			Assert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.AreEqual(0, v.Count);
		}

		internal enum TestType
		{
			PolicyDelegateCol,
			PolicyDelegateColT
		}
	}
}
