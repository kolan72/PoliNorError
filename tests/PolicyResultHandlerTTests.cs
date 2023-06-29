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
	}
}
