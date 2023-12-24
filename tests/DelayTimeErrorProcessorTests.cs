using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
			var res = await delayProcessor.ProcessAsync(exc, ProcessingErrorInfo.FromRetry(1), CancellationToken.None);
			ClassicAssert.IsTrue(exc.Equals(res));
		}

		[Test]
		public void Should_BeCalled_In_ProcessMethod_With_No_Delegate()
		{
			var delayProcessor = new DelayErrorProcessor(TimeSpan.Zero);
			var exc = new Exception();
			var res = delayProcessor.Process(exc, ProcessingErrorInfo.FromRetry(1), CancellationToken.None);
			ClassicAssert.IsTrue(exc.Equals(res));
		}

		[Test]
		public void Should_SleepProvider_BeCalled_In_ProcessMethod()
		{
			var funcMock = Substitute.For<Func<int, Exception, TimeSpan>>();
			funcMock(Arg.Any<int>(), Arg.Any<Exception>()).Returns(new TimeSpan());
			var delayProcessor = new DelayErrorProcessor(funcMock);
			var exc = new Exception();
			delayProcessor.Process(exc, ProcessingErrorInfo.FromRetry(1), CancellationToken.None);
			funcMock.Received(1)(0, exc);
		}

		[Test]
		public async Task Should_SleepProvider_BeCalled_In_ProcessAsyncMethod()
		{
			var funcMock = Substitute.For<Func<int, Exception, TimeSpan>>();
			funcMock(Arg.Any<int>(), Arg.Any<Exception>()).Returns(new TimeSpan());
			var delayProcessor = new DelayErrorProcessor(funcMock);
			var exc = new Exception();
			await delayProcessor.ProcessAsync(exc, ProcessingErrorInfo.FromRetry(1), CancellationToken.None);
			funcMock.Received(1)(0, exc);
		}

		[Test]
		public void Should_Wait_ForSimpleDelay()
		{
			var processor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1500));
			var testExc = new Exception();
			var sw = Stopwatch.StartNew();
			var resError = processor.Process(testExc, ProcessingErrorInfo.FromRetry(1), default);
			sw.Stop();
			ClassicAssert.GreaterOrEqual(Math.Floor(sw.Elapsed.TotalSeconds), 1);
			ClassicAssert.AreEqual(resError, testExc);
		}

		[Test]
		public void Should_Process_Throw_TaskCanceledException_If_Canceled()
		{
			var processor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
			var testExc = new Exception();
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(100);
			ClassicAssert.Throws<OperationCanceledException>(() => processor.Process(testExc, ProcessingErrorInfo.FromRetry(1), cancelTokenSource.Token));
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_Wait_ForTaskDelay()
		{
			var processor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
			var testExc = new Exception();
			var sw = Stopwatch.StartNew();
			Exception resError = await processor.ProcessAsync(testExc, ProcessingErrorInfo.FromRetry(1), CancellationToken.None);
			sw.Stop();
			ClassicAssert.Greater(Math.Floor(sw.Elapsed.TotalMilliseconds), 500);
			ClassicAssert.AreEqual(resError, testExc);
		}

		[Test]
		public void Should_Delegate_BeCalled_In_ProcessAsyncMethod_With_CancelError()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.CancelAfter(1000);
			var delayProcessor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(2000));
			ClassicAssert.ThrowsAsync<TaskCanceledException>(async () => await delayProcessor.ProcessAsync(new Exception(), ProcessingErrorInfo.FromRetry(1), cancelTokenSource.Token));
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_DelayErrorProcessorSubclass_ProcessAsyncMethod_Work()
		{
			var delayProcessor = new YourDelayErrorProcessor(TimeSpan.Zero);
			await delayProcessor.ProcessAsync(new Exception(), ProcessingErrorInfo.FromRetry(1), default(CancellationToken));
			ClassicAssert.AreEqual(1, delayProcessor.CurRetry);
			ClassicAssert.AreEqual(PolicyAlias.Retry, delayProcessor.PolicyKind);
		}

		public class YourDelayErrorProcessor : DelayErrorProcessor
		{
			public YourDelayErrorProcessor(TimeSpan timeSpan): base(timeSpan){}

			public override Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
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