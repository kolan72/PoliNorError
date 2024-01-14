using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class PolicyCollectionTests
	{
		[Test]
		public void Should_With_Policy_Add_In_Collection()
		{
			var policyCollection = PolicyCollection.Create().WithPolicy(new RetryPolicy(1));
			ClassicAssert.AreEqual(1, policyCollection.Count());
		}

		[Test]
		public void Should_WithRetry_Work()
		{
			var policyCollection = PolicyCollection.Create().WithRetry(1);
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.AreEqual(1, ((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.RetryCount);
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithWaitAndRetry_Work(int wayToInitDelay)
		{
			var policyCollection = wayToInitDelay == 0 ? PolicyCollection.Create().WithWaitAndRetry(1, TimeSpan.FromSeconds(1))
													   : PolicyCollection.Create().WithWaitAndRetry(1, (_, __) => TimeSpan.FromSeconds(1));
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.AreEqual(1, ((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.RetryCount);
			ClassicAssert.AreEqual(1, policyCollection.FirstOrDefault().PolicyProcessor.Count());
			ClassicAssert.IsInstanceOf<DelayErrorProcessor>(policyCollection.FirstOrDefault().PolicyProcessor.FirstOrDefault());
		}

		[Test]
		public void Should_WithInfiniteRetry_Work()
		{
			var policyCollection = PolicyCollection.Create().WithInfiniteRetry();
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.IsTrue(((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.IsInfinite);
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithWaitAndInfiiniteRetry_Work(int wayToInitDelay)
		{
			var policyCollection = wayToInitDelay == 0 ? PolicyCollection.Create().WithWaitAndInfiniteRetry(TimeSpan.FromSeconds(1))
													   : PolicyCollection.Create().WithWaitAndInfiniteRetry((_, __) => TimeSpan.FromSeconds(1));
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.IsTrue(((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.IsInfinite);
			ClassicAssert.AreEqual(1, policyCollection.FirstOrDefault().PolicyProcessor.Count());
			ClassicAssert.IsInstanceOf<DelayErrorProcessor>(policyCollection.FirstOrDefault().PolicyProcessor.FirstOrDefault());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithAction_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(() => { })
															 : PolicyCollection.Create().WithFallback((_) => { });
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasFallbackAction());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithAsyncFunc_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(async () => await Task.Delay(1))
															 : PolicyCollection.Create().WithFallback(async (_) => await Task.Delay(1));
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasAsyncFallbackFunc());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithFuncT_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(() => 1)
															 : PolicyCollection.Create().WithFallback((_) => 1);
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasFallbackFunc<int>());
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		public void Should_WithFallback_WithAsyncFuncT_Work(int withCancelTokenParam)
		{
			var policyCollection = withCancelTokenParam == 0 ? PolicyCollection.Create().WithFallback(async () => { await Task.Delay(1); return 1; })
															 : PolicyCollection.Create().WithFallback(async (_) => { await Task.Delay(1); return 1; });
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<FallbackPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.IsTrue(((FallbackPolicy)policyCollection.FirstOrDefault()).HasAsyncFallbackFunc<int>());
		}

		[Test]
		public void Should_WithSimple_Work()
		{
			var policyCollection = PolicyCollection.Create().WithSimple();
			ClassicAssert.AreEqual(1, policyCollection.Count());
			ClassicAssert.IsInstanceOf<SimplePolicy>(policyCollection.FirstOrDefault());
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandlerForAll_Set_Handler_For_Only_Elements_Have_Already_Been_Added(bool sync)
		{
			var policyCollection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			int i = 0;
			if (sync)
			{
				void action1(PolicyResult _, CancellationToken __) { i++; }
				policyCollection.AddPolicyResultHandlerForAll(action1);
			}
			else
			{
				async Task func1(PolicyResult _, CancellationToken __) { i++; await Task.Delay(1); }
				policyCollection.AddPolicyResultHandlerForAll(func1);
			}

			policyCollection.WithRetry(1).WithRetry(1);
			int m = 0;
			if (sync)
			{
				void action2(PolicyResult _, CancellationToken __) { m++; }
				policyCollection.AddPolicyResultHandlerForAll(action2);
			}
			else
			{
				async Task func2(PolicyResult _, CancellationToken __) { m++; await Task.Delay(1); }
				policyCollection.AddPolicyResultHandlerForAll(func2);
			}
			var polDelegates = policyCollection.ToPolicyDelegateCollection(() => throw new Exception("Test"));

			await polDelegates.HandleAllAsync();
			ClassicAssert.AreEqual(2, i);
			ClassicAssert.AreEqual(4, m);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_By_Generic_Action_Work()
		{
			var policyCollection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			int i = 0;
			void action1(PolicyResult<int> _, CancellationToken __) { i++; }
			void action2(PolicyResult<int> _) { i++; }
			policyCollection
				.AddPolicyResultHandlerForAll<int>(action1)
				.AddPolicyResultHandlerForAll<int>(action2);

			await policyCollection.HandleDelegateAsync<int>(() => throw new Exception("Test"));
			ClassicAssert.AreEqual(4, i);
		}

		[Test]
		public async Task Should_AddPolicyResultHandlerForAll_By_Generic_AsyncFunc_Work()
		{
			var policyCollection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			int i = 0;
			async Task func1(PolicyResult<int> _, CancellationToken __) { await Task.Delay(1); i++; }
			async Task func2(PolicyResult<int> _) { await Task.Delay(1); i++; }
			policyCollection
				.AddPolicyResultHandlerForAll<int>(func1)
				.AddPolicyResultHandlerForAll<int>(func2);

			await policyCollection.HandleDelegateAsync<int>(() => throw new Exception("Test"));
			ClassicAssert.AreEqual(4, i);
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_IncludeErrorForAll_Work(string errorParamName)
		{
			var policyDelegateCollection = PolicyCollection
							.Create()
							.WithRetry(1)
							.WithRetry(1)
							.IncludeErrorForAll<ArgumentNullException>()
							.ToPolicyDelegateCollection(() => throw new ArgumentNullException(errorParamName));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.Any(pr => pr.Result.ErrorFilterUnsatisfied));
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeErrorForAll_Work()
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorForAll(ex => ex.Message == "Test1")
						.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.Any(pr => pr.Result.ErrorFilterUnsatisfied));
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase("")]
		public async Task Should_Generic_ExcludeErrorForAll_Work(string errorParamName)
		{
			var policyDelegateCollection = PolicyCollection
							.Create()
							.WithFallback(() => { })
							.WithRetry(1)
							.ExcludeErrorForAll<ArgumentNullException>()
							.ToPolicyDelegateCollection(() => throw new ArgumentNullException(errorParamName));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForAll_Work()
		{
			var policyDelegateCollection = PolicyCollection
							.Create()
							.WithFallback(() => { })
							.WithRetry(1)
							.ExcludeErrorForAll(ex => ex.Message == "Test1")
							.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsTrue(handleRes.PolicyDelegateResults.Select(phr => phr.Result).Any(pr => pr.ErrorFilterUnsatisfied));
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_Generic_IncludeErrorForLast_Work()
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorForLast<ArgumentException>()
						.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_IncludeErrorForLast_Work()
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorForLast(ex => ex.Message == "Test2")
						.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

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
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorSet<ArgumentException, ArgumentNullException>()
						.ToPolicyDelegateCollection(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_Generic_ExcludeErrorForLast_Work()
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.ExcludeErrorForLast<ArgumentException>()
						.ToPolicyDelegateCollection(() => throw new ArgumentException("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		public async Task Should_NoGeneric_ExcludeErrorForLast_Work()
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.ExcludeErrorForLast(ex => ex.Message == "Test1")
						.ToPolicyDelegateCollection(() => throw new Exception("Test1"));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.IsTrue(handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public async Task Should_ExcludeErrorSet_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.ExcludeErrorSet<ArgumentException, ArgumentNullException>()
						.ToPolicyDelegateCollection(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(false, false)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(true, true)]
		public void Should_HandleDelegate_Work(bool generic, bool crossSync)
		{
			var polCollection = PolicyCollection
								 .Create()
								 .WithRetry(1)
								 .WithRetry(1);

			int resultCount = 0;
			int resultCountIfNoError = 0;

			if (generic)
			{
				if (crossSync)
				{
					resultCount = polCollection.HandleDelegate<int>(async (_) => { await Task.Delay(1); throw new Exception("Test1"); }).Count();

					var resIfNoError = polCollection.HandleDelegate(async (_) => { await Task.Delay(1); return 1; });
					resultCountIfNoError = resIfNoError.Count();
					ClassicAssert.AreEqual(1, resIfNoError.Result);
				}
				else
				{
					resultCount = polCollection.HandleDelegate<int>(() => throw new Exception("Test1")).Count();

					var resIfNoError = polCollection.HandleDelegate(() => 1);
					resultCountIfNoError = resIfNoError.Count();
					ClassicAssert.AreEqual(1, resIfNoError.Result);
				}
			}
			else
			{
				if (crossSync)
				{
					resultCount = polCollection.HandleDelegate(async (_) => { await Task.Delay(1); throw new Exception("Test1");}).Count();
					resultCountIfNoError = polCollection.HandleDelegate(async (_) => await Task.Delay(1)).Count();
				}
				else
				{
					resultCount = polCollection.HandleDelegate(() => throw new Exception("Test1")).Count();
					resultCountIfNoError = polCollection.HandleDelegate(() => {}).Count();
				}
			}
			ClassicAssert.AreEqual(2, resultCount);
			ClassicAssert.AreEqual(1, resultCountIfNoError);
		}

		[Test]
		[TestCase(false, false)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(true, true)]
		public async Task Should_HandleDelegateAsync_Work(bool generic, bool crossSync)
		{
			var polCollection = PolicyCollection
							 .Create()
							 .WithRetry(1)
							 .WithRetry(1);

			int resultCount = 0;
			int resultCountIfNoError = 0;

			if (generic)
			{
				if (crossSync)
				{
					resultCount = (await polCollection.HandleDelegateAsync<int>(() => throw new Exception("Test1"))).Count();

					var resIfNoError = await polCollection.HandleDelegateAsync(() => 1);
					resultCountIfNoError = resIfNoError.Count();
					ClassicAssert.AreEqual(1, resIfNoError.Result);
				}
				else
				{
					resultCount = (await polCollection.HandleDelegateAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test1"); })).Count();

					var resIfNoError = await polCollection.HandleDelegateAsync(async (_) =>  {await Task.Delay(1); return 1;});
					resultCountIfNoError = resIfNoError.Count();
					ClassicAssert.AreEqual(1, resIfNoError.Result);
				}
			}
			else
			{
				if (crossSync)
				{
					resultCount = (await polCollection.HandleDelegateAsync(() => throw new Exception("Test1"))).Count();
					 resultCountIfNoError = (await polCollection.HandleDelegateAsync(() => {})).Count();
				}
				else
				{
					resultCount = (await polCollection.HandleDelegateAsync(async (_) => { await Task.Delay(1); throw new Exception("Test1");})).Count();
					resultCountIfNoError = (await polCollection.HandleDelegateAsync(async (_) => await Task.Delay(1))).Count();
				}
			}

			ClassicAssert.AreEqual(2, resultCount);
			ClassicAssert.AreEqual(1, resultCountIfNoError);
		}

		[Test]
		public void Should_AddPolicyResultHandlerForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast((_) => { }));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast((_, __) => { }));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast((_) => { }, CancellationType.Precancelable));

			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast((_) => Task.CompletedTask));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast((_, __) => Task.CompletedTask));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast((_) => Task.CompletedTask, CancellationType.Precancelable));

			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast<int>((_) => { }));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast<int>((_, __) => { }));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast<int>((_) => { }, CancellationType.Precancelable));

			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast<int>((_) => Task.CompletedTask));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast<int>((_, __) => Task.CompletedTask));
			ClassicAssert.DoesNotThrow(() => polCollection.AddPolicyResultHandlerForLast<int>((_) => Task.CompletedTask, CancellationType.Precancelable));

			ClassicAssert.DoesNotThrow(() => polCollection.SetPolicyResultFailedIf((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.SetPolicyResultFailedIf<int>((_) => true));
		}

		[Test]
		public void Should_ExcludeErrorForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorForLast((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorForLast<ArgumentException>());
		}

		[Test]
		public void Should_IncludeErrorForLast_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorForLast((_) => true));
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorForLast<ArgumentException>());
		}

		[Test]
		public void Should_IncludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.IncludeErrorSet<ArgumentException, ArgumentNullException>());
		}

		[Test]
		public void Should_ExcludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyCollection.Create();
			ClassicAssert.DoesNotThrow(() => polCollection.ExcludeErrorSet<ArgumentException, ArgumentNullException>());
		}

		[Test]
		public void Should_WithErrorProcessorOf_Not_Throw_For_EmptyCollection()
		{
			IErrorProcessorRegistration v = new PolicyCollectionErrorProcessorRegistration(true);

			v.WithErrorProcessorOf((Exception _, CancellationToken __) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __, CancellationToken ___) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1));
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1));
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1));
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1));
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessor(new DefaultErrorProcessor());
			ClassicAssert.AreEqual(0, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), CancellationType.Precancelable);
			ClassicAssert.AreEqual(0, v.Count);
		}

		[Test]
		public void Should_WithInnerErrorProcessor_HandleError_Correctly()
		{
			var innerProcessors = new InnerErrorProcessorFuncs();
			var policyCollection = new PolicyCollection().WithSimple();
			policyCollection
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable)
				.HandleDelegate(ActionWithInner);

			Assert.That(innerProcessors.I, Is.EqualTo(4));

			policyCollection
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken)
				.HandleDelegate(ActionWithInner);

			Assert.That(innerProcessors.J, Is.EqualTo(2));

			policyCollection
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable)
				.HandleDelegate(ActionWithInner);

			Assert.That(innerProcessors.K, Is.EqualTo(4));

			policyCollection
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken)
				.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken)
				.HandleDelegate(ActionWithInner);

			Assert.That(innerProcessors.L, Is.EqualTo(2));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_AddPolicyResultHandlerForLast_Work(bool generic)
		{
			var funcsProvider = new FuncsAndResultsProvider<PolicyResult>();
			var funcsProviderGen = new FuncsAndResultsProvider<PolicyResult<int>>();

			FuncsAndResultsProviderBase resBase = null;
			if (generic)
			{
				resBase = funcsProviderGen;
			}
			else
			{
				resBase = funcsProvider;
			}

			var polCollection = PolicyCollection.Create();

			for (int i = 0; i < 6; i++)
			{
				polCollection
								.WithRetry(1)
								.WithFallback((_) => { });
				switch (i)
				{
					case 0:
						if (generic)
						{
							polCollection.AddPolicyResultHandlerForLast<int>(funcsProviderGen.Act);
						}
						else
						{
							polCollection.AddPolicyResultHandlerForLast(funcsProvider.Act);
						}

						break;
					case 1:
						if (generic)
						{
							polCollection.AddPolicyResultHandlerForLast<int>(funcsProviderGen.ActionWithCancelType, CancellationType.Precancelable);
						}
						else
						{
							polCollection.AddPolicyResultHandlerForLast(funcsProvider.ActionWithCancelType, CancellationType.Precancelable);
						}

						break;
					case 2:
						if (generic)
						{
							polCollection.AddPolicyResultHandlerForLast<int>(funcsProviderGen.ActionWithCancellation);
						}
						else
						{
							polCollection.AddPolicyResultHandlerForLast(funcsProvider.ActionWithCancellation);
						}

						break;
					case 3:
						if (generic)
						{
							polCollection.AddPolicyResultHandlerForLast<int>(funcsProviderGen.Fun);
						}
						else
						{
							polCollection.AddPolicyResultHandlerForLast(funcsProvider.Fun);
						}

						break;
					case 4:
						if (generic)
						{
							polCollection.AddPolicyResultHandlerForLast<int>(funcsProviderGen.FuncWithCancelType, CancellationType.Precancelable);
						}
						else
						{
							polCollection.AddPolicyResultHandlerForLast(funcsProvider.FuncWithCancelType, CancellationType.Precancelable);
						}

						break;
					case 5:
						if (generic)
						{
							polCollection.AddPolicyResultHandlerForLast<int>(funcsProviderGen.FuncWithCancellation);
						}
						else
						{
							polCollection.AddPolicyResultHandlerForLast(funcsProvider.FuncWithCancellation);
						}

						break;
				}
			}
			if (generic)
				await polCollection.HandleDelegateAsync<int>(() => throw new Exception("Test"));
			else
				await polCollection.HandleDelegateAsync(() => throw new Exception("Test"));

			ClassicAssert.AreEqual(1, resBase.i);
			ClassicAssert.AreEqual(1, resBase.j);
			ClassicAssert.AreEqual(1, resBase.k);
			ClassicAssert.AreEqual(1, resBase.l);
			ClassicAssert.AreEqual(1, resBase.m);
			ClassicAssert.AreEqual(1, resBase.n);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_SetPolicyResultFailedIf_Work(bool generic, bool predicateTrue)
		{
			bool genericPredicate(PolicyResult<int> pr)
			{
				return pr.Errors.Any(e => e.Message == "Test");
			}

			bool predicate(PolicyResult pr)
			{
				return pr.Errors.Any(e => e.Message == "Test");
			}

			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithFallback((_) => { });

			PolicyDelegateCollectionResultBase polDelegateCollectionResult = null;

			if (generic)
			{
				polDelegateCollectionResult = polCollection.SetPolicyResultFailedIf<int>(genericPredicate)
							 .HandleDelegate<int>(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"));
			}
			else
			{
				polDelegateCollectionResult = polCollection.SetPolicyResultFailedIf(predicate)
							 .HandleDelegate(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"));
			}
			Assert.That(polDelegateCollectionResult.IsFailed, Is.EqualTo(predicateTrue));
			Assert.That(polDelegateCollectionResult.LastPolicyResultFailedReason, Is.EqualTo(predicateTrue ? PolicyResultFailedReason.PolicyResultHandlerFailed : PolicyResultFailedReason.None));
		}

		private class FuncsAndResultsProviderBase
		{
			public int i;
			public int j;
			public int k;
			public int l;
			public int m;
			public int n;
		}

		private class FuncsAndResultsProvider<TPolicyResult> : FuncsAndResultsProviderBase where TPolicyResult : PolicyResult
		{
			public void Act(TPolicyResult pr)
				{
					i++;
					pr.SetFailed();
				}

			public void ActionWithCancelType(TPolicyResult pr)
				{
					j++;
					pr.SetFailed();
				}

#pragma warning disable S1172 // Unused method parameters should be removed
			public void ActionWithCancellation(TPolicyResult pr, CancellationToken _)
#pragma warning restore S1172 // Unused method parameters should be removed
			{
					k++;
					pr.SetFailed();
				}

			public async Task Fun(TPolicyResult pr)
				{
					l++;
					await Task.Delay(1);
					pr.SetFailed();
				}

			public async Task FuncWithCancelType(TPolicyResult pr)
				{
					m++;
					await Task.Delay(1);
					pr.SetFailed();
				}

#pragma warning disable S1172 // Unused method parameters should be removed
			public async Task FuncWithCancellation(TPolicyResult pr, CancellationToken _)
#pragma warning restore S1172 // Unused method parameters should be removed
			{
					n++;
					await Task.Delay(1);
					pr.SetFailed();
				}
		}
	}
}
