using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.BulkErrorProcessor;

namespace PoliNorError.Tests
{
	internal class DefaultFallbackProcessorAsyncTests
	{
		[Test]
		public async Task Should_FallbackAsync_CallFallback()
		{
			var processor = FallbackProcessor.CreateDefault();

			int i = 1;

			async Task fallback(CancellationToken _) { await Task.Delay(1); i++; }

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, fallback);

			Assert.AreEqual(2, i);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(1, res.Errors.Count());
		}

		[Test]
		public async Task Should_FallbackAsync_Call_Error_Func_When_Error()
		{
			int i = 1;
			async Task onErrorTask(Exception _, CancellationToken __) { await Task.FromResult(i++); }

			var processor = FallbackProcessor
										.CreateDefault().WithErrorProcessorOf(onErrorTask);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => await Task.Delay(1));

			Assert.AreEqual(2, i);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(1, res.Errors.Count());
		}

		[Test]
		public async Task Should_FallbackAsync_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }

			var processor = FallbackProcessor.CreateDefault().WithErrorProcessorOf(save);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => await Task.Delay(1));

			Assert.AreEqual(2, i);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(1, res.Errors.Count());
		}

		[Test]
		public async Task Should_FallbackAsyncT_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }

			var processor = FallbackProcessor.CreateDefault().WithErrorProcessorOf(save);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => { await Task.Delay(1); return 1; });

			Assert.AreEqual(2, i);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(1, res.Errors.Count());
			Assert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Call_Error_Func_When_Error()
		{
			int i = 1;
			async Task onErrorTask(Exception _, CancellationToken __) { await Task.FromResult(i++); }

			var processor = FallbackProcessor.CreateDefault().WithErrorProcessorOf(onErrorTask);

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); throw new Exception(); }, async (_) => { await Task.Delay(1); return 1; });

			Assert.AreEqual(2, i);
			Assert.AreEqual(false, res.IsFailed);
			Assert.AreEqual(1, res.Errors.Count());
			Assert.AreEqual(1, res.Result);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Returns_FallbackValue_If_NotError()
		{
			var processor = FallbackProcessor.CreateDefault();

			int i = 1;

			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); return ++i; }

			var res = await processor.FallbackAsync<int>(async (_) => { await Task.Delay(1); throw new Exception(); }, fallback);

			Assert.AreEqual(2, i);
			Assert.AreEqual(false, res.IsFailed);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Returns_SuccessValue_If_NotError()
		{
			var processor = FallbackProcessor.CreateDefault();

			int i = 1;

			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); return ++i; }

			var res = await processor.FallbackAsync(async (_) => { await Task.Delay(1); return i; }, fallback);

			Assert.AreEqual(1, i);
			Assert.AreEqual(false, res.IsFailed);
		}

		[Test]
		public async Task Should_FallbackAsync_Result_IsFailed_Equals_True_When_Error_In_CatchBlockProcessing()
		{
			var processor = FallbackProcessor.CreateDefault();
			var polResult = await processor.FallbackAsync((_) => throw new Exception("Test_Save"), (_) => throw new Exception("Test_Fallback"));

			Assert.AreEqual(true, polResult.IsFailed);
			Assert.AreEqual(true, polResult.CatchBlockErrors.FirstOrDefault().IsCritical);
			Assert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
		}

		[Test]
		public async Task Should_FallbackAsyncT_Result_IsFailed_Equals_True_When_Error_In_CatchBlockProcessing()
		{
			var processor = FallbackProcessor.CreateDefault();
			var polResult = await processor.FallbackAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test_Save"); }, async (_) => { await Task.Delay(1); throw new Exception("Test_Fallback");});

			Assert.AreEqual(true, polResult.IsFailed);
			Assert.AreEqual(true, polResult.CatchBlockErrors.FirstOrDefault().IsCritical);
			Assert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
		}

		[Test]
		public async Task Should_FallbackAsync_Break_When_ErrorProcessing_Canceled()
		{
			var throwingExc = new ApplicationException();
			int i = 0;
			async Task save(CancellationToken _) { await Task.Delay(1); throw throwingExc; }
			var mockedBulkProcessor = new Mock<IBulkErrorProcessor>();
			mockedBulkProcessor.Setup((t) => t.ProcessAsync(It.IsAny<ProcessErrorInfo>(), throwingExc, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
				new BulkProcessResult(throwingExc, new List<ErrorProcessorException>() { new ErrorProcessorException(new Exception(), null, ProcessStatus.Canceled) })));

			async Task fallback(CancellationToken _) { await Task.Delay(1); i++; }

			var processor = new DefaultFallbackProcessor(mockedBulkProcessor.Object);

			var tryResCount = await processor.FallbackAsync(save, fallback);

			Assert.AreEqual(true, tryResCount.IsFailed);
			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreEqual(0, i);
		}

		[Test]
		public async Task Should_FallbackAsync_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
			int i = 0;
			var processor = FallbackProcessor.CreateDefault();
			async Task fallback(CancellationToken _) { await Task.Delay(1); i++; }
			var tryResCount = await processor.FallbackAsync(save, fallback, cancelTokenSource.Token);

			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreEqual(0, i);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_FallbackAsyncT_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
			int i = 0;
			var processor = FallbackProcessor.CreateDefault();
			async Task<int> fallback(CancellationToken _) { await Task.Delay(1); i++; return i; }
			var tryResCount = await processor.FallbackAsync(save, fallback, cancelTokenSource.Token);

			Assert.AreEqual(true, tryResCount.IsCanceled);
			Assert.AreEqual(0, i);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackAsync_Null_Delegate()
		{
			var proc = FallbackProcessor.CreateDefault();
			var fallbackResult = await proc.FallbackAsync(null, async (_) => await Task.Delay(1));
			Assert.IsTrue(fallbackResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, fallbackResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), fallbackResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_FallbackTAsync_Null_Delegate()
		{
			var proc = FallbackProcessor.CreateDefault();
			var fallbackResult = await proc.FallbackAsync(null, async (_) => { await Task.Delay(1); return 1; });
			Assert.IsTrue(fallbackResult.IsFailed);
			Assert.AreEqual(PolicyResultFailedReason.DelegateIsNull, fallbackResult.FailedReason);
			Assert.AreEqual(typeof(NoDelegateException), fallbackResult.Errors.FirstOrDefault()?.GetType());
		}
	}
}