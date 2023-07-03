using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyResultHandlerTTests
	{
		[Test]
		public void Should_PolicyResult_Handled_By_Handle_Method_If_SyncHandlersAdded()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddPolicyResultHandlerInner((PolicyResult<int> __, CancellationToken _) => i++);

			retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_SyncPolicyResultHandlerT_Even_If_NoError()
		{
			void action(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandlerInner<RetryPolicy, int>(action);
			var res = retryPolicy.Handle(() =>1);
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_PolicyResult_Handled_By_Handle_And_HandleAsync_Method_If_ASyncHandlersAdded(bool asnc)
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner(async (PolicyResult<int> __, CancellationToken _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandlerInner(async (PolicyResult<int> __) => { await Task.Delay(1); i++; });

			if (asnc)
			{
				await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			}
			else
			{
				retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			}

			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Handled_By_HandleAsync_If_SyncHandler_And_ASyncHandler_Added()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner(async (PolicyResult<int> __, CancellationToken _) => { await Task.Delay(1); i++; })
					   .AddPolicyResultHandlerInner((PolicyResult<int> __) => i++);

			await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Handled_By_HandleAsync_If_Only_SyncHandlers_Added()
		{
			var retryPolicy = new RetryPolicy(1);
			int i = 0;
			retryPolicy.AddPolicyResultHandlerInner((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddPolicyResultHandlerInner((PolicyResult<int> __) => i++);

			await retryPolicy.HandleAsync<int>((_) => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public async Task Should_PolicyResult_Can_Be_SetFailed_By_ASyncPolicyResultHandlerT_Even_If_NoError()
		{
			async Task func(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); await Task.Delay(1); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandlerInner<RetryPolicy, int>(func);
			var res = await retryPolicy.HandleAsync(async (_) => { await Task.Delay(1); return 1; });
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}
	}
}
