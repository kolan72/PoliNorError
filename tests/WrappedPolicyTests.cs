using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class WrappedPolicyTests
    {
		[Test]
		public async Task Should_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			async Task func(CancellationToken _) => await Task.Delay(1);
			var retry = new RetryPolicy(1);

			var subsPolicy = Substitute.For<IPolicyBase>();
			subsPolicy.HandleAsync(func, default, default).Returns(Task.FromResult(new PolicyResult()));
			retry.WrapPolicy(subsPolicy);

			var outPolicyResult = await retry.HandleAsync(func, default);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			await subsPolicy.Received(1).HandleAsync(func, default, default);
		}

		[Test]
		public async Task Should_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var retry = new RetryPolicy(2);

			async Task func(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			var subsPolicy = Substitute.For<IPolicyBase>();

			var polResult = new PolicyResult();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.HandleAsync(func, default, default).Returns(Task.FromResult(polResult));
			retry.WrapPolicy(subsPolicy);

			var outPolicyResult = await retry.HandleAsync(func, default);
			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

			await subsPolicy.Received(3).HandleAsync(func, default, default);
		}

		[Test]
		public void Should_Handle_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			void act()
			{
				// Method intentionally left empty.
			}

			var retry = new RetryPolicy(1);

			var subsPolicy = Substitute.For<IPolicyBase>();

			subsPolicy.Handle(act, default).Returns(new PolicyResult());
			retry.WrapPolicy(subsPolicy);

			var outPolicyResult = retry.Handle(act, default);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			subsPolicy.Received(1).Handle(act, default);
		}

		[Test]
		public void Should_Handle_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var outPolicy = new RetryPolicy(2);

			var subsPolicy = Substitute.For<IPolicyBase>();

			void act() => throw new Exception();

			var polResult = new PolicyResult();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act, default).Returns(polResult);
			outPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = outPolicy.Handle(act, default);

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

			subsPolicy.Received(3).Handle(act, default);
		}

		[Test]
		public void Should_HandleT_CallWrappedPolicy_When_Wrapped_And_Error_ForFallback()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithFallbackFunc((_) => ++i);

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			int act() => throw new Exception();

			var polResult = new PolicyResult<int>();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act, cancelToken).Returns(polResult);
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = fallback.Handle(act, cancelToken);

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			ClassicAssert.IsFalse(outPolicyResult.IsFailed);
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count());

			subsPolicy.Received(1).Handle(act, cancelToken);
		}

		[Test]
		public void Should_HandleT_CallWrappedPolicy_When_Wrapped_And_Error_ForRetry()
		{
			var outPolicy = new RetryPolicy(2);

			var subsPolicy = Substitute.For<IPolicyBase>();

			int act() => throw new Exception();

			var polResult = new PolicyResult<int>();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act, default).Returns(polResult);
			outPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = outPolicy.Handle(act, default);

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

			subsPolicy.Received(3).Handle(act, default);
			ClassicAssert.AreEqual(outPolicy.PolicyName, outPolicyResult.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Error_ForFallback()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return ++i; });

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			async Task<int> func(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			var polResult = new PolicyResult<int>();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.HandleAsync(func, default, cancelToken).Returns(Task.FromResult(polResult));
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = await fallback.HandleAsync(func, false, cancelToken);

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			ClassicAssert.AreEqual(false, outPolicyResult.IsFailed);
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count());

			await subsPolicy.Received(1).HandleAsync(func, default, cancelToken);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Error_ForRetry()
		{
			var outPolicy = new RetryPolicy(2);

			var subsPolicy = Substitute.For<IPolicyBase>();
			Task<int> act(CancellationToken _) => throw new Exception();

			var polResult = PolicyResult<int>.ForNotSync();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.HandleAsync(act, default, default).Returns(Task.FromResult(polResult));
			outPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = await outPolicy.HandleAsync(act, default(CancellationToken));

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

			await subsPolicy.Received(3).HandleAsync(act, default, default);
		}

		[Test]
		public async Task Should_OuterPolicyResult_Canceled_ForHandleTAsync_If_Inner_Canceled()
		{
			var cts = new CancellationTokenSource();
			async Task<int> f(CancellationToken _)
			{
				await Task.Delay(1);
				cts.Cancel();
				throw new Exception("Test");
			}
			int i = 0;
			var wrapppedPolicy = new RetryPolicy(2).WithWait(TimeSpan.FromSeconds(1));
			int fallback(CancellationToken _) => i = 10;
			var fallBackPolicy = new FallbackPolicy().WithFallbackFunc(fallback);

			fallBackPolicy.WrapPolicy(wrapppedPolicy);
			var polResult = await fallBackPolicy.HandleAsync(f, cts.Token);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.IsFailed);

			ClassicAssert.IsTrue(polResult.WrappedPolicyResults.FirstOrDefault().Result.IsCanceled);
			ClassicAssert.IsTrue(polResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			ClassicAssert.AreEqual(0, i);
			cts.Dispose();
		}

		[Test]
		public async Task Should_PolicyResult_Be_Canceled_And_Failed_If_Cancellation_In_WrappedPolicy_For_HandleAsync()
		{
			int i = 0;
			CancellationTokenSource source = new CancellationTokenSource();

			async Task func(CancellationToken _)
			{
				await Task.Delay(1);
				if (i == 0)
				{
					i++;
					throw new Exception("Test");
				}
				source.Cancel();
				throw new TimeoutException("TestTimeout");
			}

			var wrapppedPolicy = new RetryPolicy(2).ExcludeError<TimeoutException>();
			var fallBackPolicy = new FallbackPolicy().WithFallbackAction((_) => i = 10);

			fallBackPolicy.WrapPolicy(wrapppedPolicy);

			var polResult = await fallBackPolicy.HandleAsync(func, source.Token);

			ClassicAssert.AreEqual(1, i);
			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());

			source.Dispose();
		}

		[Test]
		public async Task Should_OuterPolicyResult_Canceled_ForHandleAsync_If_Handling_Canceled()
		{
			var cts = new CancellationTokenSource();
			async Task f(CancellationToken _)
			{
				await Task.Delay(1);
				cts.Cancel();
				throw new Exception("Test");
			}
			int i = 0;
			var wrapppedPolicy = new RetryPolicy(2);
			void fallback(CancellationToken _) => i = 10;
			var fallBackPolicy = new FallbackPolicy().WithFallbackAction(fallback);

			fallBackPolicy.WrapPolicy(wrapppedPolicy);
			var polResult = await fallBackPolicy.HandleAsync(f, cts.Token);
			ClassicAssert.IsTrue(polResult.IsCanceled);
			ClassicAssert.IsTrue(polResult.IsFailed);

			ClassicAssert.IsTrue(polResult.WrappedPolicyResults.FirstOrDefault().Result.IsCanceled);
			ClassicAssert.AreEqual(0, i);
			cts.Dispose();
		}

		[Test]
		public void Should_Retry_CallWrappedFallbackPolicy_And_Handle_With_Success()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => { });
			int i = 0;
			void retryAct()
			{
				i++;
				throw new Exception();
			}
			var retryPol = new RetryPolicy(3);
			retryPol.WrapPolicy(fallbackPolicy);
			var outPolicyResult = retryPol.Handle(retryAct);
			ClassicAssert.IsFalse(outPolicyResult.IsFailed);
		}

		[Test]
		public void Should_OutPolicyResult_Contains_Errors_Of_PolicyResultHandlerFailedException_Type_When_Inner_PolicyResult_Was_SetFaiiled_By_PolicyResultHandler()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => { }).AddPolicyResultHandler((pr) => pr.SetFailed());
			int i = 0;
			void retryAct()
			{
				i++;
			}
			var retryPol = new RetryPolicy(3);
			retryPol.WrapPolicy(fallbackPolicy);
			var outPolicyResult = retryPol.Handle(retryAct);
			ClassicAssert.IsTrue(outPolicyResult.IsFailed);
			ClassicAssert.IsTrue(outPolicyResult.Errors.All(err => err.GetType().Equals(typeof(PolicyResultHandlerFailedException))));
		}

		[Test]
		public void Should_Retry_CallWrappedFallbackPolicy_And_Handle_With_Success_Work_WinGenericFunc()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackFunc((_) => 1);
			int i = 0;
			int retryAct()
			{
				i++;
				throw new Exception();
			}
			var retryPol = new RetryPolicy(3);
			retryPol.WrapPolicy(fallbackPolicy);
			var outPolicyResult = retryPol.Handle(retryAct);
			ClassicAssert.IsFalse(outPolicyResult.IsFailed);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.First().Result.Result);
		}

		[Test]
		public async Task Should_Fallback_HandleAsyncT_For_RetryPolicyWrappedByFallback_Policy_Work()
		{
			int i = 0;
			async Task<int> func(CancellationToken _)
			{
				await Task.Delay(1);
				if (i == 0)
				{
					i++;
					throw new Exception("Test");
				}
				throw new TimeoutException("TestTimeout");
			}

			var wrapppedPolicy = new RetryPolicy(2).ExcludeError<TimeoutException>();
			var fallBackPolicy = new FallbackPolicy().WithFallbackFunc((_) => { i = 10; return i; });

			fallBackPolicy.WrapPolicy(wrapppedPolicy);
			var polResult = await fallBackPolicy.HandleAsync(func);

			ClassicAssert.AreEqual(10, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
			ClassicAssert.AreEqual(10, polResult.Result);
			ClassicAssert.AreEqual(fallBackPolicy.PolicyName, polResult.PolicyName);
		}

		[Test]
		public void Should_HandleT_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithFallbackFunc((_) => ++i);

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			int act() => 56;

			subsPolicy.Handle(act, cancelToken).Returns(new PolicyResult<int>());
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = fallback.Handle(act);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			subsPolicy.Received(1).Handle(act, cancelToken);
		}

		[Test]
		public async Task Should_Fallback_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var retry = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));

			var subsPolicy = Substitute.For<IPolicyBase>();

			async Task func(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			var polResult = PolicyResult.ForNotSync();
			polResult.SetFailedInner();

			subsPolicy.HandleAsync(func, default, default).Returns(Task.FromResult(polResult));
			retry.WrapPolicy(subsPolicy);

			var outPolicyResult = await retry.HandleAsync(func, default(CancellationToken));
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(true, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			await subsPolicy.Received(1).HandleAsync(func, default, default);
		}

		[Test]
		public async Task Should_Fallback_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var fallBackPol = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));

			var subsPolicy = Substitute.For<IPolicyBase>();

			async Task func(CancellationToken _) => await Task.Delay(1);

			subsPolicy.HandleAsync(func, default, default).Returns(Task.FromResult(new PolicyResult()));
			fallBackPol.WrapPolicy(subsPolicy);

			var outPolicyResult = await fallBackPol.HandleAsync(func, default(CancellationToken));
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			await subsPolicy.Received(1).HandleAsync(func, default, default);
		}

		[Test]
		public async Task Should_Fallback_HandleAsync_For_RetryPolicyWrappedByFallback_Policy_Work()
		{
			int i = 0;
			async Task func(CancellationToken _)
			{
				await Task.Delay(1);
				if (i == 0)
				{
					i++;
					throw new Exception("Test");
				}
				throw new TimeoutException("TestTimeout");
			}

			var wrapppedPolicy = new RetryPolicy(2).ExcludeError<TimeoutException>();
			var fallBackPolicy = new FallbackPolicy().WithFallbackAction((_) => i = 10);

			fallBackPolicy.WrapPolicy(wrapppedPolicy);
			var polResult = await fallBackPolicy.HandleAsync(func);

			ClassicAssert.AreEqual(10, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
			ClassicAssert.AreEqual(fallBackPolicy.PolicyName, polResult.PolicyName);
		}

		[Test]
		public void Should_Fallback_Handle_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => { });

			var subsPolicy = Substitute.For<IPolicyBase>();

			void act()
			{
				// Method intentionally left empty.
			}

			subsPolicy.Handle(act, default).Returns(new PolicyResult());
			fallbackPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = fallbackPolicy.Handle(act);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			subsPolicy.Received().Handle(act, default);
		}

		[Test]
		public void Should_Fallback_Handle_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => { });

			var subsPolicy = Substitute.For<IPolicyBase>();

			void act() => throw new Exception();

			var polResult = PolicyResult.ForSync();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act, default).Returns(polResult);
			fallbackPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = fallbackPolicy.Handle(act, default);

			//Out policy error should be the same as wrapped policy error.
			ClassicAssert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			ClassicAssert.AreEqual(false, outPolicyResult.IsFailed);

			subsPolicy.Received(1).Handle(act, default);
		}

		[Test]
		public void Should_Fallback_Handle_For_RetryPolicyWrappedByFallbackPolicy()
		{
			int i = 0;
			void act()
			{
				if (i == 0)
				{
					i++;
					throw new Exception("Test");
				}
				throw new TimeoutException("TestTimeout");
			}

			var wrapppedPolicy = new RetryPolicy(2).ExcludeError<TimeoutException>();
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => i = 10);

			fallbackPolicy.WrapPolicy(wrapppedPolicy);
			var polResult = fallbackPolicy.Handle(act);

			ClassicAssert.AreEqual(10, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.IsTrue(polResult.IsSuccess);
			ClassicAssert.IsFalse(polResult.NoError);
			ClassicAssert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
		}

		[Test]
		public void Should_Fallback_HandleT_For_RetryPolicyWrappedByFallback_Policy()
		{
			int i = 0;
			int act()
			{
				if (i <= 1)
				{
					i++;
					throw new Exception("Test");
				}
				throw new TimeoutException("TestTimeout");
			}

			var wrapppedRetryPolicy = new RetryPolicy(3).ExcludeError<TimeoutException>();
			var fallBackPolicy = new FallbackPolicy().WithFallbackFunc((_) => { i = 10; return i; });

			fallBackPolicy.WrapPolicy(wrapppedRetryPolicy);
			var polResult = fallBackPolicy.Handle(act);

			ClassicAssert.AreEqual(10, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.AreEqual(3, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
			Assert.That(polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.OfType<TimeoutException>().Count(), Is.EqualTo(1));
			ClassicAssert.AreEqual(10, polResult.Result);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return ++i; });

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			Task<int> act(CancellationToken _) => Task.FromResult(56);

			subsPolicy.HandleAsync(act, default, cancelToken).Returns(Task.FromResult(new PolicyResult<int>()));
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = await fallback.HandleAsync(act, cancelToken);
			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			await subsPolicy.Received(1).HandleAsync(act, default, cancelToken);
		}

		[Test]
		public void Should_WrapUp_Returns_OuterPolicy_That_Can_Be_Handled()
		{
			var subsPolicy = Substitute.For<IPolicyBase>();

			void act() => throw new Exception();

			var polResult = PolicyResult.ForSync();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act).Returns(polResult);

			const string SIMPLE_WRAPPER_POLICY = "SimpleWrapperPolicy";

			var outPolicyResult = subsPolicy.WrapUp(new SimplePolicy().WithPolicyName(SIMPLE_WRAPPER_POLICY)).OuterPolicy.Handle(act);
			subsPolicy.Received(1).Handle(act);

			ClassicAssert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(SIMPLE_WRAPPER_POLICY, outPolicyResult.PolicyName);
		}

		[Test]
		public void Should_OuterPolicy_After_Double_WrapUp_Can_Be_Handled()
		{
			var subsPolicy = Substitute.For<IPolicyBase>();

			void act() => throw new Exception();

			var polResult = PolicyResult.ForSync();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act).Returns(polResult);

			const string WRAPPER_POLICY_1 = "WrapperPolicy1";
			const string WRAPPER_POLICY_2 = "WrapperPolicy2";

			var outPolicyResult = subsPolicy.WrapUp(new RetryPolicy(1).WithPolicyName(WRAPPER_POLICY_1))
											.OuterPolicy
											.WrapUp(new RetryPolicy(1).WithPolicyName(WRAPPER_POLICY_2))
											.OuterPolicy
											.Handle(act);
			subsPolicy.Received(4).Handle(act);
			ClassicAssert.AreEqual(2, outPolicyResult.WrappedPolicyResults.Count());
			ClassicAssert.AreEqual(WRAPPER_POLICY_2, outPolicyResult.PolicyName);
			ClassicAssert.AreEqual(WRAPPER_POLICY_1, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.PolicyName);
		}

		[Test]
		public void Should_WrapUp_By_NullPolicy_Throw()
		{
			var subsPolicy = Substitute.For<IPolicyBase>();
			ClassicAssert.Throws<ArgumentNullException>(() => subsPolicy.WrapUp<SimplePolicy>(null));
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(true, true)]
		[TestCase(false, false)]
		[TestCase(false, true)]
		public void Should_ResetWrap_Really_Reset(bool sync, bool generic)
		{
			var wrappedPolicy = new SimplePolicy().WrapUp(new FallbackPolicy()).OuterPolicy;

			Action act =() => throw new Exception();
			Func<int> func = () => throw new Exception();
			Func<CancellationToken, Task> asyncAct = async (_) => await Task.Delay(1);
			Func<CancellationToken, Task<int>> asyncFuncT = async (_) => { await Task.Delay(1); return 1; };

			PolicyWrapperBase wrapper = null;

			Action actWrapped = null;
			Func<int> funcWrapped = null;
			Func<CancellationToken, Task> asyncActWrapped = null;
			Func<CancellationToken, Task<int>> asyncFuncTWrapped = null;

			if (sync)
			{
				RunSync(false);
			}
			else
			{
				RunASync(false);
			}

			wrappedPolicy.ResetWrap();

			if (sync)
			{
				RunSync(true);
			}
			else
			{
				RunASync(true);
			}

			void RunSync(bool res)
			{
				if (!generic)
				{
					(actWrapped, wrapper) = wrappedPolicy.WrapDelegateIfNeed(act, default);
					ClassicAssert.AreEqual(res, act.Equals(actWrapped));
				}
				else
				{
					(funcWrapped, wrapper) = wrappedPolicy.WrapDelegateIfNeed(func, default);
					ClassicAssert.AreEqual(res, func.Equals(funcWrapped));
				}
				ClassicAssert.AreEqual(res, wrapper == null);
			}

			void RunASync(bool res)
			{
				if (!generic)
				{
					(asyncActWrapped, wrapper) = wrappedPolicy.WrapDelegateIfNeed(asyncAct, default, false);
					ClassicAssert.AreEqual(res, asyncAct.Equals(asyncActWrapped));
				}
				else
				{
					(asyncFuncTWrapped, wrapper) = wrappedPolicy.WrapDelegateIfNeed(asyncFuncT, default, false);
					ClassicAssert.AreEqual(res, asyncFuncT.Equals(asyncFuncTWrapped));
				}
				ClassicAssert.AreEqual(res, wrapper == null);
			}
		}

		[Test]
		[TestCase(PolicyResultFailedReason.PolicyResultHandlerFailed)]
		[TestCase(PolicyResultFailedReason.PolicyProcessorFailed)]
		public void Should_PolicyWrapper_Throws_Correct_Exception_If_Failed(PolicyResultFailedReason failedReason)
		{
			var pr = new PolicyResult();
			pr.AddError(new InvalidOperationException());
			pr.SetFailedInner(failedReason);

			var pw = new PolicyWrapperTest();

			if (failedReason == PolicyResultFailedReason.PolicyResultHandlerFailed)
			{
				var ex = Assert.Throws<PolicyResultHandlerFailedException>(() => pw.ThrowIf(pr));
				Assert.That(ex.Result, Is.Not.Null);
			}
			else
			{
				Assert.Throws<InvalidOperationException>(() => pw.ThrowIf(pr));
			}
		}

		[Test]
		[TestCase(PolicyResultFailedReason.PolicyResultHandlerFailed)]
		[TestCase(PolicyResultFailedReason.PolicyProcessorFailed)]
		public void Should_PolicyWrapper_Throws_Correct_Exception_If_Failed_T(PolicyResultFailedReason failedReason)
		{
			var pr = new PolicyResult<int>();
			pr.AddError(new InvalidOperationException());
			pr.SetFailedInner(failedReason);

			var pw = new PolicyWrapperTest();

			if (failedReason == PolicyResultFailedReason.PolicyResultHandlerFailed)
			{
				var ex = Assert.Throws<PolicyResultHandlerFailedException<int>>(() => pw.ThrowIf(pr));
				Assert.That(ex.Result, Is.Not.Null);
			}
			else
			{
				Assert.Throws<InvalidOperationException>(() => pw.ThrowIf(pr));
			}
		}

		[Test]
		public void Should_Retry_Then_Fallback_Returns_Valid_Result()
		{
#pragma warning disable RCS1118 // Mark local variable as const.
			var zero = 0;
#pragma warning restore RCS1118 // Mark local variable as const.
			var errorProcessorFlag = false;
			var fallbackResult = new RetryPolicy(3)
									.ExcludeError<DivideByZeroException>()
									.WrapUp(new FallbackPolicy())
									.OuterPolicy
									.WithFallbackFunc(() => int.MaxValue)
									.IncludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => errorProcessorFlag = true)
									.Handle(() => 5 / zero);

			Assert.That(fallbackResult.Result, Is.EqualTo(int.MaxValue));
			Assert.That(fallbackResult.Errors.Count(), Is.EqualTo(1));
			Assert.That(fallbackResult.Errors.FirstOrDefault()?.GetType(), Is.EqualTo(typeof(DivideByZeroException)));
			Assert.That(fallbackResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
			Assert.That(errorProcessorFlag, Is.True);
		}

		private class PolicyWrapperTest : PolicyWrapperBase
		{
			public PolicyWrapperTest() :base(CancellationToken.None){}

			public void ThrowIf(PolicyResult pr) => ThrowIfFailed(pr);

			public void ThrowIf<T>(PolicyResult<T> pr) => ThrowIfFailed(pr);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Simple_Then_Fallback_Returns_Valid_Result(bool isZero)
		{
			var fallbackErrorProcessorFlag = false;
			var simpleErrorProcessorFlag = false;
			var zero = isZero ? 0 : 5;

			Func<int> funcToHandle;
			if (isZero)
			{
				funcToHandle = () => 5 / zero;
			}
			else
			{
				funcToHandle = () => throw new InvalidOperationException();
			}

			var fallbackResult = new SimplePolicy()
									.ExcludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => simpleErrorProcessorFlag = true)
									.WrapUp(new FallbackPolicy())
									.OuterPolicy
									.WithFallbackFunc(() => int.MaxValue)
									.IncludeError<DivideByZeroException>()
									.WithErrorProcessorOf((_) => fallbackErrorProcessorFlag = true)
									.Handle(funcToHandle);
			if (isZero)
			{
				Assert.That(fallbackResult.Result, Is.EqualTo(int.MaxValue));
				Assert.That(fallbackResult.Errors.Count(), Is.EqualTo(1));
				Assert.That(fallbackResult.Errors.FirstOrDefault()?.GetType(), Is.EqualTo(typeof(DivideByZeroException)));
				Assert.That(fallbackResult.WrappedPolicyResults.LastOrDefault()?.Result.ErrorFilterUnsatisfied, Is.True);
				Assert.That(fallbackResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Failed));
				Assert.That(fallbackErrorProcessorFlag, Is.True);
				Assert.That(simpleErrorProcessorFlag, Is.False);
			}
			else
			{
				Assert.That(fallbackResult.Result, Is.EqualTo(default(int)));
				Assert.That(fallbackResult.NoError, Is.True);
				Assert.That(fallbackResult.WrappedPolicyResults.LastOrDefault()?.Result.Errors.FirstOrDefault(), Is.TypeOf<InvalidOperationException>());
				Assert.That(fallbackResult.WrappedPolicyResults.LastOrDefault()?.Result.NoError, Is.False);
				Assert.That(fallbackResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.PolicySuccess));
				Assert.That(simpleErrorProcessorFlag, Is.True);
				Assert.That(fallbackErrorProcessorFlag, Is.False);
			}
			Assert.That(fallbackResult.WrappedPolicyResults.Count(), Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Return_Correct_PolicyResult_When_OperationCanceledException_In_Wrapped_Policy_For_HandleAsync(bool withCanceledException)
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(1);
				policy.WrapPolicy(new SimplePolicy(new AlwaysFailedAndCanceledSimplePolicyProcessor(withCanceledException)));

				var pr = await policy.HandleAsync(async (_) => await Task.Delay(1), cts.Token);
				Assert.That(pr.Errors.OfType<NullReferenceException>().Count, Is.EqualTo(0));
				Assert.That(pr.IsFailed, Is.True);
				if (withCanceledException)
				{
					Assert.That(pr.WrappedPolicyResults.FirstOrDefault().Result.PolicyCanceledError, Is.Not.Null);
				}
				else
				{
					Assert.That(pr.Errors.FirstOrDefault(), Is.TypeOf<OperationFailedAndCanceledException>());
				}
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Return_Correct_PolicyResult_When_OperationCanceledException_In_Wrapped_Policy_For_HandleTAsync(bool withCanceledException)
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(1);
				policy.WrapPolicy(new SimplePolicy(new AlwaysFailedAndCanceledSimplePolicyProcessor(withCanceledException)));

				var pr = await policy.HandleAsync(async (_) => { await Task.Delay(1); return 1; }, cts.Token);
				Assert.That(pr.Errors.OfType<NullReferenceException>().Count, Is.EqualTo(0));
				Assert.That(pr.IsFailed, Is.True);
				if (withCanceledException)
				{
					Assert.That(pr.WrappedPolicyResults.FirstOrDefault().Result.PolicyCanceledError, Is.Not.Null);
				}
				else
				{
					Assert.That(pr.Errors.FirstOrDefault(), Is.TypeOf<OperationFailedAndCanceledException>());
				}
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Return_Correct_PolicyResult_When_OperationCanceledException_In_Wrapped_Policy_For_Handle(bool withCanceledException)
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(1);
				policy.WrapPolicy(new SimplePolicy(new AlwaysFailedAndCanceledSimplePolicyProcessor(withCanceledException)));

				var pr = policy.Handle((_) => { }, cts.Token);
				Assert.That(pr.Errors.OfType<NullReferenceException>().Count, Is.EqualTo(0));
				Assert.That(pr.IsFailed, Is.True);
				if (withCanceledException)
				{
					Assert.That(pr.WrappedPolicyResults.FirstOrDefault().Result.PolicyCanceledError, Is.Not.Null);
				}
				else
				{
					Assert.That(pr.Errors.FirstOrDefault(), Is.TypeOf<OperationFailedAndCanceledException>());
				}
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Return_Correct_PolicyResult_When_OperationCanceledException_In_Wrapped_Policy_For_HandleT(bool withCanceledException)
		{
			using (var cts = new CancellationTokenSource())
			{
				var policy = new RetryPolicy(1);
				policy.WrapPolicy(new SimplePolicy(new AlwaysFailedAndCanceledSimplePolicyProcessor(withCanceledException)));

				var pr = policy.Handle((_) => 1, cts.Token);
				Assert.That(pr.Errors.OfType<NullReferenceException>().Count, Is.EqualTo(0));
				Assert.That(pr.IsFailed, Is.True);
				if (withCanceledException)
				{
					Assert.That(pr.WrappedPolicyResults.FirstOrDefault().Result.PolicyCanceledError, Is.Not.Null);
				}
				else
				{
					Assert.That(pr.Errors.FirstOrDefault(), Is.TypeOf<OperationFailedAndCanceledException>());
				}
			}
		}

		private class AlwaysFailedAndCanceledSimplePolicyProcessor : ISimplePolicyProcessor
		{
			private readonly bool _setCanceledExcepton;
			public AlwaysFailedAndCanceledSimplePolicyProcessor(bool setCanceledExcepton = true)
			{
				_setCanceledExcepton = setCanceledExcepton;
			}

			public PolicyProcessor.ExceptionFilter ErrorFilter => throw new NotImplementedException();

			public void AddErrorProcessor(IErrorProcessor newErrorProcessor) => throw new NotImplementedException();

			public PolicyResult Execute(Action action, CancellationToken token = default)
			{
				var pr = new PolicyResult(true);
				if (_setCanceledExcepton)
				{
					pr.SetFailedAndCanceled(new OperationCanceledException());
				}
				else
				{
					pr.SetFailedAndCanceled();
				}
				return pr;
			}

			public PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
			{
				var pr = new PolicyResult<T>(true);
				if (_setCanceledExcepton)
				{
					pr.SetFailedAndCanceled(new OperationCanceledException());
				}
				else
				{
					pr.SetFailedAndCanceled();
				}
				return pr;
			}

			public Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
			{
				var pr = new PolicyResult(true);
				if (_setCanceledExcepton)
				{
					pr.SetFailedAndCanceled(new OperationCanceledException());
				}
				else
				{
					pr.SetFailedAndCanceled();
				}
				return Task.FromResult(pr);
			}

			public Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
			{
				var pr = new PolicyResult<T>(true);
				if (_setCanceledExcepton)
				{
					pr.SetFailedAndCanceled(new OperationCanceledException());
				}
				else
				{
					pr.SetFailedAndCanceled();
				}
				return Task.FromResult(pr);
			}

			public IEnumerator<IErrorProcessor> GetEnumerator() => throw new NotImplementedException();
			IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
		}
	}
}