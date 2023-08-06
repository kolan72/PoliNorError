using Moq;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class DelayTimeErrorProcessorTests
	{
		[Test]
		public async Task Should_BeCalled_In_ProcessAsyncMethod_With_No_Delegate()
		{
			var delayProcessor = new DelayErrorProcessor(TimeSpan.Zero);
			var exc = new Exception();
			var res = await delayProcessor.ProcessAsync(exc, CatchBlockProcessErrorInfo.FromRetry(1), CancellationToken.None);
			Assert.IsTrue(exc.Equals(res));
		}

		[Test]
		public void Should_BeCalled_In_ProcessMethod_With_No_Delegate()
		{
			var delayProcessor = new DelayErrorProcessor(TimeSpan.Zero);
			var exc = new Exception();
			var res = delayProcessor.Process(exc, CatchBlockProcessErrorInfo.FromRetry(1), CancellationToken.None);
			Assert.IsTrue(exc.Equals(res));
		}

		[Test]
		public void Should_BeCalled_SleepProvider_In_ProcessMethod()
		{
			var funcMock = new Mock<Func<int, Exception, TimeSpan>>();
			funcMock.Setup((f) => f(It.IsAny<int>(), It.IsAny<Exception>()));
			var delayProcessor = new DelayErrorProcessor(funcMock.Object);
			delayProcessor.Process(new Exception(), CatchBlockProcessErrorInfo.FromRetry(1), CancellationToken.None);
			funcMock.Verify((f) => f(It.IsAny<int>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public async Task Should_SleepProvider_BeCalled_In_ProcessAsyncMethod()
		{
			var funcMock = new Mock<Func<int, Exception, TimeSpan>>();
			funcMock.Setup((f) => f(It.IsAny<int>(), It.IsAny<Exception>()));
			var delayProcessor = new DelayErrorProcessor(funcMock.Object);
			await delayProcessor.ProcessAsync(new Exception(), CatchBlockProcessErrorInfo.FromRetry(1), CancellationToken.None);
			funcMock.Verify((f) => f(It.IsAny<int>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public void Should_Wait_ForSimpleDelay()
		{
			var processor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1500));
			var testExc = new Exception();
			var sw = Stopwatch.StartNew();
			var resError = processor.Process(testExc, CatchBlockProcessErrorInfo.FromRetry(1), default);
			sw.Stop();
			Assert.GreaterOrEqual(Math.Floor(sw.Elapsed.TotalSeconds), 1);
			Assert.AreEqual(resError, testExc);
		}

		[Test]
		public void Should_Process_Throw_TaskCanceledException_If_Canceled()
		{
			var processor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
			var testExc = new Exception();
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			Assert.Throws<OperationCanceledException>(() => processor.Process(testExc, CatchBlockProcessErrorInfo.FromRetry(1), cancelTokenSource.Token));
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_Wait_ForTaskDelay()
		{
			var processor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
			var testExc = new Exception();
			var sw = Stopwatch.StartNew();
			Exception resError = await processor.ProcessAsync(testExc, CatchBlockProcessErrorInfo.FromRetry(1), CancellationToken.None);
			sw.Stop();
			Assert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 500);
			Assert.AreEqual(resError, testExc);
		}

		[Test]
		public void Should_Delegate_BeCalled_In_ProcessAsyncMethod_With_CancelError()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(1000);
			var delayProcessor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(2000));
			Assert.ThrowsAsync<TaskCanceledException>(async () => await delayProcessor.ProcessAsync(new Exception(), CatchBlockProcessErrorInfo.FromRetry(1), cancelTokenSource.Token));
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_DelayErrorProcessorSubclass_ProcessAsyncMethod_Work()
		{
			var delayProcessor = new YourDelayErrorProcessor(TimeSpan.Zero);
			await delayProcessor.ProcessAsync(new Exception(), CatchBlockProcessErrorInfo.FromRetry(1), default(CancellationToken));
			Assert.AreEqual(1, delayProcessor.CurRetry);
			Assert.AreEqual(PolicyAlias.Retry, delayProcessor.PolicyKind);
		}

		public class YourDelayErrorProcessor : DelayErrorProcessor
		{
			public YourDelayErrorProcessor(TimeSpan timeSpan): base(timeSpan){}

			public override Task<Exception> ProcessAsync(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
			{
				CurRetry = catchBlockProcessErrorInfo.CurrentRetryCount;
				PolicyKind = catchBlockProcessErrorInfo.PolicyKind;
				return base.ProcessAsync(error, catchBlockProcessErrorInfo, configAwait, cancellationToken);
			}

			public int CurRetry { get; private set; }

			public PolicyAlias PolicyKind { get; private set; }
		}
	}
}