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
			Assert.IsTrue(res.NoError);
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
			Assert.IsTrue(res.NoError);
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
			Assert.IsTrue(res.NoError);
		}

		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public async Task Should_CatchBlockError_Handled_For_AsyncHandleT(bool throwError, int CatchBlockErrorsCount, int k)
		{
			async Task<int> saveAsync(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			Assert.IsTrue(res.Errors.Count() == 1);

			Assert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			Assert.AreEqual(k, i);
		}

		[Test]
		public async Task Should_HandleTAsync_If_NoError_Work()
		{
			async Task<int> saveAsync(CancellationToken _) { await Task.Delay(1); return 1; }
			var retryPolTest = new SimplePolicyProcessor();
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			Assert.IsTrue(res.NoError);
			Assert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_ExecuteAsync_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
			const int i = 0;
			var processor = SimplePolicyProcessor.CreateDefault();
			var tryResCount = await processor.ExecuteAsync(save, cancelTokenSource.Token);

			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreEqual(0, i);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_ExecuteAsyncT_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
			const int i = 0;
			var processor = SimplePolicyProcessor.CreateDefault();
			var tryResCount = await processor.ExecuteAsync(save, cancelTokenSource.Token);

			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreEqual(0, i);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_PolicyResult_Be_Failed_And_Canceled_If_Canceled_During_Error_Processors_Run()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var processor = SimplePolicyProcessor.CreateDefault();
			var res =  processor.WithErrorProcessorOf((_, __) => cancelTokenSource.Cancel())
					 .Execute(() => throw new Exception(), cancelTokenSource.Token);
			Assert.IsTrue(res.IsFailed);
			Assert.IsTrue(res.IsCanceled);
			Assert.IsFalse(res.IsSuccess);
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_Generic_IncludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			processor.IncludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_IncludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault().IncludeError(ex => ex.Message == paramName);
			void saveWithInclude() => throw new Exception(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

	    [Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_Generic_ExcludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			processor.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_ExcludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault().ExcludeError(ex => ex.Message == paramName);
			void saveWithInclude() => throw new Exception(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_Execute_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = proc.Execute(null);
			Assert.IsTrue(simpleResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_ExecuteT_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = proc.Execute<int>(null);
			Assert.IsTrue(simpleResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_ExecuteAsync_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = await proc.ExecuteAsync(null);
			Assert.IsTrue(simpleResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_ExecuteTAsync_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = await proc.ExecuteAsync<int>(null);
			Assert.IsTrue(simpleResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_WithErrorProcessor_AddErrorProcessorInCollection()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			proc.AddErrorProcessor(new TestErrorProcessor());
			Assert.IsTrue(proc.Count() == 1);
		}

		private class TestErrorProcessor : IErrorProcessor
		{
			public Exception Process(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
			{
				throw new NotImplementedException();
			}

			public Task<Exception> ProcessAsync(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
			{
				throw new NotImplementedException();
			}
		}
	}
}
