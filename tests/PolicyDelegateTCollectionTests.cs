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

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create(retrySI);
			policyDelegateCollection = policyDelegateCollection.WithThrowOnLastFailed();

			var res = Assert.ThrowsAsync<PolicyDelegateCollectionException<int>>(async () => await policyDelegateCollection.HandleAllAsync());
			Assert.IsTrue(res.GetResults().FirstOrDefault() == default);
		}

		[Test]
		public void Should_WithThrowOnLastFailed_Throw_When_Setup_By_Func()
		{
			var retry = new RetryPolicy(2);

			int actSave() { throw new Exception("Test"); }
			var retrySI = retry.ToPolicyDelegate(actSave);

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create(retrySI).WithThrowOnLastFailed((_) => new InvalidOperationException());
			Assert.ThrowsAsync<InvalidOperationException>(async () => await policyDelegateCollection.HandleAllAsync());
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
			Assert.AreEqual(true, res.IsSuccess);
		}

		[Test]
		public async Task Should_Handle_For_FallbackAsyncFuncInExtensions_Work()
		{
			Task<int> taskSave(CancellationToken _) { throw new Exception("Test"); }
			async Task<int> func(CancellationToken _) => await Task.FromResult(6);

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create()
										  .WithFallback(func).AndDelegate(taskSave);
			var res = await policyDelegateCollection.HandleAllAsync(new CancellationToken());
			Assert.AreEqual(6, res.Result);
			Assert.AreEqual(true, res.IsSuccess);
		}

		[Test]
		public async Task Should_NoGeneric_IncludeException_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			_ = polBuilder.WithRetry(1).AndDelegate(() => throw new Exception("Test1")).WithRetry(1).AndDelegate(() => throw new Exception("Test2"))
				.IncludeErrorForAll(ex => ex.Message == "Test1")
				.IncludeErrorForAll(ex => ex.Message == "Test2");
			var handleRes = await polBuilder.HandleAllAsync();
			Assert.IsFalse(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_IncludeException_Work(string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder.WithRetry(1).AndDelegate(() => throw new ArgumentNullException(errParamName)).IncludeErrorForAll<ArgumentNullException>();
			var handleRes = await polBuilder.HandleAllAsync();
			Assert.IsFalse(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public async Task Should_Generic_IncludeException_WithFunc_Work(string paramName, bool errFilterUnsatisfied, string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create()
				.WithRetry(1)
				.AndDelegate(() => throw new ArgumentNullException(errParamName))
				.WithFallback(() => 1)
				.AndDelegate(() => throw new ArgumentNullException(errParamName))
				.IncludeErrorForAll<ArgumentNullException>((ae) => ae.ParamName == paramName);
			var handleRes = await polBuilder.HandleAllAsync();

			Assert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			Assert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForAll_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test1"))
				.ExcludeErrorForAll(ex => ex.Message == "Test1")
				.WithRetry(1)
				.AndDelegate(() => throw new Exception("Test1"));
			var handleRes = await polBuilder.HandleAllAsync();

			Assert.AreEqual(true, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			Assert.AreEqual(false, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_Generic_ExcludeErrorForAll_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder
					.WithRetry(1).AndDelegate(() => throw new InvalidOperationException())
					.ExcludeErrorForAll<InvalidOperationException>();
			var handleRes = await polBuilder.HandleAllAsync();
			Assert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("Test", true, "Test")]
		[TestCase("Test2", false, "Test")]
		public async Task Should_Generic_ExcludeErrorForAll_WithFunc_Work(string paramName, bool errFilterUnsatisfied, string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create()
				.WithRetry(1)
				.AndDelegate(() => throw new ArgumentNullException(errParamName))
				.WithFallback(() => 1)
				.AndDelegate(() => throw new ArgumentNullException(errParamName))
				.ExcludeErrorForAll<ArgumentNullException>((ae) => ae.ParamName == paramName);
			var handleRes = await polBuilder.HandleAllAsync();

			Assert.AreEqual(handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied, errFilterUnsatisfied);
			Assert.AreEqual(handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied, errFilterUnsatisfied);
		}

		[Test]
		public async Task Should_SetCommonDelegate_Work_ForSyncFunc()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			var pol = new RetryPolicy(2);
			int actSave() { throw new Exception("Test"); }

			policyDelegateCollection.WithPolicy(pol).AndDelegate(actSave).WithRetry(1).AndDelegate(actSave);

			var polHandleResults = await policyDelegateCollection.HandleAllAsync();

			var handledResult = (IEnumerable<PolicyDelegateResult<int>>)polHandleResults;
			Assert.AreEqual(2, handledResult.Count());

			Assert.AreEqual(3, handledResult.ToList()[0].Result.Errors.Count());
			Assert.AreEqual(2, handledResult.ToList()[1].Result.Errors.Count());
		}

		[Test]
		public void Should_AndDelegate_Work_For_Async_When_Can_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create().WithPolicy(new RetryPolicy(1))
										  .AndDelegate(async (_) => { await Task.Delay(1); return 1; });
			Assert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
		}

		[Test]
		public async Task Should_Result_And_Status_Be_Correct_When_All_Policies_Failed()
		{
			var polDelegates = PolicyDelegateCollection<int>.Create()
															.WithRetry(1)
															.AndDelegate(() => throw new Exception("Test"))
															.WithRetry(1)
															.AndDelegate(() => throw new Exception("Test"))
															;

			var res = await polDelegates.HandleAllAsync();
			Assert.AreEqual(0, res.Result);
			Assert.AreEqual(true, res.IsFailed);
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

			Assert.AreEqual(2, policyDelegateCollection.Count());

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

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create(retrySI, retrySI2);
			var res = await policyDelegateCollection.HandleAllAsync();

			Assert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public void Should_HandleAll_Instant_Cancellation_Work_ForMisc()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo2);

			var result = polDelegateCol.BuildCollectionHandler().Handle(cancelSource.Token);
			Assert.IsFalse(result.Any());

			cancelSource.Dispose();
		}

		[Test]
		public void Should_HandleAll_Works_For_Sync_And_ASync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1");});
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo2);

			var result = polDelegateCol.BuildCollectionHandler().Handle();
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(true, result.IsFailed);
		}

		[Test]
		public void Should_HandleAll_Works_For_BothSync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1");});

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo1);

			var result = polDelegateCol.BuildCollectionHandler().Handle();
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(true, result.IsFailed);
		}

		[Test]
		public void Should_AddPolicyResultHandlerForAll_For_Action_Work()
		{
			int i = 0;
			var polDelegates = PolicyCollection.Create().WithRetry(1).WithRetry(1).ToPolicyDelegateCollection<int>(() => throw new Exception("Test"));
			polDelegates
				.AddPolicyResultHandlerForAll((_, __) => i++)
				.AddPolicyResultHandlerForAll((_) => i++);
			var result = polDelegates.BuildCollectionHandler().Handle();
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(4, i);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_For_AsyncFunc_Work()
		{
			int i = 0;
			var polDelegates = PolicyCollection.Create().WithRetry(1).WithRetry(1).ToPolicyDelegateCollection<int>(() => throw new Exception("Test"));
			polDelegates
				.AddPolicyResultHandlerForAll(async (_, __) => { await Task.Delay(1); i++; })
				.AddPolicyResultHandlerForAll(async (_) => { await Task.Delay(1); i++; });
			var result = await polDelegates.HandleAllAsync();
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(4, i);
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

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create(retrySI, retrySI2);
			var res = await policyDelegateCollection.HandleAllAsync();

			Assert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_HandleAllAsync_With_Instant_Cancellation_Work()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo2);

			var result = await polDelegateCol.HandleAllAsync(cancelSource.Token);
			Assert.IsFalse(result.Any());
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Be_IsCanceled_ForAsync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo2);

			var result = await polDelegateCol.HandleAllAsync(cancelSource.Token);
			Assert.IsFalse(result.Any());
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_HandleAllAsync_Instant_Cancellation_Be_IsCanceled_ForMisc()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1"); });
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo2);

			var result = await polDelegateCol.HandleAllAsync(cancelSource.Token);
			Assert.IsFalse(result.Any());
			cancelSource.Dispose();
		}

		[Test]
		public void Should_PolicyDelegateCollectionHandleException_GetCorrectMessage()
		{
			var handledErrors = GetTestPolicyDelegateResultErrorsCollection();
			var exception = new PolicyDelegateCollectionException<int>(handledErrors);

			Assert.AreEqual("Policy RetryPolicy handled TestClass.Save method with exception: 'Test'.;Policy RetryPolicy handled TestClass.Save method with exception: 'Test2'.", exception.Message);
			Assert.AreEqual("Policy RetryPolicy handled TestClass.Save method with exception: 'Test'.;Policy RetryPolicy handled TestClass.Save method with exception: 'Test2'.", exception.Message);

			Assert.AreEqual(2, exception.GetResults().Count());
			Assert.AreEqual(1, exception.GetResults().FirstOrDefault());
			Assert.AreEqual(2, exception.GetResults().Skip(1).FirstOrDefault());
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForLast_Work()
		{
			int i = 0;
			void action(PolicyResult<int> pr)
			{
				i++;
				pr.SetFailed();
			}

			int j = 0;
			void actionWithCancelType(PolicyResult<int> pr)
			{
				j++;
				pr.SetFailed();
			}

			int k = 0;
			void actionWithCancellation(PolicyResult<int> pr, CancellationToken _)
			{
				k++;
				pr.SetFailed();
			}

			int l = 0;
			async Task func(PolicyResult<int> pr)
			{
				l++;
				await Task.Delay(1);
				pr.SetFailed();
			}

			int m = 0;
			async Task funcWithCancelType(PolicyResult<int> pr)
			{
				m++;
				await Task.Delay(1);
				pr.SetFailed();
			}

			int n = 0;
			async Task funcWithCancellation(PolicyResult<int> pr, CancellationToken _)
			{
				n++;
				await Task.Delay(1);
				pr.SetFailed();
			}

			var polDelegates = PolicyDelegateCollection<int>.Create()
							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => 1)
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(action)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => 1)
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(actionWithCancelType, CancellationType.Precancelable)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => 1)
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(actionWithCancellation)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => 1)
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(func)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => 1)
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(funcWithCancelType, CancellationType.Precancelable)

							.WithRetry(1)
							.AndDelegate(() => throw new Exception("Test"))
							.WithFallback((_) => 1)
							.AndDelegate(() => throw new Exception("Test"))

							.AddPolicyResultHandlerForLast(funcWithCancellation)
							;

			await polDelegates.BuildCollectionHandler().HandleAsync();
			Assert.AreEqual(1, i);
			Assert.AreEqual(1, j);
			Assert.AreEqual(1, k);
			Assert.AreEqual(1, l);
			Assert.AreEqual(1, m);
			Assert.AreEqual(1, n);
		}

		private IEnumerable<PolicyDelegateResultErrors<int>> GetTestPolicyDelegateResultErrorsCollection()
		{
			var policy = new RetryPolicy(1);
			var ts = new TestClass();

			var policySyncInfo = policy.ToPolicyDelegate(ts.Save);

			var polResult = new PolicyResult<int>();
			polResult.AddError(new Exception("Test"));
			polResult.SetResult(1);

			var handledResult = new PolicyDelegateResult<int>(polResult, policy.PolicyName, policySyncInfo.GetMethodInfo());
			var polHandledError = PolicyDelegateResultErrors<int>.FromDelegateResult(handledResult);

			var policySyncInfo2 = policy.ToPolicyDelegate(ts.Save);

			var polResult2 = new PolicyResult<int>();
			polResult2.AddError(new Exception("Test2"));
			polResult2.SetResult(2);

			var handledResult2 = new PolicyDelegateResult<int>(polResult2, policy.PolicyName, policySyncInfo2.GetMethodInfo());
			var polHandledError2 = PolicyDelegateResultErrors<int>.FromDelegateResult(handledResult2);

			return new List<PolicyDelegateResultErrors<int>>() { polHandledError, polHandledError2 };
		}

		[Test]
		public void Should_WithRetry_AndInvokeRetryPolicyParams_Work()
		{
			int i1 = 0;
			void actionError(Exception _) { i1++; }
			var polDelCol = PolicyDelegateCollection<int>.Create();
			var builder = polDelCol.WithRetry(1, ErrorProcessorParam.From(actionError))
								   .AndDelegate(() => throw new Exception("Test"))
								   .WithRetry(1)
								   .AndDelegate(() => throw new Exception("Test"));
			var res = builder.HandleAllAsync().GetAwaiter().GetResult();
			Assert.AreEqual(2, res.Count());
			Assert.AreEqual(1, i1);
		}

		[Test]
		public void Should_WithSimple_AddElement_In_Collection()
		{
			var polDelCol = PolicyDelegateCollection<int>.Create();
			polDelCol.WithSimple();
			Assert.AreEqual(1, polDelCol.Count());
		}

		[Test]
		public void Should_Handling_Empty_PolicyDelegateCollectionT_When_Cancellation_Has_Already_Occured_DoesNotThrow()
		{
			var polDelegateCollection = PolicyDelegateCollection<int>.Create();
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				Assert.DoesNotThrowAsync(async () => await PolicyDelegatesHandler.HandleAllBySyncType((PolicyDelegateCollection<int>)polDelegateCollection, PolicyDelegateHandleType.Sync, cts.Token));
			}
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsFailed_Equals_False_For_Empty_Collection()
		{
			var collectionResult = new PolicyDelegateCollectionResult<int>(new List<PolicyDelegateResult<int>>(), new List<PolicyDelegate<int>>());
			Assert.IsFalse(collectionResult.IsFailed);
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsSuccess_Equals_False_For_Empty_Collection()
		{
			var collectionResult = new PolicyDelegateCollectionResult<int>(new List<PolicyDelegateResult<int>>(), new List<PolicyDelegate<int>>());
			Assert.IsFalse(collectionResult.IsSuccess);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, false)]
		public void Should_PolicyDelegateCollectionResult_IsFailed_Equals_LastPolicyResult_IsFailed_For_NotEmpty_Collection(bool failed, bool res)
		{
			var lastPolResult = new PolicyResult<int>();
			if (failed)
			{
				lastPolResult.SetFailed();
			}
			var collectionResult = new PolicyDelegateCollectionResult<int>(new List<PolicyDelegateResult<int>>() { new PolicyDelegateResult<int>(lastPolResult, "", null) }, new List<PolicyDelegate<int>>());
			Assert.AreEqual(res, collectionResult.IsFailed);
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(false, false, true)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		public void Should_PolicyDelegateCollectionResult_IsSuccess_Equals_LastPolicyResult_IsSuccess_For_NotEmpty_Collection(bool failed, bool canceled, bool res)
		{
			var lastPolResult = new PolicyResult<int>();
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
			var collectionResult = new PolicyDelegateCollectionResult<int>(new List<PolicyDelegateResult<int>>() { new PolicyDelegateResult<int>(lastPolResult, "", null) }, new List<PolicyDelegate<int>>());
			Assert.AreEqual(res, collectionResult.IsSuccess);
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