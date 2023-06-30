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
			retryPolicy.AddPolicyResultHandler((PolicyResult<int> __, CancellationToken _) => i++)
					   .AddPolicyResultHandler((PolicyResult<int> __, CancellationToken _) => i++);

			retryPolicy.Handle<int>(() => throw new Exception("Handle"));
			Assert.AreEqual(2, i);
		}

		[Test]
		public void Should_PolicyResult_Can_Be_SetFailed_By_SyncPolicyResultHandlerT_Even_If_NoError()
		{
			void action(PolicyResult<int> pr, CancellationToken _) { pr.SetFailed(); }
			var retryPolicy = new RetryPolicy(1).AddPolicyResultHandler<RetryPolicy, int>(action);
			var res = retryPolicy.Handle(() =>1);
			Assert.IsTrue(res.NoError);
			Assert.IsTrue(res.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.PolicyResultHandlerFailed, res.FailedReason);
		}

	}
}
