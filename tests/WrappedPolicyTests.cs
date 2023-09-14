using NSubstitute;
using NUnit.Framework;
using System;
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
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

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
			Assert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

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
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

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
			Assert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

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
			Assert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			Assert.IsFalse(outPolicyResult.IsFailed);
			Assert.AreEqual(1, outPolicyResult.Errors.Count());

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
			Assert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

			subsPolicy.Received(3).Handle(act, default);
			Assert.AreEqual(outPolicy.PolicyName, outPolicyResult.PolicyName);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Error_ForFallback()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return ++i; });

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			Func<CancellationToken, Task<int>> act = async (_) => { await Task.Delay(1); throw new Exception(); };

			var polResult = new PolicyResult<int>();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.HandleAsync(act, default, cancelToken).Returns(Task.FromResult(polResult));
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = await fallback.HandleAsync(act, false, cancelToken);

			//Out policy error should be the same as wrapped policy error.
			Assert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			Assert.AreEqual(false, outPolicyResult.IsFailed);
			Assert.AreEqual(1, outPolicyResult.Errors.Count());

			await subsPolicy.Received(1).HandleAsync(act, default, cancelToken);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Error_ForRetry()
		{
			var outPolicy = new RetryPolicy(2);

			var subsPolicy = Substitute.For<IPolicyBase>();
			Func<CancellationToken, Task<int>> act = (_) => throw new Exception();

			var polResult = PolicyResult<int>.ForNotSync();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.HandleAsync(act, default, default).Returns(Task.FromResult(polResult));
			outPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = await outPolicy.HandleAsync(act, default(CancellationToken));

			//Out policy error should be the same as wrapped policy error.
			Assert.AreEqual(3, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(3, outPolicyResult.WrappedPolicyResults.Count());

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
			Assert.IsTrue(polResult.IsCanceled);
			Assert.IsTrue(polResult.IsFailed);

			Assert.IsTrue(polResult.WrappedPolicyResults.FirstOrDefault().Result.IsCanceled);
			Assert.IsTrue(polResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			Assert.AreEqual(0, i);
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

			Assert.AreEqual(1, i);
			Assert.IsTrue(polResult.IsFailed);
			Assert.IsTrue(polResult.IsCanceled);
			Assert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());

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
			Assert.IsTrue(polResult.IsCanceled);
			Assert.IsTrue(polResult.IsFailed);

			Assert.IsTrue(polResult.WrappedPolicyResults.FirstOrDefault().Result.IsCanceled);
			Assert.AreEqual(0, i);
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
			Assert.IsFalse(outPolicyResult.IsFailed);
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
			Assert.IsTrue(outPolicyResult.IsFailed);
			Assert.IsTrue(outPolicyResult.Errors.All(err => err.GetType().Equals(typeof(PolicyResultHandlerFailedException))));
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
			Assert.IsFalse(outPolicyResult.IsFailed);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.First().Result.Result);
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

			Assert.AreEqual(10, i);
			Assert.IsFalse(polResult.IsFailed);
			Assert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
			Assert.AreEqual(10, polResult.Result);
			Assert.AreEqual(fallBackPolicy.PolicyName, polResult.PolicyName);
		}

		[Test]
		public void Should_HandleT_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithFallbackFunc((_) => ++i);

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			Func<int> act = () => 56;

			subsPolicy.Handle(act, cancelToken).Returns(new PolicyResult<int>());
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = fallback.Handle(act);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			Assert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			subsPolicy.Received(1).Handle(act, cancelToken);
		}

		[Test]
		public async Task Should_Fallback_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var retry = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));

			var subsPolicy = Substitute.For<IPolicyBase>();

			Func<CancellationToken, Task> func = async (_) => { await Task.Delay(1); throw new Exception(); };

			var polResult = PolicyResult.ForNotSync();
			polResult.SetFailedInner();

			subsPolicy.HandleAsync(func, default, default).Returns(Task.FromResult(polResult));
			retry.WrapPolicy(subsPolicy);

			var outPolicyResult = await retry.HandleAsync(func, default(CancellationToken));
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			Assert.AreEqual(true, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			await subsPolicy.Received(1).HandleAsync(func, default, default);
		}

		[Test]
		public async Task Should_Fallback_HandleAsync_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var fallBackPol = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => await Task.Delay(1));

			var subsPolicy = Substitute.For<IPolicyBase>();

			Func<CancellationToken, Task> func = async (_) => await Task.Delay(1);

			subsPolicy.HandleAsync(func, default, default).Returns(Task.FromResult(new PolicyResult()));
			fallBackPol.WrapPolicy(subsPolicy);

			var outPolicyResult = await fallBackPol.HandleAsync(func, default(CancellationToken));
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			Assert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

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

			Assert.AreEqual(10, i);
			Assert.IsFalse(polResult.IsFailed);
			Assert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
			Assert.AreEqual(fallBackPolicy.PolicyName, polResult.PolicyName);
		}

		[Test]
		public void Should_Fallback_Handle_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => { });

			var subsPolicy = Substitute.For<IPolicyBase>();

			Action act = () => { };

			subsPolicy.Handle(act, default).Returns(new PolicyResult());
			fallbackPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = fallbackPolicy.Handle(act);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			Assert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			subsPolicy.Received().Handle(act, default);
		}

		[Test]
		public void Should_Fallback_Handle_CallWrappedPolicy_When_Wrapped_And_Error()
		{
			var fallbackPolicy = new FallbackPolicy().WithFallbackAction((_) => { });

			var subsPolicy = Substitute.For<IPolicyBase>();

			Action act = () => throw new Exception();

			var polResult = PolicyResult.ForSync();
			polResult.SetFailedInner();
			polResult.AddError(new Exception("Wrapped exception"));

			subsPolicy.Handle(act, default).Returns(polResult);
			fallbackPolicy.WrapPolicy(subsPolicy);

			var outPolicyResult = fallbackPolicy.Handle(act, default);

			//Out policy error should be the same as wrapped policy error.
			Assert.AreEqual(1, outPolicyResult.Errors.Count(ex => ex.Message == "Wrapped exception"));

			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());

			Assert.AreEqual(false, outPolicyResult.IsFailed);

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

			Assert.AreEqual(10, i);
			Assert.IsFalse(polResult.IsFailed);
			Assert.IsTrue(polResult.IsSuccess);
			Assert.IsFalse(polResult.NoError);
			Assert.AreEqual(2, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
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

			Assert.AreEqual(10, i);
			Assert.IsFalse(polResult.IsFailed);
			Assert.AreEqual(3, polResult.WrappedPolicyResults.FirstOrDefault().Result.Errors.Count());
			Assert.AreEqual(10, polResult.Result);
		}

		[Test]
		public async Task Should_HandleAsyncT_CallWrappedPolicy_When_Wrapped_And_Not_Error()
		{
			int i = 1;
			var fallback = new FallbackPolicy().WithAsyncFallbackFunc(async (_) => { await Task.Delay(1); return ++i; });

			var subsPolicy = Substitute.For<IPolicyBase>();
			var cancelToken = new CancellationToken();

			Func<CancellationToken, Task<int>> act = (_) => Task.FromResult(56);

			subsPolicy.HandleAsync(act, default, cancelToken).Returns(Task.FromResult(new PolicyResult<int>()));
			fallback.WrapPolicy(subsPolicy);

			var outPolicyResult = await fallback.HandleAsync(act, cancelToken);
			Assert.AreEqual(1, outPolicyResult.WrappedPolicyResults.Count());
			Assert.AreEqual(false, outPolicyResult.WrappedPolicyResults.FirstOrDefault().Result.IsFailed);

			await subsPolicy.Received(1).HandleAsync(act, default, cancelToken);
		}
	}
}