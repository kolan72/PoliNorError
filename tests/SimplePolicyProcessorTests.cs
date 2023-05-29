using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PoliNorError.Tests
{
	internal class SimplePolicyProcessorTests
	{
		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public void Should_CatchBlockError_Handled_For_Handle(bool throwError, int CatchBlockErrorsCount, int k)
		{
			void save() => throw new Exception();

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; } 
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = retryPolTest.Execute(save);
			Assert.IsTrue(res.Errors.Count() == 1);

			Assert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			Assert.AreEqual(k, i);
		}

		[Test]
		public void Should_Handle_If_NoError_Work()
		{
			void save() => Expression.Empty();
			var retryPolTest = new SimplePolicyProcessor();
			var res = retryPolTest.Execute(save);
			Assert.IsTrue(res.IsOk);
		}

		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public void Should_CatchBlockError_Handled_For_HandleT(bool throwError, int CatchBlockErrorsCount, int k)
		{
			int save() => throw new Exception();

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);

			var res = retryPolTest.Execute(save);
			Assert.IsTrue(res.Errors.Count() == 1);

			Assert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			Assert.AreEqual(k, i);
		}

		[Test]
		public void Should_HandleT_If_NoError_Work()
		{
			int save() => 1;
			var retryPolTest = new SimplePolicyProcessor();
			var res = retryPolTest.Execute(save);
			Assert.IsTrue(res.IsOk);
			Assert.AreEqual(1, res.Result);
		}
	}
}
