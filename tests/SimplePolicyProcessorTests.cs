using NUnit.Framework;
using System;
using System.Linq;

namespace PoliNorError.Tests
{
	internal class SimplePolicyProcessorTests
	{
		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public void Should_CatchBlockError_Handled_For_Handle(bool throwError, int CatchBlockErrorsCount, int k)
		{
			void saveAsync() => throw new Exception();

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; } 
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = retryPolTest.Execute(saveAsync);
			Assert.IsTrue(res.Errors.Count() == 1);

			Assert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			Assert.AreEqual(k, i);
		}
	}
}
