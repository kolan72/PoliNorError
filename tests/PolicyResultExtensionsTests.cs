using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyResultExtensionsTests
	{
		[Test]
		public void Should_ChangeByRetryDelayResult_DoesNotThrow_On_Null_BasicResult()
		{
			var pr = new PolicyResult<int>();
			pr.SetResult(1);
			Assert.DoesNotThrow(() => pr.ChangeByRetryDelayResult(null, new Exception()));
		}
	}
}
