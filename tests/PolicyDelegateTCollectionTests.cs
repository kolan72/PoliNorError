using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

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

			var res = ClassicAssert.ThrowsAsync<PolicyDelegateCollectionException<int>>(async () => await policyDelegateCollection.HandleAllAsync());
			ClassicAssert.IsTrue(res.GetResults().FirstOrDefault() == default);
		}

		[Test]
		public void Should_WithThrowOnLastFailed_Throw_When_Setup_By_Func()
		{
			var retry = new RetryPolicy(2);

			int actSave() { throw new Exception("Test"); }
			var retrySI = retry.ToPolicyDelegate(actSave);

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create(retrySI).WithThrowOnLastFailed((_) => new InvalidOperationException());
			ClassicAssert.ThrowsAsync<InvalidOperationException>(async () => await policyDelegateCollection.HandleAllAsync());
		}

		[Test]
		public async Task Should_Handle_For_FallbackFuncInExtensions_Work()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create();
			int taskSave() { throw new Exception("Test"); }
			int func(CancellationToken _) => 6;
			policyDelegateCollection = policyDelegateCollection.WithFallback(func).AndDelegate(taskSave);
			var res = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.AreEqual(6, res.Result);
			ClassicAssert.AreEqual(true, res.IsSuccess);
		}

		[Test]
		public async Task Should_Handle_For_FallbackAsyncFuncInExtensions_Work()
		{
			Task<int> taskSave(CancellationToken _) { throw new Exception("Test"); }
			async Task<int> func(CancellationToken _) => await Task.FromResult(6);

			var policyDelegateCollection = PolicyDelegateCollection<int>.Create()
										  .WithFallback(func).AndDelegate(taskSave);
			var res = await policyDelegateCollection.HandleAllAsync(new CancellationToken());
			ClassicAssert.AreEqual(6, res.Result);
			ClassicAssert.AreEqual(true, res.IsSuccess);
		}

		[Test]
		public async Task Should_NoGeneric_IncludeException_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			_ = polBuilder.WithRetry(1).AndDelegate(() => throw new Exception("Test1")).WithRetry(1).AndDelegate(() => throw new Exception("Test2"))
				.IncludeErrorForAll(ex => ex.Message == "Test1")
				.IncludeErrorForAll(ex => ex.Message == "Test2");
			var handleRes = await polBuilder.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_IncludeException_Work(string errParamName)
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder.WithRetry(1).AndDelegate(() => throw new ArgumentNullException(errParamName)).IncludeErrorForAll<ArgumentNullException>();
			var handleRes = await polBuilder.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
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

			ClassicAssert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(errFilterUnsatisfied, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
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

			ClassicAssert.AreEqual(true, handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(false, handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied);
		}

		[Test]
		public async Task Should_Generic_ExcludeErrorForAll_Work()
		{
			var polBuilder = PolicyDelegateCollection<int>.Create();
			polBuilder
					.WithRetry(1).AndDelegate(() => throw new InvalidOperationException())
					.ExcludeErrorForAll<InvalidOperationException>();
			var handleRes = await polBuilder.HandleAllAsync();
			ClassicAssert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, handleRes.LastPolicyResultFailedReason);
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

			ClassicAssert.AreEqual(handleRes.PolicyDelegateResults.Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied, errFilterUnsatisfied);
			ClassicAssert.AreEqual(handleRes.PolicyDelegateResults.Skip(1).Take(1).FirstOrDefault().Result.ErrorFilterUnsatisfied, errFilterUnsatisfied);
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
			ClassicAssert.AreEqual(2, handledResult.Count());

			ClassicAssert.AreEqual(3, handledResult.ToList()[0].Result.Errors.Count());
			ClassicAssert.AreEqual(2, handledResult.ToList()[1].Result.Errors.Count());
		}

		[Test]
		public void Should_AndDelegate_Work_For_Async_When_Can_BeSet()
		{
			var policyDelegateCollection = PolicyDelegateCollection<int>.Create().WithPolicy(new RetryPolicy(1))
										  .AndDelegate(async (_) => { await Task.Delay(1); return 1; });
			ClassicAssert.True(policyDelegateCollection.FirstOrDefault().DelegateExists);
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
			ClassicAssert.AreEqual(0, res.Result);
			ClassicAssert.AreEqual(true, res.IsFailed);
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

			ClassicAssert.AreEqual(2, policyDelegateCollection.Count());

			ClassicAssert.AreEqual(0, res.PolicyDelegatesUnused.Count());
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

			ClassicAssert.AreEqual(0, res.PolicyDelegatesUnused.Count());
		}

		[Test]
		public void Should_HandleAll_Works_For_Sync_And_ASync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1");});
			var polInfo2 = new RetryPolicy(1).ToPolicyDelegate<int>(async (_) => { await Task.Delay(TimeSpan.FromMilliseconds(1)); throw new Exception("2"); });

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo2);

			var result = polDelegateCol.BuildCollectionHandler().Handle();
			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.AreEqual(true, result.IsFailed);
		}

		[Test]
		public void Should_HandleAll_Works_For_BothSync()
		{
			var polInfo1 = new RetryPolicy(1).ToPolicyDelegate<int>(() => { Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult(); throw new Exception("1");});

			var polDelegateCol = PolicyDelegateCollection<int>.Create(polInfo1, polInfo1);

			var result = polDelegateCol.BuildCollectionHandler().Handle();
			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.AreEqual(true, result.IsFailed);
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
			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.AreEqual(4, i);
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
			ClassicAssert.AreEqual(2, result.Count());
			ClassicAssert.AreEqual(4, i);
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

			ClassicAssert.AreEqual(0, res.PolicyDelegatesUnused.Count());
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
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);
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
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);
			cancelSource.Dispose();
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
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);

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
			ClassicAssert.IsFalse(result.Any());
			ClassicAssert.IsTrue(result.IsCanceled);

			cancelSource.Dispose();
		}

		[Test]
		public void Should_PolicyDelegateCollectionHandleException_GetCorrectMessage()
		{
			var handledErrors = GetTestPolicyDelegateResultCollection();
			var exception = new PolicyDelegateCollectionException<int>(handledErrors);

			ClassicAssert.AreEqual("Policy RetryPolicy handled TestClass.Save method with exception: 'Test'.;Policy RetryPolicy handled TestClass.Save method with exception: 'Test2'.", exception.Message);

			ClassicAssert.AreEqual(2, exception.GetResults().Count());
			ClassicAssert.AreEqual(1, exception.GetResults().FirstOrDefault());
			ClassicAssert.AreEqual(2, exception.GetResults().Skip(1).FirstOrDefault());
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
			ClassicAssert.AreEqual(1, i);
			ClassicAssert.AreEqual(1, j);
			ClassicAssert.AreEqual(1, k);
			ClassicAssert.AreEqual(1, l);
			ClassicAssert.AreEqual(1, m);
			ClassicAssert.AreEqual(1, n);
		}

		private IEnumerable<PolicyDelegateResult<int>> GetTestPolicyDelegateResultCollection()
		{
			var policy = new RetryPolicy(1);
			var ts = new TestClass();

			var policySyncInfo = policy.ToPolicyDelegate(ts.Save);

			var polResult = new PolicyResult<int>();
			polResult.AddError(new Exception("Test"));
			polResult.SetResult(1);

			var handledResult = new PolicyDelegateResult<int>(polResult, policy.PolicyName, policySyncInfo.GetMethodInfo());

			var policySyncInfo2 = policy.ToPolicyDelegate(ts.Save);
			var polResult2 = new PolicyResult<int>();
			polResult2.AddError(new Exception("Test2"));
			polResult2.SetResult(2);

			var handledResult2 = new PolicyDelegateResult<int>(polResult2, policy.PolicyName, policySyncInfo2.GetMethodInfo());

			return new List<PolicyDelegateResult<int>>() { handledResult, handledResult2 };
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_SetPolicyResultFailedIf_Work(bool predicateTrue)
		{
			var collection = PolicyDelegateCollection<int>.Create()
								.WithRetry(1)
								.AndDelegate(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"))
								.WithFallback((_) => 1)
								.AndDelegate(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"))
								.SetPolicyResultFailedIf(PredicateFuncsForTests.GenericPredicate)
								.HandleAll();

			Assert.That(collection.IsFailed, Is.EqualTo(predicateTrue));
			Assert.That(collection.LastPolicyResultFailedReason, Is.EqualTo(predicateTrue ? PolicyResultFailedReason.PolicyResultHandlerFailed : PolicyResultFailedReason.None));
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
			ClassicAssert.AreEqual(2, res.Count());
			ClassicAssert.AreEqual(1, i1);
		}

		[Test]
		public void Should_WithSimple_AddElement_In_Collection()
		{
			var polDelCol = PolicyDelegateCollection<int>.Create();
			polDelCol.WithSimple();
			ClassicAssert.AreEqual(1, polDelCol.Count());
		}

		[Test]
		public void Should_Handling_Empty_PolicyDelegateCollectionT_When_Cancellation_Has_Already_Occured_DoesNotThrow()
		{
			var polDelegateCollection = PolicyDelegateCollection<int>.Create();
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				ClassicAssert.DoesNotThrowAsync(async () => await PolicyDelegatesHandler.HandleAllBySyncType((PolicyDelegateCollection<int>)polDelegateCollection, PolicyDelegateHandleType.Sync, cts.Token));
			}
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsFailed_Equals_False_For_Empty_Collection()
		{
			var collectionResult = new PolicyDelegateCollectionResult<int>(new List<PolicyDelegateResult<int>>(), new List<PolicyDelegate<int>>());
			ClassicAssert.IsFalse(collectionResult.IsFailed);
		}

		[Test]
		public void Should_PolicyDelegateCollectionResult_IsSuccess_Equals_False_For_Empty_Collection()
		{
			var collectionResult = new PolicyDelegateCollectionResult<int>(new List<PolicyDelegateResult<int>>(), new List<PolicyDelegate<int>>());
			ClassicAssert.IsFalse(collectionResult.IsSuccess);
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
			ClassicAssert.AreEqual(res, collectionResult.IsFailed);
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
			ClassicAssert.AreEqual(res, collectionResult.IsSuccess);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_PolicyDelegate_HandleSafely_Work(bool byCollection)
		{
			var excToThrow = new NullReferenceException();
			var policy = new SimplePolicy(true)
				.ExcludeError<NullReferenceException>();
			PolicyResult<int> result = null;
			if (byCollection)
			{
				result = PolicyCollection
						.Create(policy)
						.HandleDelegate<int>(() => throw excToThrow)
						.LastPolicyResult;
			}
			else
			{
				result = policy
						.ToPolicyDelegate<int>(() => throw excToThrow)
						.HandleSafely(default);
			}
			Assert.That(result.UnprocessedError, Is.EqualTo(excToThrow));
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.UnhandledError));
			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			Assert.That(result.IsFailed, Is.True);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public async Task Should_PolicyDelegate_HandleSafelyAsync_Work(bool byCollection)
		{
			var excToThrow = new NullReferenceException();
			var policy = new SimplePolicy(true)
						.ExcludeError<NullReferenceException>();
			PolicyResult<int> result = null;
			if (byCollection)
			{
				result = (await PolicyCollection
						.Create(policy)
						.HandleDelegateAsync<int>((_) => throw excToThrow))
						.LastPolicyResult;
			}
			else
			{
				result = await policy
				.ToPolicyDelegate<int>(async (_) => { await Task.Delay(1); throw excToThrow; })
				.HandleSafelyAsync(false, default);
			}
			Assert.That(result.UnprocessedError, Is.EqualTo(excToThrow));
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.UnhandledError));
			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			Assert.That(result.IsFailed, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_PolicyDelegate_HandleAsyncAsSync_Work(bool canceled)
		{
			using (var cts = new CancellationTokenSource())
			{
				if (canceled)
				{
					cts.Cancel();
				}
				var excToThrow = new NullReferenceException();
				var (result, IsCanceled) = new SimplePolicy(true)
					.ExcludeError<NullReferenceException>()
					.ToPolicyDelegate<int>(async (_) => { await Task.Delay(1); throw excToThrow; })
					.HandleAsyncAsSyncSafely(cts.Token);

				if (canceled)
				{
					Assert.That(IsCanceled, Is.True);
					Assert.That(result, Is.Null);
				}
				else
				{
					Assert.That(result.UnprocessedError, Is.EqualTo(excToThrow));
					Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.UnhandledError));
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
					Assert.That(result.IsFailed, Is.True);
				}
			}
		}

		[Test]
		public void Should_PolicyDelegate_HandleAsyncAsSync_Work_WhenHandling_ByCollection()
		{
			var excToThrow = new NullReferenceException();
			var policy = new SimplePolicy(true)
			   .ExcludeError<NullReferenceException>();
			var collectionResult = PolicyCollection
							.Create(policy)
							.HandleDelegate<int>(async (_) => { await Task.Delay(1); throw excToThrow; });
			Assert.That(collectionResult.LastPolicyResult.ErrorFilterUnsatisfied, Is.True);
			Assert.That(collectionResult.IsFailed, Is.True);
		}

		[Test]
		public void Should_PolicyDelegateCollection_From_PolicyCollection_Handling_FailFast_For_Null_SyncFunc()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Func<int> fn = null;
			var result = polCollection.HandleDelegate(fn);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public async Task Should_PolicyDelegateCollection_From_PolicyCollection_HandlingAsync_FailFast_For_Null_SyncFunc()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Func<int> fn = null;
			var result = await polCollection.HandleDelegateAsync(fn, false);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public void Should_PolicyDelegateCollection_From_PolicyCollection_Handling_FailFast_For_Null_AsyncFunc()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Func<CancellationToken, Task<int>> fn = null;
			var result = polCollection.HandleDelegate(fn);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public async Task Should_PolicyDelegateCollection_From_PolicyCollection_HandlingAsync_FailFast_For_Null_AsyncFunc()
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithRetry(1);
			Func<CancellationToken, Task<int>> fn = null;
			var result = await polCollection.HandleDelegateAsync(fn, false);
			ClassicAssert.AreEqual(0, result.Count());
			ClassicAssert.IsTrue(result.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, result.LastPolicyResultFailedReason);
		}

		[Test]
		public async Task Should_Generic_IncludeErrorForLast_Work()
		{
			int testDelegate() => throw new Exception("Test1");
			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(testDelegate)
						.WithRetry(1).AndDelegate(testDelegate)
						.IncludeErrorForLast<int, ArgumentException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeErrorForLast_Work()
		{
			int testDelegate() => throw new Exception("Test1");
			var policyDelegateCollection = PolicyDelegateCollection<int>
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
			int testDelegate() => throw new ArgumentException("Test1");
			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(testDelegate)
						.WithRetry(1).AndDelegate(testDelegate)
						.ExcludeErrorForLast<int, ArgumentException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForLast_Work()
		{
			int testDelegate() => throw new Exception("Test1");
			var policyDelegateCollection = PolicyDelegateCollection<int>
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
			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.IncludeErrorSet<int, ArgumentException, ArgumentNullException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public async Task Should_IncludeErrorSet_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.IncludeErrorSet(errorSet);

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
			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.ExcludeErrorSet<int, ArgumentException, ArgumentNullException>();

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public async Task Should_ExcludeErrorSet_Work_IErrorSetParam(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.WithRetry(1).AndDelegate(TestHandlingForErrorSet.GetTwoGenericParamFunc(testErrorSetMatch))
						.ExcludeErrorSet(errorSet);

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
			var funcToHandle = TestHandlingForInnerError.GetFunc(withInnerError, satisfyFilterFunc);

			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(funcToHandle)
						.WithRetry(1).AndDelegate(funcToHandle);

			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policyDelegateCollection.IncludeInnerError<int, TestInnerException>();
			}
			else
			{
				policyDelegateCollection.IncludeInnerError<int, TestInnerException>(ex => ex.Message == "Test");
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
			var funcToHandle = TestHandlingForInnerError.GetFunc(withInnerError, satisfyFilterFunc);

			var policyDelegateCollection = PolicyDelegateCollection<int>
						.Create()
						.WithRetry(1).AndDelegate(funcToHandle)
						.WithRetry(1).AndDelegate(funcToHandle);

			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policyDelegateCollection.ExcludeInnerError<int, TestInnerException>();
			}
			else
			{
				policyDelegateCollection.ExcludeInnerError<int, TestInnerException>(ex => ex.Message == "Test");
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
			var policyDelegateCollection = PolicyDelegateCollection<int>
					.Create()
					.WithSimple()
					.AndDelegate(FuncWithInner);

			policyDelegateCollection
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.Action)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.Action, CancellationType.Precancelable)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.AsyncFunc)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable)
					.HandleAll();

			Assert.That(innerProcessors.I, Is.EqualTo(4));

			policyDelegateCollection
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.ActionWithToken)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.AsyncFuncWithToken)
					.HandleAll();

			Assert.That(innerProcessors.J, Is.EqualTo(2));

			policyDelegateCollection
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.ActionWithErrorInfo)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable)
					.HandleAll();

			Assert.That(innerProcessors.K, Is.EqualTo(4));

			policyDelegateCollection
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken)
					.WithInnerErrorProcessorOf<int, TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken)
					.HandleAll();

			Assert.That(innerProcessors.L, Is.EqualTo(2));
		}

		[Test]
		public void Should_ExcludeErrorForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection<int>.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorForLast((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorForLast<int, ArgumentException>());
		}

		[Test]
		public void Should_IncludeErrorForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection<int>.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorForLast((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorForLast<int, ArgumentException>());
		}

		[Test]
		public void Should_IncludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection<int>.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorSet<int, ArgumentException, ArgumentNullException>());

			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			Assert.DoesNotThrow(() => polCollection.IncludeErrorSet(errorSet));
		}

		[Test]
		public void Should_ExcludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyDelegateCollection<int>.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorSet<int, ArgumentException, ArgumentNullException>());

			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			Assert.DoesNotThrow(() => polCollection.ExcludeErrorSet(errorSet));
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