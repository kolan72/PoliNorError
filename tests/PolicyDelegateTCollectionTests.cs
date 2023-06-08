using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyDelegateTCollectionTests
	{
		[Test]
		public void Should_ThrowError_IfSyncPolicyTerminated_And_ResultIsFailed()
		{
			var retry = new RetryPolicy(2);

			int actSave() { throw new Exception("Test"); }
			PolicyDelegate<int> retrySI = retry.ToPolicyDelegate(actSave);

			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicyDelegates(retrySI);
			policyDelegateCollection = policyDelegateCollection.WithThrowOnLastFailed();

			var res = Assert.ThrowsAsync<PolicyDelegateCollectionHandleException<int>>(async () => await policyDelegateCollection.HandleAllAsync());
			Assert.IsTrue(res.ErrorResults.FirstOrDefault() == default);
		}

		[Test]
		public async Task Should_Throw_NoDelegateException_IfNoPolicyDelegate()
		{
			var retry = new RetryPolicy(2);
			var retry2 = new RetryPolicy(2).ToPolicyDelegate<int>((_) => throw new Exception("Test"));

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create().WithPolicy(retry).WithPolicyDelegate(retry2);

			var result = await policyDelegateCollection.HandleAllAsync();

			Assert.AreEqual(result.PolicyHandledResults.FirstOrDefault().Result.Errors.FirstOrDefault()?.GetType(), typeof(NoDelegateException));
		}

		[Test]
		public void Should_NotThrowError_IfAddPolicyWithDelegate_After_PolicyWithNoDelegate_ForSync()
		{
			var noDelegatePolicy = new RetryPolicy(2);
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create().WithPolicy(noDelegatePolicy);
			int taskSave() { throw new Exception("Test"); }
			Assert.DoesNotThrow(() => policyDelegateCollection.WithPolicyAndDelegate(new RetryPolicy(2), taskSave));
		}

		[Test]
		public async Task Should_Handle_For_FallbackFuncInExtensions_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			int taskSave() { throw new Exception("Test"); }
			int func(CancellationToken _) => 6;
			policyDelegateCollection = policyDelegateCollection.WithFallback(func).AndDelegate(taskSave);
			var res = await policyDelegateCollection.HandleAllAsync();
			Assert.AreEqual(6, res.Result);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.LastPolicySuccess, res.Status);
		}

		[Test]
		public async Task Should_Handle_For_FallbackAsyncFuncInExtensions_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			Task<int> taskSave(CancellationToken _) { throw new Exception("Test"); }
			async Task<int> func(CancellationToken _) => await Task.FromResult(6);
			policyDelegateCollection = policyDelegateCollection.WithFallback(func).AndDelegate(taskSave);
			var res = await policyDelegateCollection.HandleAllAsync(new CancellationToken());
			Assert.AreEqual(6, res.Result);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.LastPolicySuccess, res.Status);
		}

		[Test]
		public void Should_NotThrowError_IfAddPolicyWithDelegate_After_PolicyWithNoDelegate_ForASync()
		{
			var noDelegatePolicy = new RetryPolicy(2);
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create().WithPolicy(noDelegatePolicy);

			async Task<int> taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception("Test"); }

			Assert.DoesNotThrow(() => policyDelegateCollection.WithPolicyAndDelegate(new RetryPolicy(2), taskSave));
		}

		[Test]
		public void Should_WithPolicyWithoutDelegate_ThrowError_WhenLastPolicyWithoutDelegate()
		{
			var builder = PolicyDelegateCollection<int>.Create();
			var pol = new RetryPolicy(2);

			int actSave() => throw new Exception("Test");

			builder = builder.WithRetry(1).AndDelegate(actSave).WithPolicy(pol);

			Assert.Throws<InconsistencyPolicyException>(() => builder.WithPolicy(new RetryPolicy(2)));
		}

		[Test]
		public void Should_FromPolicyDelegates_ThrowError_WhenNotOnlyLastPolicyWithoutDelegate()
		{
			var pol = new RetryPolicy(2);

			int actSave() { throw new Exception("Test"); }

			Assert.Throws<InconsistencyPolicyException>(() => PolicyDelegateCollection<int>.FromPolicyDelegates(pol.ToPolicyDelegate(actSave), pol.ToPolicyDelegate<int>(), pol.ToPolicyDelegate<int>()));
		}

		[Test]
		public void Should_FromPolicyDelegates_Work_WhenOnlyLastPolicyWithoutDelegate()
		{
			var pol = new RetryPolicy(2);

			int actSave() { throw new Exception("Test"); }

			var polBuilder = PolicyDelegateCollection<int>.FromPolicyDelegates(pol.ToPolicyDelegate(actSave), pol.ToPolicyDelegate<int>());

			Assert.AreEqual(2, polBuilder.Count());
		}

		[Test]
		public void Should_FromPolicyDelegates_Work_WhenAllPoliciesWithoutDelegates()
		{
			var pol = new RetryPolicy(2);

			var polBuilder = PolicyDelegateCollection<int>.FromPolicyDelegates(pol.ToPolicyDelegate<int>(), pol.ToPolicyDelegate<int>());

			Assert.AreEqual(2, polBuilder.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeException_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			_ = polBuilder.WithRetry(1).AndDelegate(() => throw new Exception("Test1")).WithRetry(1).AndDelegate(() => throw new Exception("Test2"))
				.ForError(ex => ex.Message == "Test1")
				.ForError(ex => ex.Message == "Test2")
				;
			var handleRes = await polBuilder.HandleAllAsync();
			Assert.IsFalse(handleRes.PolicyHandledResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_IncludeException_Work(string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder.WithRetry(1).AndDelegate(() => throw new ArgumentNullException(errParamName)).ForError<ArgumentNullException>();
			var handleRes = await polBuilder.HandleAllAsync();
			Assert.IsFalse(handleRes.PolicyHandledResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public async Task Should_Generic_IncludeException_WithFunc_Work(string paramName, bool errFilterUnsatisfied, string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create()
				.WithRetry(1)
				.WithFallback(() => 1)
				.WithCommonDelegate(() => throw new ArgumentNullException(errParamName))
				.ForError<ArgumentNullException>((ae) => ae.ParamName == paramName);
			var handleRes = await polBuilder.HandleAllAsync();

			Assert.AreEqual(errFilterUnsatisfied, handleRes.PolicyHandledResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			Assert.AreEqual(errFilterUnsatisfied, handleRes.PolicyHandledResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeException_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test1"))
				.ExcludeError(ex => ex.Message == "Test1")
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test1"));
			var handleRes = await polBuilder.HandleAllAsync();

			Assert.AreEqual(true, handleRes.PolicyHandledResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			Assert.AreEqual(false, handleRes.PolicyHandledResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_Generic_ExcludeException_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder.WithRetry(1).AndDelegate(() => throw new InvalidOperationException()).ExcludeError<InvalidOperationException>();
			var handleRes = await polBuilder.HandleAllAsync();
			Assert.IsTrue(handleRes.PolicyHandledResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("Test", true, "Test")]
		[TestCase("Test2", false, "Test")]
		public async Task Should_Generic_ExcludeException_WithFunc_Work(string paramName, bool errFilterUnsatisfied, string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create()
				.WithRetry(1)
				.WithFallback(() => 1)
				.WithCommonDelegate(() => throw new ArgumentNullException(errParamName))
				.ExcludeError<ArgumentNullException>((ae) => ae.ParamName == paramName);
			var handleRes = await polBuilder.HandleAllAsync();

			Assert.AreEqual(handleRes.PolicyHandledResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied, errFilterUnsatisfied);
			Assert.AreEqual(handleRes.PolicyHandledResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied, errFilterUnsatisfied);
		}

		[Test]
		public async Task Should_WithCommonDelegate_Work_ForSyncFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			var pol = new RetryPolicy(2);

			policyDelegateCollection.WithPolicy(pol).WithRetry(1);

			int actSave() { throw new Exception("Test"); }

			policyDelegateCollection.WithCommonDelegate(actSave);

			var polHandleResults = await policyDelegateCollection.HandleAllAsync();

			var handledResult = (IEnumerable<PolicyHandledResult<int>>)polHandleResults;
			Assert.AreEqual(2, handledResult.Count());

			Assert.AreEqual(3, handledResult.ToList()[0].Result.Errors.Count());
			Assert.AreEqual(2, handledResult.ToList()[1].Result.Errors.Count());
		}

		[Test]
		public void Should_WithCommonDelegate_Throws_When_PolicyWithDelegateExists_ForSyncFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			var pol = new RetryPolicy(2);

			int funSave() { throw new Exception("Test"); }

			policyDelegateCollection.WithRetry(1).AndDelegate(funSave).WithPolicy(pol);

			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.WithCommonDelegate(funSave));
		}

		[Test]
		public void Should_ClearDelegates_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(2)).WithFallback(() => 1).WithCommonDelegate(async(ct) => { await Task.Delay(1); throw new Exception(); });
			Assert.IsFalse(policyDelegateCollection.Any(pd => !pd.DelegateExists));
			policyDelegateCollection.ClearDelegates();
			Assert.AreEqual(policyDelegateCollection.Count(), policyDelegateCollection.Count(pd => !pd.DelegateExists));
		}

		[Test]
		public async Task Should_WithCommonDelegate_Work_ForFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			var pol = new RetryPolicy(2);

			policyDelegateCollection.WithPolicy(pol).WithRetry(1);

			async Task<int> taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception("Test"); }

			policyDelegateCollection.WithCommonDelegate(taskSave);

			var polHandleResults = await policyDelegateCollection.HandleAllAsync();

			var handledResult = (IEnumerable<PolicyHandledResult<int>>)polHandleResults;
			Assert.AreEqual(2, handledResult.Count());

			Assert.AreEqual(3, handledResult.ToList()[0].Result.Errors.Count());
			Assert.AreEqual(2, handledResult.ToList()[1].Result.Errors.Count());
		}

		[Test]
		public void Should_WithCommonDelegate_Throws_When_PolicyWithDelegateExists_ForAsyncFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(2)).WithRetry(1);

			async Task<int> taskSave(CancellationToken _) { await Task.Delay(1); throw new Exception("Test"); }

			policyDelegateCollection.WithCommonDelegate(taskSave);

			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.WithCommonDelegate(taskSave));
		}

		[Test]
		public void Should_WithCommonDelegate_Set_Handler_For_Only_Elements_Have_Already_Been_Added()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(1)).WithRetry(1);
			int i = 0;
			async Task<int> funcCommon(CancellationToken _) { i++; await Task.Delay(1) ; throw new Exception("Test"); }

			policyDelegateCollection.WithCommonDelegate(funcCommon);
			int m = 0;
			policyDelegateCollection.WithRetry(1).AndDelegate(async (ct) => { m++; await Task.Delay(1); throw new Exception("Test2"); });
			policyDelegateCollection.HandleAll();

			Assert.AreEqual(4, i);
			Assert.AreEqual(2, m);
		}

		[Test]
		public void Should_AddPolicyDelegate_After_WithCommonDelegate_Work_ForAsyncFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(1)).WithRetry(1);
			int i = 0;
			async Task<int> funcCommon(CancellationToken _) { i++; await Task.Delay(1); throw new Exception("Test"); }

			policyDelegateCollection.WithCommonDelegate(funcCommon).WithRetry(1).AndDelegate(funcCommon);
			policyDelegateCollection.HandleAll();
			Assert.AreEqual(6, i);
		}

		[Test]
		public void Should_AndDelegate_Work_For_Sync_When_Can_Not_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			int act() => 1;
			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.AndDelegate(act));

			policyDelegateCollection.WithPolicyAndDelegate(new RetryPolicy(1), act);

			Assert.AreEqual(1, policyDelegateCollection.Count());
			Assert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public void Should_AndDelegate_Work_For_Sync_When_Can_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(1));
			Assert.False(policyDelegateCollection.FirstOrDefault().DelegateExists);
			policyDelegateCollection.AndDelegate(() => 1);
			Assert.AreEqual(1, policyDelegateCollection.Count());
			Assert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public void Should_AndDelegate_Work_For_Async_When_Can_Not_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 1; }
			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.AndDelegate(func));

			policyDelegateCollection.WithPolicyAndDelegate(new RetryPolicy(1), func);
			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.AndDelegate(func));

			Assert.AreEqual(1, policyDelegateCollection.Count());
			Assert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public void Should_AndDelegate_Work_For_Async_When_Can_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(1));
			policyDelegateCollection.AndDelegate(async (_) => { await Task.Delay(1); return 1; });
			Assert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public void Should_AndDelegate_ForSync_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicyDelegates(new RetryPolicy(1).ToPolicyDelegate(() => 1));
			int act() => 1;
			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.AndDelegate(act));

			var policyDelegateCollection2 = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(1)).AndDelegate(act);
			Assert.IsTrue(policyDelegateCollection2.LastOrDefault().DelegateExists);
		}

		[Test]
		public void Should_AndDelegate_ForAsync_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicyDelegates(new RetryPolicy(1).ToPolicyDelegate(() => 1));
			async Task<int> func(CancellationToken _) { await Task.Delay(1); return 1; }
			Assert.Throws<InvalidOperationException>(() => policyDelegateCollection.AndDelegate(func));
		}

		[Test]
		public async Task Should_WithCommonResultHandler_For_ActionWithCancelToken_Work()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback((_) => 1);
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));
			void action(PolicyResult __, CancellationToken _) { i++; }
			polDelegates.WithCommonResultHandler(action);
			await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_WithCommonResultHandler_For_Action_Work()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback((_) => 1);
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));
			void action(PolicyResult _)
			{
				i++;
			}

			polDelegates.WithCommonResultHandler(action);
			await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_WithCommonResultErrorsHandler_For_ActionWithExceptionsParam_Work()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback((_) => 1);
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));

			polDelegates.WithCommonResultErrorsHandler((IEnumerable<Exception> _) => i++);
			await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_WithCommonResultErrorsHandler_For_ActionWithExceptionsAndCancelTokenParams_Work()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback((_) => 1);
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));

			polDelegates.WithCommonResultErrorsHandler((IEnumerable<Exception> __, CancellationToken _) => i++);
			await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_WithPolicyAsyncResultHandler_For_Func_WithCancelTokenParam_Work()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback((_) => 1);
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));

			polDelegates.WithCommonResultHandler(async (__, _) => { i++; await Task.Delay(1);});
			var res = await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
			Assert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_Result_And_Status_Be_Correct_When_All_Policies_Failed()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithRetry(1);
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));

			var res = await polDelegates.HandleAllAsync();
			Assert.AreEqual(0, res.Result);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Faulted, res.Status);
		}

		[Test]
		public async Task Should_WithPolicyAsyncResultHandler_For_Func_Work()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback((_) => 1);
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));

			polDelegates.WithCommonResultHandler(async (_) => { i++; await Task.Delay(1);});
			var res = await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
			Assert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_WithPolicyAsyncResultHandler_For_Func_Work_If_All_Is_PolicyDelegates_IsFailed()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create().WithRetry(1).WithFallback(async () => { await Task.Delay(1); throw new Exception("Fallback error"); });
			int i = 0;
			polDelegates.WithCommonDelegate(() => throw new Exception("Test"));

			polDelegates.WithCommonResultHandler(async (_) => { i++; await Task.Delay(1);});
			await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_UnhandledDelegateInfoCount_Equals_Zero_If_All_Policies_Handled_For_Only_Sync_Policies()
		{
			var retry = new RetryPolicy(2);

			int actSave() { throw new Exception(); }
			PolicyDelegate<int> retrySI = retry.ToPolicyDelegate(actSave);

			int actSave2() { return 1; }
			PolicyDelegate<int> retrySI2 = retry.ToPolicyDelegate(actSave2);

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create().WithPolicyDelegate(retrySI).WithPolicyDelegate(retrySI2);
			var res = await policyDelegateCollection.HandleAllAsync();

			Assert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_UnhandledDelegateInfoCount_Equals_Zero_If_All_Policies_Handled_For_Misc_SyncTypes_Policies()
		{
			var retry = new RetryPolicy(2);

			int actSave() => throw new Exception();
			PolicyDelegate<int> retrySI = retry.ToPolicyDelegate(actSave);

			async Task<int> actSave2(CancellationToken _) => await Task.FromResult(1);
			var retry2 = new RetryPolicy(2);
			PolicyDelegate<int> retrySI2 = retry2.ToPolicyDelegate(actSave2);

			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicyDelegates(retrySI, retrySI2);
			var res = await policyDelegateCollection.HandleAllAsync();

			Assert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public void Should_HandleAll_Instant_Cancellation_Be_IsCanceled()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.FromPolicyDelegates(polInfo1, polInfo2);

			var result = polDelegateCol.HandleAll(cancelSource.Token);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Canceled, result.Status);
			cancelSource.Dispose();
		}

		[Test]
		public void Should_HandleAll_Works_For_Sync_And_ASync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1");});
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });

			var polDelegateCol = PolicyDelegateCollection<int>.FromPolicyDelegates(polInfo1, polInfo2);

			var result = polDelegateCol.HandleAll();
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Faulted, result.Status);
		}

		[Test]
		public void Should_HandleAll_Works_For_BothSync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1");});

			var polDelegateCol = PolicyDelegateCollection<int>.FromPolicyDelegates(polInfo1, polInfo1);

			var result = polDelegateCol.HandleAll();
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Faulted, result.Status);
		}

		[Test]
		public async Task Should_UnhandledDelegateInfoCount_Equals_Zero_If_All_Policies_Handled_For_Only_Async_Policies()
		{
			var retry = new RetryPolicy(2);

			async Task<int> actSave(CancellationToken _) { await Task.FromResult(1); throw new Exception(); }
			PolicyDelegate<int> retrySI = retry.ToPolicyDelegate(actSave);

			async Task<int> actSave2(CancellationToken _) => await Task.FromResult(1);
			var retry2 = new RetryPolicy(2);
			PolicyDelegate<int> retrySI2 = retry2.ToPolicyDelegate(actSave2);

			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicyDelegates(retrySI, retrySI2);
			var res = await policyDelegateCollection.HandleAllAsync();

			Assert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Be_IsCanceled_ForSync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.FromPolicyDelegates(polInfo1, polInfo2);

			var result = await polDelegateCol.HandleAllAsync(cancelSource.Token);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Canceled, result.Status);
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Be_IsCanceled_ForAsync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.FromPolicyDelegates(polInfo1, polInfo2);

			var result = await polDelegateCol.HandleAllAsync(cancelSource.Token);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Canceled, result.Status);
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Be_IsCanceled_ForMisc()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.FromPolicyDelegates(polInfo1, polInfo2);

			var result = await polDelegateCol.HandleAllAsync(cancelSource.Token);
			Assert.AreEqual(PolicyDelegateCollectionResultStatus.Canceled, result.Status);
			cancelSource.Dispose();
		}

		[Test]
		public void Should_PolicyDelegateCollectionHandleException_GetCorrectMessage()
		{
			var handledErrors = GetTestPolicyHandledErrors();
			var exception = new PolicyDelegateCollectionHandleException<int>(handledErrors);

			Assert.AreEqual("Policy RetryPolicy handled TestClass.Save method with exception: 'Test'.;Policy RetryPolicy handled TestClass.Save method with exception: 'Test2'.", exception.Message);
			Assert.AreEqual("Policy RetryPolicy handled TestClass.Save method with exception: 'Test'.;Policy RetryPolicy handled TestClass.Save method with exception: 'Test2'.", exception.Message);

			Assert.AreEqual(2, exception.ErrorResults.Count());
			Assert.AreEqual(1, exception.ErrorResults.ToList()[0]);
			Assert.AreEqual(2, exception.ErrorResults.ToList()[1]);
		}

		private IEnumerable<PolicyHandledErrors<int>> GetTestPolicyHandledErrors()
		{
			var policy = new RetryPolicy(1);
			var ts = new TestClass();

			var policySyncInfo = policy.ToPolicyDelegate(ts.Save);
			var polhandledInfo = PolicyHandledInfo.FromPolicyDelegate(policySyncInfo);

			var polResult = new PolicyResult<int>();
			polResult.AddError(new Exception("Test"));
			polResult.SetResult(1);

			var handledResult = new PolicyHandledResult<int>(polhandledInfo, polResult);
			var polHandledError = PolicyHandledErrors<int>.FromHandledResult(handledResult);

			var policySyncInfo2 = policy.ToPolicyDelegate(ts.Save);
			var polhandledInfo2 = PolicyHandledInfo.FromPolicyDelegate(policySyncInfo2);

			var polResult2 = new PolicyResult<int>();
			polResult2.AddError(new Exception("Test2"));
			polResult2.SetResult(2);

			var handledResult2 = new PolicyHandledResult<int>(polhandledInfo2, polResult2);
			var polHandledError2 = PolicyHandledErrors<int>.FromHandledResult(handledResult2);

			return new List<PolicyHandledErrors<int>>() { polHandledError, polHandledError2 };
		}

		[Test]
		public async Task Should_WithoutDelegates_Work()
		{
			var polDelCol = PolicyDelegateCollection<int>.Create();
			var builder = polDelCol.WithRetry(1).WithRetry(1);
			var res = await builder.HandleAllAsync();
			Assert.AreEqual(null, res.FirstOrDefault().PolicyInfo.PolicyMethodInfo);

			Assert.IsTrue(res.FirstOrDefault().Result.IsFailed);
		}

		[Test]
		public void Should_WithRetry_AndInvokeRetryPolicyParams_Work()
		{
			int i1 = 0;
			void actionError(Exception _) { i1++; }
			var polDelCol = PolicyDelegateCollection<int>.Create();
			var builder = polDelCol.WithRetry(1, InvokeParams.From(actionError)).WithRetry(1);
			builder.WithCommonDelegate(() => throw new Exception("Test"));
			var res = builder.HandleAllAsync().GetAwaiter().GetResult();
			Assert.AreEqual(2, res.Count());
			Assert.AreEqual(1, i1);
		}

		[Test]
		public void Should_FromPolicyDelegates_Throw_Exception_For_PolicyCollection_Inconsistency()
		{
			var policyDelegates = new List<PolicyDelegate<int>>
			{
				new RetryPolicy(1).ToPolicyDelegate<int>(),
				new RetryPolicy(1).ToPolicyDelegate(() => 1)
			};
			Assert.Throws<InconsistencyPolicyException>(() => PolicyDelegateCollection<int>.FromPolicyDelegates(policyDelegates));
		}

		[Test]
		public void Should_WithThrowOnLastFailed_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.FromPolicies(new RetryPolicy(1));
			policyDelegateCollection.WithThrowOnLastFailed();
			Assert.IsTrue(policyDelegateCollection.ThrowOnLastFailed);
		}

		private class TestClass
		{
			public void Save()
			{
				//
			}
		}
	}
}