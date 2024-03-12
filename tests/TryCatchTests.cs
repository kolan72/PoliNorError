using NUnit.Framework;
using PoliNorError.TryCatch;
using System;

namespace PoliNorError.Tests
{
	internal class TryCatchTests
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_TryCatchResult_Initialized_From_PolicyResult_Correctly_When_Error_Or_Ok(bool isError)
		{
			var policyResult = new PolicyResult();
			if (isError)
			{
				policyResult.SetFailedWithError(new Exception());
			}
			else
			{
				policyResult.SetOk();
			}
			var tryCatchResult = new TryCatchResult(policyResult);
			if (isError)
			{
				Assert.That(tryCatchResult.Error, Is.Not.Null);
				Assert.That(tryCatchResult.IsError, Is.True);
			}
			else
			{
				Assert.That(tryCatchResult.Error, Is.Null);
				Assert.That(tryCatchResult.IsError, Is.False);
			}
		}
	}
}
