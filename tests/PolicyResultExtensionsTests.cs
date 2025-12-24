using NUnit.Framework;
using System;

namespace PoliNorError.Tests
{
	internal class PolicyResultExtensionsTests
	{
#pragma warning disable CS0618 // Type or member is obsolete
		[Test]
		public void Should_ChangeByRetryDelayResult_DoesNotThrow_On_Null_BasicResult()
		{
			var pr = new PolicyResult<int>();
			pr.SetResult(1);
			Assert.DoesNotThrow(() => pr.ChangeByRetryDelayResult(null, new Exception()));
		}
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
