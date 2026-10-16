using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class DelayTimeErrorProcessorTests
	{
		[Test]
		public void Should_Wait_Start_From_ZeroRetry()
		{
			var delayProvider = new FakeDelayProvider();
			var delayProcessor = new DelayErrorProcessor((_, __) => TimeSpan.FromTicks(1), delayProvider);
			var policy = new RetryPolicy(1)
						.WithWait(delayProcessor);
			policy.Handle(() => throw new InvalidOperationException());
			Assert.That(delayProvider.NumOfCalls, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Apply_Exception_Dependent_Delay(bool firstExceptionDelay)
		{
			bool? firstDelayFlag = null;
			bool? secondDelayFlag = null;

			Exception errorToHandle;
			if (firstExceptionDelay)
			{
				errorToHandle = new InvalidCastException();
			}
			else
			{
				errorToHandle = new InvalidOperationException();
			}

			TimeSpan func(int _, Exception ex)
			{
				switch (ex)
				{
					case InvalidCastException _:
						firstDelayFlag = true;
						break;
					case InvalidOperationException _:
						secondDelayFlag = true;
						break;
				}
				return TimeSpan.FromTicks(1);
			}

			var sp = new SimplePolicy().WithErrorProcessor(new DelayErrorProcessor(func));
			sp.Handle(() => throw errorToHandle);
			if (firstExceptionDelay)
			{
				Assert.That(firstDelayFlag, Is.True);
				Assert.That(secondDelayFlag, Is.Null);
			}
			else
			{
				Assert.That(secondDelayFlag, Is.True);
				Assert.That(firstDelayFlag, Is.Null);
			}
		}

		[Test]
		[TestCase(1)]
		[TestCase(2)]
		public void Should_UseRetryDelayForSleepDuration_In_Process_WhenInitializedWithRetryDelay(int numOfRetry)
		{
			const int baseMsTime = 2;
			var innerDelay = LinearRetryDelay.Create(TimeSpan.FromMilliseconds(baseMsTime));
			var retryDelay = new LinearRetryDelayThatStoreTime(innerDelay);
			var processor = new DelayErrorProcessor(retryDelay);

			processor.Process(new Exception(), new RetryProcessingErrorInfo(numOfRetry));

			Assert.That(retryDelay.Delay, Is.EqualTo(innerDelay.GetDelay(numOfRetry)));
		}

		[Test]
		public void Should_Respect_RetryCount_When_Using_ErrorProcessor_With_Context_And_With_DelayProcessor()
		{
			const int baseTime = 2;
			var innerDelay = LinearRetryDelay.Create(TimeSpan.FromTicks(baseTime));
			var retryDelay = new LinearRetryDelayThatStoreTime(innerDelay);
			var processor = new DelayErrorProcessor(retryDelay);

			processor.Process(new Exception(), new RetryProcessingErrorInfo<int>(new RetryProcessingErrorContext<int>(1, 4)));

			Assert.That(retryDelay.Delay, Is.EqualTo(innerDelay.GetDelay(1)));
		}

		[Test]
		[TestCase(1)]
		[TestCase(2)]
		public async Task Should_UseRetryDelayForSleepDuration_In_ProcessAsync_WhenInitializedWithRetryDelay(int numOfRetry)
		{
			const int baseMsTime = 2;
			var innerDelay = LinearRetryDelay.Create(TimeSpan.FromMilliseconds(baseMsTime));
			var retryDelay = new LinearRetryDelayThatStoreTime(innerDelay);
			var processor = new DelayErrorProcessor(retryDelay);

 			await processor.ProcessAsync(new Exception(), new RetryProcessingErrorInfo(numOfRetry));

			Assert.That(retryDelay.Delay, Is.EqualTo(innerDelay.GetDelay(numOfRetry)));
		}

		public class YourDelayErrorProcessor : DelayErrorProcessor
		{
			public YourDelayErrorProcessor(TimeSpan timeSpan): base(timeSpan){}

			public override Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
			{
				if (catchBlockProcessErrorInfo is RetryProcessingErrorInfo retryProcessingErrorInfo)
				{
					CurRetry = retryProcessingErrorInfo.RetryCount;
				}
				else
				{
					CurRetry = -1;
				}
				PolicyKind = catchBlockProcessErrorInfo.PolicyKind;
				return base.ProcessAsync(error, catchBlockProcessErrorInfo, configAwait, cancellationToken);
			}

			public int CurRetry { get; private set; }

			public PolicyAlias PolicyKind { get; private set; }
		}
	}
}