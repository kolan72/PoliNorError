﻿using NUnit.Framework;
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
		public void Should_WithRetry_WithRetryDelay_Adds_Policy_Correctly()
		{
			var delay = new ConstantRetryDelay(TimeSpan.FromSeconds(1));
			var policyCollection = PolicyCollection.Create().WithRetry(1, delay);
			ClassicAssert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.AreEqual(1, ((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.RetryCount);
			ClassicAssert.AreEqual(delay, ((RetryPolicy)policyCollection.FirstOrDefault()).Delay);
		}

		[Test]
		public void Should_WithInfiniteRetry_WithRetryDelay_Adds_Policy_Correctly()
		{
			var delay = new ConstantRetryDelay(TimeSpan.FromSeconds(1));
			var policyCollection = PolicyCollection.Create().WithInfiniteRetry(delay);
			ClassicAssert.IsInstanceOf<RetryPolicy>(policyCollection.FirstOrDefault());
			ClassicAssert.AreEqual(true, ((RetryPolicy)policyCollection.FirstOrDefault()).RetryInfo.IsInfinite);
			ClassicAssert.AreEqual(delay, ((RetryPolicy)policyCollection.FirstOrDefault()).Delay);
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
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public async Task Should_IncludeErrorSet_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorSet(errorSet)
						.ToPolicyDelegateCollection(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false, true)]
		[TestCase(TestErrorSetMatch.NoMatch, true, false)]
		[TestCase(TestErrorSetMatch.FirstParam, false, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false, false)]
		public async Task Should_IncludeErrorSet_ForInnerExceptions_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed, bool consistsOfErrorAndInnerError)
		{
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.IncludeErrorSet(errorSet)
						.ToPolicyDelegateCollection(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, null, true));

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
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public async Task Should_ExcludeErrorSet_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.ExcludeErrorSet(errorSet)
						.ToPolicyDelegateCollection(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch));

			var handleRes = await policyDelegateCollection.HandleAllAsync();
			ClassicAssert.IsFalse(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(isFailed, handleRes.LastPolicyResult.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(0, handleRes.PolicyDelegatesUnused.Count());
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true, true)]
		[TestCase(TestErrorSetMatch.NoMatch, false, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true, false)]
		[TestCase(TestErrorSetMatch.SecondParam, true, false)]
		public async Task Should_ExcludeErrorSet_ForInnerExceptions_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool isFailed, bool consistsOfErrorAndInnerError)
		{
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			var policyDelegateCollection = PolicyCollection
						.Create()
						.WithRetry(1)
						.WithRetry(1)
						.ExcludeErrorSet(errorSet)
						.ToPolicyDelegateCollection(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, null, true));

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
			var policyCollection = PolicyCollection
					.Create()
					.WithRetry(1)
					.WithRetry(1);

			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policyCollection.IncludeInnerError<TestInnerException>();
			}
			else
			{
				policyCollection.IncludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			var handleRes = await policyCollection.ToPolicyDelegateCollection(actionToHandle).HandleAllAsync();
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
			var policyCollection = PolicyCollection
					.Create()
					.WithRetry(1)
					.WithRetry(1);

			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				policyCollection.ExcludeInnerError<TestInnerException>();
			}
			else
			{
				policyCollection.ExcludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			var actionToHandle = TestHandlingForInnerError.GetAction(withInnerError, satisfyFilterFunc);
			var handleRes = await policyCollection.ToPolicyDelegateCollection(actionToHandle).HandleAllAsync();
			Assert.That(handleRes.PolicyDelegateResults.FirstOrDefault().Result.ErrorFilterUnsatisfied, Is.False);
			TestHandlingForInnerError.PolicyResultAsserts.AfterHandlingWithExcludeInnerErrorFilter(handleRes.LastPolicyResult, withInnerError, satisfyFilterFunc);
			Assert.That(handleRes.PolicyDelegatesUnused.Count(), Is.EqualTo(0));
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
					resultCount = polCollection.HandleDelegate(async (_) => { await Task.Delay(1); throw new Exception("Test1"); }).Count();
					resultCountIfNoError = polCollection.HandleDelegate(async (_) => await Task.Delay(1)).Count();
				}
				else
				{
					resultCount = polCollection.HandleDelegate(() => throw new Exception("Test1")).Count();
					resultCountIfNoError = polCollection.HandleDelegate(() => { }).Count();
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

					var resIfNoError = await polCollection.HandleDelegateAsync(async (_) => { await Task.Delay(1); return 1; });
					resultCountIfNoError = resIfNoError.Count();
					ClassicAssert.AreEqual(1, resIfNoError.Result);
				}
			}
			else
			{
				if (crossSync)
				{
					resultCount = (await polCollection.HandleDelegateAsync(() => throw new Exception("Test1"))).Count();
					resultCountIfNoError = (await polCollection.HandleDelegateAsync(() => { })).Count();
				}
				else
				{
					resultCount = (await polCollection.HandleDelegateAsync(async (_) => { await Task.Delay(1); throw new Exception("Test1"); })).Count();
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
			Assert.DoesNotThrow(() => polCollection.IncludeErrorSet<ArgumentException, ArgumentNullException>());

			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			Assert.DoesNotThrow(() => polCollection.IncludeErrorSet(errorSet));
		}

		[Test]
		public void Should_ExcludeErrorSet_Not_Throw_For_EmptyCollection()
		{
			var polCollection = PolicyCollection.Create();
			Assert.DoesNotThrow(() => polCollection.ExcludeErrorSet<ArgumentException, ArgumentNullException>());

			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			Assert.DoesNotThrow(() => polCollection.ExcludeErrorSet(errorSet));
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
		[TestCase(FallbackTypeForTests.WithAction, true, true, true)]
		[TestCase(FallbackTypeForTests.WithAction, false, true, true)]
		[TestCase(FallbackTypeForTests.WithAction, true, false, false)]
		[TestCase(FallbackTypeForTests.WithAction, false, false, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, true, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false, true, true)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, true, false, false)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, false, false, false)]
		public void Should_WithFallback_Set_OnlyGenericFallbackForGenericDelegate_Correctly(FallbackTypeForTests fallbackType, bool withCancelTokenParam, bool paramValue, bool propertyValue)
		{
			var polCollection = PolicyCollection.Create();
			switch (fallbackType)
			{
				case FallbackTypeForTests.WithAction:
					if (withCancelTokenParam)
					{
						polCollection = polCollection.WithFallback((_) => { }, paramValue);
					}
					else
					{
						polCollection = polCollection.WithFallback(() => { }, paramValue);
					}
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					if (withCancelTokenParam)
					{
						polCollection = polCollection.WithFallback(async (_) => await Task.Delay(1), paramValue);
					}
					else
					{
						polCollection = polCollection.WithFallback(async () => await Task.Delay(1), paramValue);
					}
					break;
			}

			Assert.That(((FallbackPolicy)polCollection.Last()).OnlyGenericFallbackForGenericDelegate, Is.EqualTo(propertyValue));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_SetPolicyResultFailedIf_Work(bool generic, bool predicateTrue)
		{
			var polCollection = PolicyCollection.Create()
								.WithRetry(1)
								.WithFallback((_) => { });

			PolicyDelegateCollectionResultBase polDelegateCollectionResult = null;

			if (generic)
			{
				polDelegateCollectionResult = polCollection.SetPolicyResultFailedIf<int>(PredicateFuncsForTests.GenericPredicate)
							 .HandleDelegate<int>(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"));
			}
			else
			{
				polDelegateCollectionResult = polCollection.SetPolicyResultFailedIf(PredicateFuncsForTests.Predicate)
							 .HandleDelegate(() => throw new ArgumentException(predicateTrue ? "Test" : "Test2"));
			}
			Assert.That(polDelegateCollectionResult.IsFailed, Is.EqualTo(predicateTrue));
			Assert.That(polDelegateCollectionResult.LastPolicyResultFailedReason, Is.EqualTo(predicateTrue ? PolicyResultFailedReason.PolicyResultHandlerFailed : PolicyResultFailedReason.None));
		}

		[Test]
		public void Should_WithFallback_With_FallbackFuncsProvider_Arg_Work()
		{
			var polCollection = PolicyCollection.Create()
								.WithFallback(FallbackFuncsProvider
												.Create(async (_) => await Task.Delay(1), (_) => { }, true)
												.AddOrReplaceAsyncFallbackFunc(async (_) => { await Task.Delay(1); return 1; })
												.AddOrReplaceFallbackFunc((_) => 1)
												);
			var lastPolicy = ((FallbackPolicyBase)polCollection.LastOrDefault());
			Assert.That(lastPolicy.HasAsyncFallbackFunc(), Is.True);
			Assert.That(lastPolicy.HasFallbackAction(), Is.True);
			Assert.That(lastPolicy.HasAsyncFallbackFunc<int>(), Is.True);
			Assert.That(lastPolicy.HasFallbackFunc<int>(), Is.True);
			Assert.That(lastPolicy.OnlyGenericFallbackForGenericDelegate, Is.True);
		}

		[Test]
		public void Should_Imitate_RetryPolicy_Using_Collection_Of_SimplePolicies()
		{
			bool firstHandlerFlag = false;
			bool secondHandlerFlag = false;

			void act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection.Create();
			var result = polCollection
				.WithSimple()
				.AddPolicyResultHandlerForLast(async (_) => { await Task.Delay(TimeSpan.FromTicks(2)); firstHandlerFlag = true; })
				.WithSimple()
				.AddPolicyResultHandlerForLast(async (_) => { await Task.Delay(TimeSpan.FromTicks(4)); secondHandlerFlag = true; })
				.WithSimple()
				.AddPolicyResultHandlerForAll(pr => { if (!pr.NoError) pr.SetFailed(); })
				.HandleDelegate(act);

			Assert.That(firstHandlerFlag, Is.True);
			Assert.That(secondHandlerFlag, Is.True);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.PolicyDelegateResults.Count, Is.EqualTo(3));
		}

		[Test]
		public void Should_Imitate_RetryPolicy_WithConstantRetryDelay_Using_Collection_Of_SimplePolicies()
		{
			void act() => throw new InvalidOperationException();
			var counter = 0;

			var polCollection = PolicyCollection.Create();
			var result = polCollection
				.WithSimple()
				.WithSimple()
				.WithSimple()
				.AddPolicyResultHandlerForAll(pr => { if (!pr.NoError) pr.SetFailed(); })
				.AddPolicyResultHandlerForAll(_ =>  { Task.Delay(TimeSpan.FromTicks(1)).GetAwaiter().GetResult(); counter++; }, true)
				.HandleDelegate(act);

			Assert.That(counter, Is.EqualTo(2));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.PolicyDelegateResults.Count, Is.EqualTo(3));
		}

		[Test]
		public void Should_Imitate_RetryPolicy_Using_Collection_Of_SimplePolicies_WithErrorProcessors()
		{
			bool firstHandlerFlag = false;
			bool secondHandlerFlag = false;

			void act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection.Create();
			var result = polCollection
				.WithSimple()
				.WithErrorProcessorOf(async (_) => { await Task.Delay(TimeSpan.FromTicks(2)); firstHandlerFlag = true; })
				.WithSimple()
				.WithErrorProcessorOf(async (_) => { await Task.Delay(TimeSpan.FromTicks(4)); secondHandlerFlag = true; })
				.WithSimple()
				.AddPolicyResultHandlerForAll(pr => { if (!pr.NoError) pr.SetFailed(); })
				.HandleDelegate(act);

			Assert.That(firstHandlerFlag, Is.True);
			Assert.That(secondHandlerFlag, Is.True);
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.PolicyDelegateResults.Count, Is.EqualTo(3));
		}

		[Test]
		public void Should_Imitate_RetryPolicy_WithConstantDelay_Using_Repeated_SimplePolicy()
		{
			void act() => throw new InvalidOperationException();
			var retries = 0;

			var sp = new SimplePolicy()
				.SetPolicyResultFailedIf((pr) => !pr.NoError)
				.AddPolicyResultHandler(async (_) => { await Task.Delay(TimeSpan.FromTicks(1)); retries++; });

			var polCollection = PolicyCollection
								.Create(sp, 2)
								.WithSimple()
								.SetPolicyResultFailedIf((pr) => !pr.NoError);

			var result = polCollection
				.HandleDelegate(act);

			Assert.That(retries, Is.EqualTo(2));
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.PolicyDelegateResults.Count, Is.EqualTo(3));
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(false, false, false)]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		public void Should_Add_ActionBased_PolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy, bool? withCancellationType)
		{
			void act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			if (withCancellationType == false)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.ActWithPRParam, excludeLastPolicy);
			}
			else if (withCancellationType == true)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.ActWithPRParam, CancellationType.Precancelable, excludeLastPolicy);
			}

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(false, false, false)]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		public void Should_Add_ActionBased_GenericPolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy, bool? withCancellationType)
		{
			int act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			if (withCancellationType == false)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.ActWithGenericPRParam, excludeLastPolicy);
			}
			else if (withCancellationType == true)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.ActWithGenericPRParam, CancellationType.Precancelable, excludeLastPolicy);
			}

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(false, false, false)]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		public void Should_Add_FuncBased_GenericPolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy, bool? withCancellationType)
		{
			int act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			if (withCancellationType == false)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.FuncWithGenericPRParam, excludeLastPolicy);
			}
			else if (withCancellationType == true)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.FuncWithGenericPRParam, CancellationType.Precancelable, excludeLastPolicy);
			}

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_Add_ActionWithTokenBased_PolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy)
		{
			void act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.ActWithPRAndTokenParams, excludeLastPolicy);

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_Add_ActionWithTokenBased_GenericPolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy)
		{
			int act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.ActWithGenericPRAndTokenParams, excludeLastPolicy);

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_Add_FuncWithTokenBased_GenericPolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy)
		{
			int act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.FuncWithGenericPRAndTokenParams, excludeLastPolicy);

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_Add_FuncWithTokenBased_PolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy)
		{
			void act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.FuncWithPRAndTokenParams, excludeLastPolicy);

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		[Test]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(false, false, false)]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		public void Should_Add_FuncBased_PolicyResultHandler_For_All_Policies(bool excludeLastPolicy, bool onePolicy, bool? withCancellationType)
		{
			void act() => throw new InvalidOperationException();

			var polCollection = PolicyCollection
								.Create()
								.WithRetry(1);

			if (!onePolicy)
			{
				polCollection.WithRetry(1);
			}

			var handlerProvider = new PolicyResultHandlerProvider();

			if (withCancellationType == false)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.FuncWithPRParam, excludeLastPolicy);
			}
			else if (withCancellationType == true)
			{
				polCollection
				.AddPolicyResultHandlerForAll(handlerProvider.FuncWithPRParam, CancellationType.Precancelable, excludeLastPolicy);
			}

			var result = polCollection
				.HandleDelegate(act);

			if (excludeLastPolicy)
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 0 : 1));
			}
			else
			{
				Assert.That(handlerProvider.HandlersCounter, Is.EqualTo(onePolicy ? 1 : 2));
			}
			Assert.That(result.PolicyDelegatesUnused.Count(), Is.Zero);
			Assert.That(result.IsFailed, Is.True);

			Assert.That(handlerProvider.FirstHandlerFlag, Is.EqualTo(!onePolicy || !excludeLastPolicy));
			Assert.That(handlerProvider.SecondHandlerFlag, Is.EqualTo(!onePolicy && !excludeLastPolicy));
		}

		private class PolicyResultHandlerProvider
		{
			public Action<PolicyResult> ActWithPRParam => (_) => Core();

			public Action<PolicyResult<int>> ActWithGenericPRParam => (_) => Core();

			public Action<PolicyResult<int>, CancellationToken> ActWithGenericPRAndTokenParams => (_, __) => Core();

			public Action<PolicyResult, CancellationToken> ActWithPRAndTokenParams => (_, __) => Core();

			public Func<PolicyResult<int>, CancellationToken, Task> FuncWithGenericPRAndTokenParams => async (_, __) => { await Task.Delay(TimeSpan.FromTicks(1)); Core(); };

			public Func<PolicyResult, CancellationToken, Task> FuncWithPRAndTokenParams => async (_, __) => { await Task.Delay(TimeSpan.FromTicks(1)); Core(); };

			public Func<PolicyResult, Task> FuncWithPRParam => async (_) => { await Task.Delay(TimeSpan.FromTicks(1)); Core(); };

			public Func<PolicyResult<int>, Task> FuncWithGenericPRParam => async (_) => { await Task.Delay(TimeSpan.FromTicks(1)); Core(); };

			public bool FirstHandlerFlag { get; private set; }
			public bool SecondHandlerFlag { get; private set; }

			public int HandlersCounter { get; private set; }

			private Action Core => () =>
			{
				if (HandlersCounter == 0)
				{
					FirstHandlerFlag = true;
				}
				else
				{
					SecondHandlerFlag = true;
				}
				HandlersCounter++;
			};
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
