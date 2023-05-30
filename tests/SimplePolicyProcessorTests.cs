using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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

		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public async Task Should_CatchBlockError_Handled_For_AsyncHandle(bool throwError, int CatchBlockErrorsCount, int k)
		{
			async Task saveAsync(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			Assert.IsTrue(res.Errors.Count() == 1);

			Assert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			Assert.AreEqual(k, i);
		}

		[Test]
		public async Task Should_HandleAsync_If_NoError_Work()
		{
			async Task saveAsync(CancellationToken _) => await Task.Delay(1);
			var retryPolTest = new SimplePolicyProcessor();
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			Assert.IsTrue(res.IsOk);
		}
	}
}
