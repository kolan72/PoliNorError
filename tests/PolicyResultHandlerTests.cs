using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace PoliNorError.Tests
{
	internal class PolicyResultHandlerTests
	{
		[Test]
		public void Should_PolicyResult_Handled_By_Handle_Method_If_MiscHandlerAdded()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandler(async (__, _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandler((__, _) => i++)
					   ;
			retryPolicy.Handle(() => throw new Exception("Handle"));
			Assert.AreEqual(3, i);
		}

		[Test]
		public void Should_HandleResult_If_Token_Canceled()
		{
			var cancelSource = new CancellationTokenSource();
			cancelSource.Cancel();
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandler(async (__, _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandler(async (_) => { await Task.Delay(1); i++; });

			var res2 = retryPolicy.Handle(() => throw new Exception("Handle"), cancelSource.Token);
			Assert.AreEqual(typeof(OperationCanceledException), res2.PolicyResultHandlingErrors.FirstOrDefault().InnerException.GetType());
			cancelSource.Dispose();
		}

		[Test]
		public async Task Should_Work_HandleResultAsync_MiscHandlers()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			int m = 0;
			retryPolicy =  retryPolicy.AddPolicyResultHandler((__, _) => i++)
									  .AddPolicyResultHandler((_) => i++)
									  .AddPolicyResultHandler(async (__, _) => { await Task.Delay(1); m++; })
									  .AddPolicyResultHandler((__, _) => i++);

			await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Handle"); });

			Assert.AreEqual(3, i);
			Assert.AreEqual(1, m);
		}

		[Test]
		public void Should_HandleResultError_NotAffect_NoError()
		{
			void action(PolicyResult _) { throw new Exception(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler(action);
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.PolicyResultHandlingErrors.Count() == 1);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_SyncPolicyResultHandler_Even_If_NoError()
		{
			void action(PolicyResult pr) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler(action);
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_AsyncPolicyResultHandler_Even_If_NoError()
		{
			async Task func(PolicyResult pr) { await Task.Delay(1); pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler(func);
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		public void Should_HandleResult_HasError_For_AggregateException()
		{
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler((_) => Task.FromException(new Exception()));
			var res = retryPolicy.Handle(() => { });
			Assert.IsTrue(res.PolicyResultHandlingErrors.Count() == 1);
		}
    }
}
