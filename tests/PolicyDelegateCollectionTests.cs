using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class PolicyDelegateCollectionTests
	{
		[Test]
		public void Should_ThrowError_IfSyncPolicyInTerminatedCollection_And_ResultIsFailed()
		{
			var retry = new RetryPolicy(2);

			void actSave() { throw new Exception("Test"); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create(retrySI);
			policyDelegateCollection.WithThrowOnLastFailed();
			ClassicAssert.ThrowsAsync<PolicyDelegateCollectionException>(async () => await policyDelegateCollection.BuildCollectionHandler().HandleAsync());
		}

		[Test]
		public void Should_WithThrowOnLastFailed_Throw_When_Setup_By_Func()
		{
			var retry = new RetryPolicy(2);

			void actSave() { throw new Exception("Test"); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create(retrySI).WithThrowOnLastFailed((_) => new InvalidOperationException());
			ClassicAssert.ThrowsAsync<InvalidOperationException>(async () => await policyDelegateCollection.HandleAllAsync());
		}

		[Test]
		public async Task Should_Handle_IfSyncPolicy_And_NoError()
		{
			var retry = new RetryPolicy(2);

			void actSave() => Expression.Empty();
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create(retrySI);

			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsFalse(res.LastOrDefault().Result.IsFailed);
		}

		[Test]
		public async Task Should_Handle_IfASyncPolicy_And_NoError()
		{
			var retry = new RetryPolicy(2);

			async Task taskSave(CancellationToken _) { await Task.Delay(1); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(taskSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create(retrySI);

			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsFalse(res.LastOrDefault().Result.IsFailed);
		}

		[Test]
		public async Task Should_Handle_IfASyncPolicy_And_Error()
		{
			var retry = new RetryPolicy(2);

			async Task taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception(); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(taskSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create(retrySI);

			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsTrue(res.LastOrDefault().Result.IsFailed);
		}

		[Test]
		public void Should_ThrowError_IfASyncPolicyInTerminatedCollection_And_ResultIsFailed()
		{
			var retry = new RetryPolicy(2);

			async Task taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception("Test"); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(taskSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create(retrySI);
			policyDelegateCollection.WithThrowOnLastFailed();

			ClassicAssert.ThrowsAsync<PolicyDelegateCollectionException>(async () => await policyDelegateCollection.BuildCollectionHandler().HandleAsync());
		}

		[Test]
		public async Task Should_SetLastPolicyInfoDelegate_Work()
		{
			var testClass = new TestAsyncClass();

			var policyDelegateCollection = PolicyDelegateCollection.Create(new RetryPolicy(1), testClass.Save);

			await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(1, testClass.I);
		}

		[Test]
		public async Task Should_SetCommonDelegate_Work_ForAction()
		{
			int i = 0;
			void actSave() { i++; throw new Exception("Test"); }
			var policyDelegateCollection = PolicyDelegateCollection.Create(new RetryPolicy(2), actSave).WithRetry(1).AndDelegate(actSave);

			var polHandleResults = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(2, polHandleResults.Count());

			ClassicAssert.AreEqual(3, polHandleResults.ToList()[0].Result.Errors.Count());
			ClassicAssert.AreEqual(2, polHandleResults.ToList()[1].Result.Errors.Count());
			ClassicAssert.AreEqual(5, i);
		}

		[Test]
		public void Should_SetCommonDelegate_Set_Handler_For_Only_Elements_Have_Already_Been_Added()
		{
			int i = 0;
			async Task<int> funcCommon(CancellationToken _) { i++; await Task.Delay(1); throw new Exception("Test"); }

			var policyDelegateCollection = PolicyDelegateCollection.Create(new RetryPolicy(1), funcCommon).WithRetry(1).AndDelegate(funcCommon);

			int m = 0;
			policyDelegateCollection.WithRetry(1).AndDelegate(async (_) => { m++; await Task.Delay(1); throw new Exception("Test2"); });
			policyDelegateCollection
				.BuildCollectionHandler()
				.Handle();

			ClassicAssert.AreEqual(4, i);
			ClassicAssert.AreEqual(2, m);
		}

		[Test]
		public async Task Should_SetCommonDelegate_Work_ForAsyncFunc()
		{
			async Task taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception("Test"); }

			var policyDelegateCollection = PolicyDelegateCollection.Create(new RetryPolicy(2), taskSave).WithRetry(1).AndDelegate(taskSave);

			var polHandleResults2 = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(2, polHandleResults2.Count());

			ClassicAssert.AreEqual(3, polHandleResults2.ToList()[0].Result.Errors.Count());
			ClassicAssert.AreEqual(2, polHandleResults2.ToList()[1].Result.Errors.Count());
		}

		[Test]
		public void Should_AndDelegate_Work_For_Sync_When_Can_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection.Create(new RetryPolicy(1), () => Expression.Empty());
			ClassicAssert.AreEqual(1, policyDelegateCollection.Count());
			ClassicAssert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public void Should_AndDelegate_Work_For_Async_When_Can_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection.Create(new RetryPolicy(1), async (_) => await Task.Delay(1));

			ClassicAssert.AreEqual(1, policyDelegateCollection.Count());
			ClassicAssert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public async Task Should_HandleAsync_NotContinue_InSuccess_ForNextAfterSuccessPolicy()
		{
			var policyDelegateCollection = PolicyDelegateCollection.Create();
			void actSave() => Expression.Empty();

			policyDelegateCollection.WithRetry(1).AndDelegate(actSave).WithRetry(2).AndDelegate(actSave);

			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(1, res.Count());
			ClassicAssert.AreEqual(1, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public void Should_AddPolicyWithDelegate_InEmptyBuilder_Work_ForAction()
		{
			var policyDelegateCollection = PolicyDelegateCollection.Create();
			void taskSave() { throw new Exception("Test"); }

			policyDelegateCollection.WithPolicyAndDelegate(new RetryPolicy(2), taskSave);

			ClassicAssert.AreEqual(1, policyDelegateCollection.Count());
		}

		[Test]
		public void Should_AddPolicyWithDelegate_InEmptyBuilder_Work_ForFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection.Create();

			async Task taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception("Test"); }

			policyDelegateCollection.WithPolicyAndDelegate(new RetryPolicy(2), taskSave);

			ClassicAssert.AreEqual(1, policyDelegateCollection.Count());
		}

		[Test]
		public async Task Should_UnhandledDelegateInfoCount_Equals_Zero_If_All_Policies_HandledAsync_For_Misc_SyncTypes_Policies()
		{
			var retry = new RetryPolicy(2);

			void actSave() { throw new Exception(); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			async Task funcAsyncSave(CancellationToken _) { await Task.FromResult(2); throw new Exception(); }
			var retry2 = new RetryPolicy(2);
			PolicyDelegate retrySI2 = retry2.ToPolicyDelegate(funcAsyncSave);

			var retry3 = new RetryPolicy(1);
			PolicyDelegate retrySI3 = retry3.ToPolicyDelegate(funcAsyncSave);

			var policyDelegateCollection = PolicyDelegateCollection.Create().WithPolicyDelegate(retrySI).WithPolicyDelegate(retrySI2).WithPolicyDelegate(retrySI3);
			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(3, policyDelegateCollection.Count());
			ClassicAssert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_Work_For_Diff_PoliciesTypes()
		{
			var retry = new RetryPolicy(2);

			void actSave() => throw new Exception();
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			var fallBack = new FallbackPolicy().WithFallbackAction((_) => { });
			PolicyDelegate fallbackSI = fallBack.ToPolicyDelegate(actSave);

			var res = await PolicyDelegateCollection.Create(retrySI, fallbackSI).BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(0, res.PolicyDelegatesUnused.Count());
			ClassicAssert.AreEqual(2, res.PolicyDelegateResults.Count());
		}

		[Test]
		public async Task Should_Break_Handling_If_Canceled_For_Misc_Sync_Types()
		{
			var retry = new RetryPolicy(2);

			void actSave() => throw new Exception();

			var fallBack = new FallbackPolicy().WithFallbackAction((_) => { });
			PolicyDelegate fallbackSI = fallBack.ToPolicyDelegate(actSave);

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(500);

			PolicyDelegate retrySI1 = retry.ToPolicyDelegate(async (_) => { await Task.Delay(1000); throw new Exception(); });
			PolicyDelegate retrySI2 = retry.ToPolicyDelegate(async (_) => { await Task.Delay(1000); throw new Exception(); });

			var res = await PolicyDelegateCollection.Create(retrySI1, retrySI2, fallbackSI).BuildCollectionHandler().HandleAsync(token: cancelTokenSource.Token);
			ClassicAssert.AreEqual(2, res.PolicyDelegatesUnused.Count());
			ClassicAssert.AreEqual(1, res.PolicyDelegateResults.Count());
		}

		[Test]
		public async Task Should_UnhandledDelegateInfoCount_Equals_Zero_If_All_Policies_HandledAsync_For_AllSync_Policies()
		{
			var retry = new RetryPolicy(2);

			void actSave() { throw new Exception(); }
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			void actSave2() => Expression.Empty();
			var retry2 = new RetryPolicy(2);
			PolicyDelegate retrySI2 = retry2.ToPolicyDelegate(actSave2);

			var policyDelegateCollection = PolicyDelegateCollection.Create().WithPolicyDelegate(retrySI).WithPolicyDelegate(retrySI2);
			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_UnhandledDelegateInfoExists_Equals_Zero_ForHandleAllAsync_If_First_Policy_Ok()
		{
			var retry = new RetryPolicy(2);

			void actSave() => Expression.Empty();
			PolicyDelegate retrySI = retry.ToPolicyDelegate(actSave);

			void actSave2() => Expression.Empty();
			var retry2 = new RetryPolicy(2);
			PolicyDelegate retrySI2 = retry2.ToPolicyDelegate(actSave2);

			var policyDelegateCollection = PolicyDelegateCollection.Create().WithPolicyDelegate(retrySI).WithPolicyDelegate(retrySI2);
			var res = await policyDelegateCollection.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(1, res.PolicyDelegatesUnused.Count());
			ClassicAssert.AreEqual(1, res.PolicyDelegateResults.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeErrorForAll_Work()
		{
			var polBuilder = PolicyDelegateCollection.Create();
			polBuilder
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test1"))
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test2"))
				.IncludeErrorForAll(ex => ex.Message == "Test1")
				.IncludeErrorForAll(ex => ex.Message == "Test2");
			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			var policyResultsCollection = handleRes.PolicyDelegateResults.Select(phr => phr.Result).ToList();
			ClassicAssert.AreEqual(2, policyResultsCollection[0].Errors.Count());
			ClassicAssert.AreEqual(2, policyResultsCollection[1].Errors.Count());
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_IncludeErrorForAll_Work(string errorParamName)
		{
			var polBuilder = PolicyDelegateCollection
							.Create()
							.WithRetry(1)
							.AndDelegate(() => throw new ArgumentNullException(errorParamName))
							.IncludeErrorForAll<ArgumentNullException>();
			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public async Task Should_Generic_IncludeErrorForAll_WithFunc_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var polBuilder = PolicyDelegateCollection
							.Create()
							.WithRetry(1)
							.AndDelegate(() => throw new ArgumentNullException(errorParamName))
							.WithFallback(() => Expression.Empty())
							.AndDelegate(() => throw new ArgumentNullException(errorParamName))
							.IncludeErrorForAll<ArgumentNullException>((ae) => ae.ParamName == paramName);

			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", "Test2")]
		[TestCase("Test2", "Test")]
		public async Task Should_Generic_IncludeErrorForAll_With2Calls_WithFunc_Work(string paramName1, string paramName2)
		{
			var polBuilder = PolicyDelegateCollection.Create()
				.WithRetry(1)
				.AndDelegate(() => throw new ArgumentNullException(paramName2))
				.WithFallback(() => Expression.Empty())
				.AndDelegate(() => throw new ArgumentNullException(paramName2))
				.IncludeErrorForAll<ArgumentNullException>((ae) => ae.ParamName == paramName1)
				.IncludeErrorForAll<ArgumentNullException>((ae) => ae.ParamName == paramName2);

			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(false, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(false, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForAll_Work()
		{
			var polBuilder = PolicyDelegateCollection.Create();
			polBuilder.WithRetry(1).AndDelegate(() => throw new Exception("Test1")).ExcludeErrorForAll(ex => ex.Message == "Test1");
			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, handleRes.LastPolicyResultFailedReason);
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_ExcludeErrorForAll_Work(string errorParamName)
		{
			var polBuilder = PolicyDelegateCollection.Create();
			polBuilder.WithRetry(1).AndDelegate(() => throw new ArgumentNullException(errorParamName)).ExcludeErrorForAll<ArgumentNullException>();
			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();
			ClassicAssert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		public async Task Should_Generic_IncludeErrorForLast_Work()
		{
			void testDelegate() => throw new Exception("Test1");
			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(testDelegate)
						.WithRetry(1).AndDelegate(testDelegate)
						.IncludeErrorForLast<ArgumentException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeErrorForLast_Work()
		{
			void testDelegate() => throw new Exception("Test1");
			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(testDelegate)
						.WithRetry(1).AndDelegate(testDelegate)
						.IncludeErrorForLast(ex => ex.Message == "Test2");

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_Generic_ExcludeErrorForLast_Work()
		{
			void testDelegate() => throw new ArgumentException("Test1");
			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(testDelegate)
						.WithRetry(1).AndDelegate(testDelegate)
						.ExcludeErrorForLast<ArgumentException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForLast_Work()
		{
			void testDelegate() => throw new Exception("Test1");
			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(testDelegate)
						.WithRetry(1).AndDelegate(testDelegate)
						.ExcludeErrorForLast(ex => ex.Message == "Test1");

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public async Task Should_IncludeErrorSet_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch))
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch))
						.IncludeErrorSet<ArgumentException, ArgumentNullException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public async Task Should_ExcludeErrorSet_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch))
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch))
						.ExcludeErrorSet<ArgumentException, ArgumentNullException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public async Task Should_IncludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);

			var policyDelegateCollection = PolicyDelegateCollection
						.Create()
						.WithRetry(1).AndDelegate(actionToHandle)
						.WithRetry(1).AndDelegate(actionToHandle);

			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policyDelegateCollection.IncludeInnerError<TestInnerException>();
			}
			else
			{
				policyDelegateCollection.IncludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			Assert.That(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied, Is.False);
			TestHandlingForInnerError.PolicyResultAsserts.AfterHandlingWithIncludeInnerErrorFilter(handleRes.LastPolicyResult, withInnerError, satisfyFilterFunc);
			Assert.That(handleRes.PolicyDelegatesUnused.Count(), Is.EqualTo(0));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public async Task Should_ExcludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);

			var policyDelegateCollection = PolicyDelegateCollection
					.Create()
					.WithRetry(1).AndDelegate(actionToHandle)
					.WithRetry(1).AndDelegate(actionToHandle);

			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policyDelegateCollection.ExcludeInnerError<TestInnerException>();
			}
			else
			{
				policyDelegateCollection.ExcludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			Assert.That(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied, Is.False);
			TestHandlingForInnerError.PolicyResultAsserts.AfterHandlingWithExcludeInnerErrorFilter(handleRes.LastPolicyResult, withInnerError, satisfyFilterFunc);
			Assert.That(handleRes.PolicyDelegatesUnused.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_HandleError_Correctly()
		{
			var innerProcessors = new InnerErrorProcessorFuncs();
			var policyDelegateCollection = PolicyDelegateCollection
					.Create()
					.WithSimple()
					.AndDelegate(ActionWithInner);

			policyDelegateCollection
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable)
					.HandleAll();

			Assert.That(innerProcessors.I, Is.EqualTo(4));

			policyDelegateCollection
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken)
					.HandleAll();

			Assert.That(innerProcessors.J, Is.EqualTo(2));

			policyDelegateCollection
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable)
					.HandleAll();

			Assert.That(innerProcessors.K, Is.EqualTo(4));

			policyDelegateCollection
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken)
					.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken)
					.HandleAll();

			Assert.That(innerProcessors.L, Is.EqualTo(2));
		}

		[Test]
		public void Should_IncludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorSet<ArgumentException, ArgumentNullException>());
		}

		[Test]
		public void Should_ExcludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorSet<ArgumentException, ArgumentNullException>());
		}

		[Test]
		public void Should_ExcludeErrorForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorForLast((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorForLast<ArgumentException>());
		}

		[Test]
		public void Should_IncludeErrorForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorForLast((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorForLast<ArgumentException>());
		}

		[Test]
		[TestCase("Test", true, "Test")]
		[TestCase("Test2", false, "Test")]
		public async Task Should_Generic_ExcludeErrorForAll_WithFunc_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var polBuilder = PolicyDelegateCollection.Create()
				.WithRetry(1)
				.AndDelegate(() => throw new ArgumentNullException(errorParamName))
				.WithFallback(() => Expression.Empty())
				.AndDelegate(() => throw new ArgumentNullException(errorParamName))
				.ExcludeErrorForAll<ArgumentNullException>((ae) => ae.ParamName == paramName);
			var handleRes = await polBuilder.BuildCollectionHandler().HandleAsync();

			ClassicAssert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_For_ActionWithCancelToken_Work()
		{
			var polDelegates = PolicyDelegateCollection.Create()
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test"))
				.WithFallback((_) => { })
				.AndDelegate(() => throw new Exception("Test"))
				;
			int i = 0;
			void action(PolicyResult __, CancellationToken _) { i++; }
			polDelegates.AddPolicyResultHandlerForAll(action);
			await polDelegates.BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_For_Action_Work()
		{
			var polDelegates = PolicyDelegateCollection.Create()
							  .WithRetry(1)
							  .AndDelegate(() => throw new Exception("Test"))
							  .WithFallback((_) => { })
							  .AndDelegate(() => throw new Exception("Test"))
							  ;
			int i = 0;
			void action(PolicyResult _)
			{
				i++;
			}

			polDelegates.AddPolicyResultHandlerForAll(action);
			await polDelegates.BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_Set_Handler_For_Only_Elements_Have_Already_Been_Added()
		{
			var polDelegates = PolicyDelegateCollection.Create()
							  .WithRetry(1)
							  .AndDelegate(() => throw new Exception("Test"))
							  .WithRetry(1)
							  .AndDelegate(() => throw new Exception("Test"))
							  ;
			int i = 0;
			void action1(PolicyResult _) { i++; }
			polDelegates.AddPolicyResultHandlerForAll(action1);

			polDelegates.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test"))
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test"))
				;
			int m = 0;
			void action2(PolicyResult _) { m++; }
			polDelegates.AddPolicyResultHandlerForAll(action2);

			await polDelegates.BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(4, m);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_For_Func_Work_If_All_PolicyDelegates_Is_IsFailed()
		{
			var polDelegates = PolicyDelegateCollection.Create()
								.WithRetry(1)
								.AndDelegate(() => throw new Exception("Test"))
								.WithFallback(async () => { await Task.Delay(1); throw new Exception("Fallback error"); })
								.AndDelegate(() => throw new Exception("Test"))
								;
			int i = 0;

			polDelegates.AddPolicyResultHandlerForAll(async (_) => { i++; await Task.Delay(1); });
			await polDelegates.BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_For_Func_Work()
		{
			var polDelegates = PolicyDelegateCollection.Create()
							  .WithRetry(1)
							  .AndDelegate(() => throw new Exception("Test"))
							  .WithFallback((_) => { })
							  .AndDelegate(() => throw new Exception("Test"))
							  ;
			int i = 0;

			polDelegates.AddPolicyResultHandlerForAll(async (__, _) => { i++; await Task.Delay(1); });
			await polDelegates.BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(2, i);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_SetPolicyResultFailedIf_Work(bool predicateTrue)
		{
			var collection = PolicyDelegateCollection.Create()
								.WithRetry(1)
								.AndDelegate(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"))
								.WithFallback((_) => { })
								.AndDelegate(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"))
								.SetPolicyResultFailedIf(PredicateFuncsForTests.Predicate)
								.HandleAll();

			Assert.That(collection.IsFailed, Is.EqualTo(predicateTrue));
			Assert.That(collection.LastPolicyResultFailedReason, Is.EqualTo(predicateTrue ? PolicyResultFailedReason.PolicyResultHandlerFailed : PolicyResultFailedReason.None));
		}

		[Test]
		public void Should_PolicyDelegateCollectionHandleException_GetCorrectMessage()
		{
			var handledErrors = GetTestPolicyDelegateResultCollection();
			var exception = new PolicyDelegateCollectionException(handledErrors);

			ClassicAssert.AreEqual("Policy RetryPolicy handled TestClass.Save method with exception: 'Test'.;Policy RetryPolicy handled TestClass.Save method with exception: 'Test2'.;", exception.Message);
		}

		[Test]
		public async Task Should_HandleAllAsync_With_Instant_Cancellation_Work()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection.Create(polInfo1, polInfo2);

			var result = await polDelegateCol.BuildCollectionHandler().HandleAsync(token: cancelSource.Token);
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Work_ForAsync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection.Create(polInfo1, polInfo2);

			var result = await polDelegateCol.BuildCollectionHandler().HandleAsync(token: cancelSource.Token);
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Work_ForMisc()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection.Create(polInfo1, polInfo2);

			var result = await polDelegateCol.BuildCollectionHandler().HandleAsync(token: cancelSource.Token);
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);
			cancelSource.Dispose();
		}

		[Test]
		public void Should_HandleAll_Instant_Cancellation_Work_ForMisc()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var result = PolicyDelegateCollection
									.Create(polInfo1, polInfo2)
									.BuildCollectionHandler()
									.Handle(cancelSource.Token);

			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);
			cancelSource.Dispose();
		}

		[Test]
		public void Should_HandleAll_Works_For_Sync_And_ASync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });

			var result = PolicyDelegateCollection
							.Create(polInfo1, polInfo2)
							.BuildCollectionHandler()
							.Handle();
			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.AreEqual(true, result.IsFailed);
		}

		[Test]
		public void Should_HandleAll_Works_For_BothSync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });

			var result = PolicyDelegateCollection
							.Create(polInfo1, polInfo1)
							.BuildCollectionHandler()
							.Handle();

			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.AreEqual(true, result.IsFailed);
		}

		[Test]
		public void Should_Clone_By_FromPolicyDelegates_Work()
		{
			int i = 0;
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { i++; throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(() => { i++; throw new Exception("2"); });

			var polDelegateCol = PolicyDelegateCollection.Create(polInfo1, polInfo2);
			polDelegateCol.BuildCollectionHandler().HandleAsync().GetAwaiter().GetResult();
			ClassicAssert.AreEqual(4, i);

			var polDelegateCol2 = PolicyDelegateCollection.Create(polDelegateCol);
			var resHandle2 = polDelegateCol2.BuildCollectionHandler().HandleAsync().GetAwaiter().GetResult();
			ClassicAssert.AreEqual(2, resHandle2.Count());
			ClassicAssert.AreEqual(8, i);
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsSuccess_Equals_False_When_Cancellation_Occurs_Between_Handling_Policies()
		{
			int i = 0;
			var cts = new CancellationTokenSource();
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate(() => { i++; throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate(() => { i++; cts.Cancel();  throw new Exception("2"); });
			var polInfo3 = new RetryPolicy(1).ToPolicyDelegate(() => { i++; throw new Exception("3"); });

			var polDelegateCol = PolicyDelegateCollection.Create(polInfo1, polInfo2, polInfo3);
			var result = polDelegateCol.HandleAll(cts.Token);
			ClassicAssert.IsTrue(result.IsCanceled);
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.IsFalse(result.IsSuccess);
			ClassicAssert.AreEqual(3, i);
		}

		[Test]
		public void Should_WithRetry_AndInvokeRetryPolicyParams_Work()
		{
			int i1 = 0;
			void actionError(Exception _) { i1++; }
			var polDelCol = PolicyDelegateCollection.Create();
			var builder = polDelCol.WithRetry(1, ErrorProcessorParam.From(actionError)).AndDelegate(() => throw new Exception("Test")).WithRetry(1).AndDelegate(() => throw new Exception("Test"));
			var res = builder.BuildCollectionHandler().HandleAsync().GetAwaiter().GetResult();
			ClassicAssert.AreEqual(2, res.Count());
			ClassicAssert.AreEqual(1, i1);
		}

		[Test]
		public void Should_WithSimple_AddElement_In_Collection()
		{
			var polDelCol = PolicyDelegateCollection.Create();
			polDelCol.WithSimple();
			ClassicAssert.AreEqual(1, polDelCol.Count());
		}

		[Test]
		public void Should_Handling_Empty_PolicyDelegateCollection_When_Cancellation_Has_Already_Occured_DoesNotThrow()
		{
			var polDelegateCollection = PolicyDelegateCollection.Create();
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				ClassicAssert.DoesNotThrowAsync(async () => await PolicyDelegatesHandler.HandleAllBySyncType((PolicyDelegateCollection)polDelegateCollection, PolicyDelegateHandleType.Sync, cts.Token));
			}
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForLast_Work()
		{
			int i = 0;
			void action(PolicyResult pr)
			{
				i++;
				pr.SetFailed();
			}

			int j = 0;
			void actionWithCancelType(PolicyResult pr)
			{
				j++;
				pr.SetFailed();
			}

			int k = 0;
			void actionWithCancellation(PolicyResult pr, CancellationToken _)
			{
				k++;
				pr.SetFailed();
			}

			int l = 0;
			async Task func(PolicyResult pr)
			{
				l++;
				await Task.Delay(1);
				pr.SetFailed();
			}

			int m = 0;
			async Task funcWithCancelType(PolicyResult pr)
			{
				m++;
				await Task.Delay(1);
				pr.SetFailed();
			}

			int n = 0;
			async Task funcWithCancellation(PolicyResult pr, CancellationToken _)
			{
				n++;
				await Task.Delay(1);
				pr.SetFailed();
			}

			var polDelegates = PolicyDelegateCollection.Create()
							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => { })
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(action)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => { })
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(actionWithCancelType, CancellationType.Precancelable)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => { })
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(actionWithCancellation)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => { })
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(func)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => { })
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(funcWithCancelType, CancellationType.Precancelable)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => { })
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(funcWithCancellation)
							;

			await polDelegates.BuildCollectionHandler().HandleAsync();
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(1, j);
			ClassicAssert.AreEqual(1, k);
			ClassicAssert.AreEqual(1, l);
			ClassicAssert.AreEqual(1, m);
			ClassicAssert.AreEqual(1, n);
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsFailed_Equals_False_For_Empty_Collection()
		{
			var collectionResult = new PolicyDelegateCollectionResult(new List<PolicyDelegateResult>(), new List<PolicyDelegate>());
			ClassicAssert.IsFalse(collectionResult.IsFailed);
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsSuccess_Equals_False_For_Empty_Collection()
		{
			var collectionResult = new PolicyDelegateCollectionResult(new List<PolicyDelegateResult>(), new List<PolicyDelegate>());
			ClassicAssert.IsFalse(collectionResult.IsSuccess);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, false)]
		public void Should_PolicyDelegateCollectionResult_IsFailed_Equals_LastPolicyResult_IsFailed_For_NotEmpty_Collection(bool failed, bool res)
		{
			var lastPolResult = new PolicyResult();
			if (failed)
			{
				lastPolResult.SetFailed();
			}
			var collectionResult = new PolicyDelegateCollectionResult(new List<PolicyDelegateResult>() { new PolicyDelegateResult(lastPolResult, "", null) }, new List<PolicyDelegate>());
			ClassicAssert.AreEqual(res, collectionResult.IsFailed);
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(false, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		public void Should_PolicyDelegateCollectionResult_IsSuccess_Equals_LastPolicyResult_IsSuccess_For_NotEmpty_Collection(bool failed, bool canceled, bool res)
		{
			var lastPolResult = new PolicyResult();
			if (failed && canceled)
			{
				lastPolResult.SetFailedAndCanceled();
			}
			else if (failed)
			{
				lastPolResult.SetFailed();
			}
			else if (canceled)
			{
				lastPolResult.SetCanceled();
			}
			var collectionResult = new PolicyDelegateCollectionResult(new List<PolicyDelegateResult>() { new PolicyDelegateResult(lastPolResult, "", null) }, new List<PolicyDelegate>());
			ClassicAssert.AreEqual(res, collectionResult.IsSuccess);
		}

		[Test]
		public void Should_PolicyDelegateCollection_From_PolicyCollection_Handling_FailFast_For_Null_Action()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Action act = null;
			var result = polCollection.HandleDelegate(act);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public async Task Should_PolicyDelegateCollection_From_PolicyCollection_HandlingAsync_FailFast_For_Null_Action()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Action act = null;
			var result = await polCollection.HandleDelegateAsync(act, false);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public void Should_PolicyDelegateCollection_From_PolicyCollection_Handling_FailFast_For_Null_Func()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Func<CancellationToken, Task> func = null;
			var result = polCollection.HandleDelegate(func);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public async Task Should_PolicyDelegateCollection_From_PolicyCollection_HandlingAsync_FailFast_For_Null_Func()
		{
			var polCollection = PolicyCollection.Create()
							.WithRetry(1)
							.WithRetry(1);
			Func<CancellationToken, Task> func = null;
			var result = await polCollection.HandleDelegateAsync(func, false);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		private IEnumerable<PolicyDelegateResult> GetTestPolicyDelegateResultCollection()
		{
			var policy = new RetryPolicy(1);
			var ts = new TestClass();

			var policySyncInfo = policy.ToPolicyDelegate(ts.Save);

			var polResult = new PolicyResult();
			polResult.AddError(new Exception("Test"));

			var handledResult = new PolicyDelegateResult(polResult, policy.PolicyName, policySyncInfo.GetMethodInfo());

			var policySyncInfo2 = policy.ToPolicyDelegate(ts.Save);
			var polResult2 = new PolicyResult();
			polResult.AddError(new Exception("Test2"));

			var handledResult2 = new PolicyDelegateResult(polResult2, policy.PolicyName, policySyncInfo2.GetMethodInfo());

			return new List<PolicyDelegateResult>() { handledResult, handledResult2 };
		}

		private class TestClass
		{
			public void Save()
			{
				// Method intentionally left empty.
			}
		}

		private class TestAsyncClass
		{
			private int _i;

			public async Task Save(CancellationToken ct)
			{
				await Task.Delay(1, ct);
				_i++;
			}

			public int I
			{
				get
				{
					return _i;
				}
			}
		}
	}
}