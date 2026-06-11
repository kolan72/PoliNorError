using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class BulkErrorProcessorTests
	{
		[Test]
		public async Task Should_ProcessAsync_Return_Status_None_When_No_Processors()
		{
			var bulkProcessor = new BulkErrorProcessor();
			var res =  await bulkProcessor.ProcessAsync(new Exception(), new RetryProcessingErrorContext(1), CancellationToken.None);
			ClassicAssert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public async Task Should_ProcessAsync_Return_Status_ProcessorException_When_ProcessorWithError()
		{
			var bulkProcessor = new BulkErrorProcessor();

			var mockedErrorProcessor = Substitute.For<IErrorProcessor>();

			var exc = new Exception();

			mockedErrorProcessor.ProcessAsync(Arg.Any<Exception>(), Arg.Any<ProcessingErrorInfo>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).ThrowsAsync(exc);

			bulkProcessor.AddProcessor(mockedErrorProcessor);

			var res = await bulkProcessor.ProcessAsync(new Exception(), new RetryProcessingErrorContext(1), default);
			ClassicAssert.IsTrue(res.ProcessErrors.Count() == 1);
		}

		[Test]
		public async Task Should_ProcessAsync_Return_Status_Success_And_CorrectProcessor()
		{
			var bulkProcessor = new BulkErrorProcessor();

			var exc = new Exception();

			var mockedErrorProcessor = Substitute.For<IErrorProcessor>();
			mockedErrorProcessor.ProcessAsync(Arg.Any<Exception>(), Arg.Any<ProcessingErrorInfo>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(exc);

			bulkProcessor.AddProcessor(mockedErrorProcessor);
			var res = await bulkProcessor.ProcessAsync(new Exception(), new RetryProcessingErrorContext(1), default);
			ClassicAssert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public void Should_Process_Return_Status_None_When_No_Processors()
		{
			var bulkProcessor = new BulkErrorProcessor();
			var res = bulkProcessor.Process(new Exception(), new RetryProcessingErrorContext(1), CancellationToken.None);
			ClassicAssert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public void Should_Process_When_ProcessingErrorContext_Param_Is_Null()
		{
			bool processorFlag = false;
			PolicyAlias? policyAlias = null;
			var handlingError = new InvalidOperationException();
			var bulkProcessor = new BulkErrorProcessor().WithErrorProcessorOf((Exception _, ProcessingErrorInfo pi) =>
			{
			  policyAlias = pi.PolicyKind;
			  processorFlag = true;
			});
			var res = bulkProcessor.Process(handlingError, null, CancellationToken.None);
			Assert.That(res.HandlingError, Is.EqualTo(handlingError));
			Assert.That(policyAlias, Is.EqualTo(PolicyAlias.NotSet));
			Assert.That(processorFlag, Is.True);
		}

		[Test]
		public async Task Should_ProcessAsync_When_ProcessingErrorContext_Param_Is_Null()
		{
			bool processorFlag = false;
			PolicyAlias? policyAlias = null;
			var handlingError = new InvalidOperationException();
			var bulkProcessor = new BulkErrorProcessor().WithErrorProcessorOf((Exception _, ProcessingErrorInfo pi) =>
			{
				policyAlias = pi.PolicyKind;
				processorFlag = true;
			});
			var res = await bulkProcessor.ProcessAsync(handlingError, null, CancellationToken.None);
			Assert.That(res.HandlingError, Is.EqualTo(handlingError));
			Assert.That(policyAlias, Is.EqualTo(PolicyAlias.NotSet));
			Assert.That(processorFlag, Is.True);
		}

		[Test]
		public void Should_Process_Return_Status_ProcessorException_When_ProcessorWithError()
		{
			var bulkProcessor = new BulkErrorProcessor();

			var exc = new Exception();

			var mockedErrorProcessor = Substitute.For<IErrorProcessor>();
			mockedErrorProcessor.Process(Arg.Any<Exception>(), Arg.Any<ProcessingErrorInfo>(), Arg.Any<CancellationToken>()).Throws(exc);

			bulkProcessor.AddProcessor(mockedErrorProcessor);

			var res = bulkProcessor.Process(new Exception(), new RetryProcessingErrorContext(1), default);
			ClassicAssert.IsTrue(res.ProcessErrors.Count() == 1);
		}

		[Test]
		public void Should_Process_Return_Status_Success_When_CorrectProcessor()
		{
			var bulkProcessor = new BulkErrorProcessor();

			var exc = new Exception();

			var mockedErrorProcessor = Substitute.For<IErrorProcessor>();
			mockedErrorProcessor.Process(Arg.Any<Exception>(), Arg.Any<ProcessingErrorInfo>(), Arg.Any<CancellationToken>()).Returns(exc);

			bulkProcessor.AddProcessor(mockedErrorProcessor);
			var res = bulkProcessor.Process(new Exception(), new RetryProcessingErrorContext(1), default);
			ClassicAssert.IsTrue(!res.ProcessErrors.Any());
		}

		[Test]
		public void Should_Process_NotCallOtherProcessor_If_Canceled()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var bulkProcessor = new BulkErrorProcessor();
				cancelTokenSource.CancelAfter(500);
				var delayProcessor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
				bulkProcessor.AddProcessor(delayProcessor);
				bulkProcessor.AddProcessor(new BasicErrorProcessor());

				var res = bulkProcessor.Process(new Exception(), new RetryProcessingErrorContext(1), cancelTokenSource.Token);
				ClassicAssert.IsTrue(res.ProcessErrors.Count() == 1);
				ClassicAssert.IsTrue(res.ProcessErrors.FirstOrDefault().InnerException?.GetType().Equals(typeof(OperationCanceledException)));
				ClassicAssert.IsTrue(res.IsCanceled);
			}
		}

		[Test]
		public async Task Should_ProcessAsync_NotCallOtherProcessor_If_Canceled()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var bulkProcessor = new BulkErrorProcessor();
				cancelTokenSource.CancelAfter(500);
				var delayProcessor = new DelayErrorProcessor(TimeSpan.FromMilliseconds(1000));
				bulkProcessor.AddProcessor(delayProcessor);
				bulkProcessor.AddProcessor(new BasicErrorProcessor());

				var res = await bulkProcessor.ProcessAsync(new Exception(), new RetryProcessingErrorContext(1), cancelTokenSource.Token);
				ClassicAssert.IsTrue(res.ProcessErrors.Count() == 1);
				//				The real type here id TaskCanceledException.
				ClassicAssert.IsTrue(res.ProcessErrors.FirstOrDefault().InnerException?.GetType().BaseType.Equals(typeof(OperationCanceledException)));
				ClassicAssert.IsTrue(res.IsCanceled);
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple, true, true)]
		[TestCase(PolicyAlias.Fallback, true, true)]
		[TestCase(PolicyAlias.Retry, true, true)]
		[TestCase(PolicyAlias.Simple, true, false)]
		[TestCase(PolicyAlias.Fallback, true, false)]
		[TestCase(PolicyAlias.Retry, true, false)]
		[TestCase(PolicyAlias.Simple, false, false)]
		[TestCase(PolicyAlias.Fallback, false, false)]
		[TestCase(PolicyAlias.Retry, false, false)]
		[TestCase(PolicyAlias.Simple, false, true)]
		[TestCase(PolicyAlias.Fallback, false, true)]
		[TestCase(PolicyAlias.Retry, false, true)]
		public async Task Should_BulkErrorProcessor_Without_Alias_Can_Be_Used_By_Policies(PolicyAlias policyAlias, bool sync, bool generic)
		{
			int i = 0;
			void act(Exception _, ProcessingErrorInfo errorInfo)
			{
				if (errorInfo.PolicyKind == policyAlias)
					i++;
			}

			var bulkErrorProcessor = new BulkErrorProcessor()
										.WithErrorProcessorOf(act);
			var policy = GetPolicyByAlias();
			if (sync)
			{
				if (generic)
				{
					policy.Handle<int>(() => throw new Exception("Test"));
				}
				else
				{
					policy.Handle(() => throw new Exception("Test"));
				}
			}
			else
			{
				if (generic)
				{
					await policy.HandleAsync<int>(async (_) => { await Task.Delay(1); throw new Exception("Test"); });
				}
				else
				{
					await policy.HandleAsync(async (_) => { await Task.Delay(1); throw new Exception("Test"); });
				}
			}

			ClassicAssert.AreEqual(1, i);

			IPolicyBase GetPolicyByAlias()
			{
				switch (policyAlias)
				{
					case PolicyAlias.Simple: return new SimplePolicy(bulkErrorProcessor);
					case PolicyAlias.Fallback: return new FallbackPolicy(bulkErrorProcessor);
					case PolicyAlias.Retry: return new RetryPolicy(1, bulkErrorProcessor);
					default: throw new NotImplementedException();
				}
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(false, true)]
		public async Task Should_WithInnerErrorProcessor_HandleError_Correctly(bool sync, bool withCancellationType)
		{
			var processor = new BulkErrorProcessor();
			var innerProcessors = new InnerErrorProcessorFuncs();

			var policy = new SimplePolicy(processor);

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action);
				}

				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc);
				}

				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.I, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken);
				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken);
				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.J, Is.EqualTo(1));

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo);
				}
				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo);
				}
				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.K, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken);
				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken);
				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.L, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_BulkErrorProcessor_Process_Generic_DefaultErrorProcessor_Created_By_Action(bool syncRun)
		{
			var bulkProcessor = new BulkErrorProcessor();

			const int contextParam = 2;
			var processingErrorContext = new ProcessingErrorContext<int>(PolicyAlias.NotSet, contextParam);

			bool errorProcessorWorksFlag = false;
			var errorProcessor = new DefaultErrorProcessor<int>((_, pir) =>
			{
				if (pir.Param == contextParam)
				{
					errorProcessorWorksFlag = true;
				}
			});

			bool errorProcessorThatShoulNotWorkFlag = false;

			var errorProcessorThatShoulNotWork = new DefaultErrorProcessor<string>((_, __) => errorProcessorThatShoulNotWorkFlag = true);

			bulkProcessor.AddProcessor(errorProcessor);
			bulkProcessor.AddProcessor(errorProcessorThatShoulNotWork);

			if (syncRun)
			{
				bulkProcessor.Process(new Exception(), processingErrorContext);
			}
			else
			{
				await bulkProcessor.ProcessAsync(new Exception(), processingErrorContext);
			}

			Assert.That(errorProcessorWorksFlag, Is.True);
			Assert.That(errorProcessorThatShoulNotWorkFlag, Is.False);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_BulkErrorProcessor_Process_Generic_DefaultErrorProcessor_Created_By_AsyncFunc(bool syncRun)
		{
			var bulkProcessor = new BulkErrorProcessor();

			const int contextParam = 2;
			var processingErrorContext = new ProcessingErrorContext<int>(PolicyAlias.NotSet, contextParam);

			bool errorProcessorWorksFlag = false;
			var errorProcessor = new DefaultErrorProcessor<int>(async (_, pir) =>
			{
				await Task.Delay(1);
				if (pir.Param == contextParam)
				{
					errorProcessorWorksFlag = true;
				}
			});

			bool errorProcessorThatShoulNotWorkFlag = false;

			var errorProcessorThatShoulNotWork = new DefaultErrorProcessor<string>((_, __) => errorProcessorThatShoulNotWorkFlag = true);

			bulkProcessor.AddProcessor(errorProcessor);
			bulkProcessor.AddProcessor(errorProcessorThatShoulNotWork);

			if (syncRun)
			{
				bulkProcessor.Process(new Exception(), processingErrorContext);
			}
			else
			{
				await bulkProcessor.ProcessAsync(new Exception(), processingErrorContext);
			}

			Assert.That(errorProcessorWorksFlag, Is.True);
			Assert.That(errorProcessorThatShoulNotWorkFlag, Is.False);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_AddErrorContextProcessor_Using_Action(bool shouldWork, bool withCancellationType)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			SimplePolicyProcessor processor;
			BulkErrorProcessor bp;

			if (!withCancellationType)
			{
				bp = new BulkErrorProcessor().WithErrorContextProcessorOf<int>(action);
			}
			else
			{
				bp = new BulkErrorProcessor().WithErrorContextProcessorOf<int>(action, CancellationType.Precancelable);
			}
			processor = new SimplePolicyProcessor(bp);

			PolicyResult result = null;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddErrorContextProcessor_Using_Action_With_Token(bool shouldWork)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(new BulkErrorProcessor()
				.WithErrorContextProcessorOf<int>(action));

			PolicyResult result;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_AddErrorContextProcessor_Using_AsyncFunc(bool shouldWork, bool withCancellationType)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi)
			{
				await Task.Delay(1);
				m = pi.Param;
			}

			SimplePolicyProcessor processor;
			BulkErrorProcessor bp;

			if (!withCancellationType)
			{
				bp = new BulkErrorProcessor().WithErrorContextProcessorOf<int>(fn);
			}
			else
			{
				bp = new BulkErrorProcessor().WithErrorContextProcessorOf<int>(fn, CancellationType.Precancelable);
			}

			processor = new SimplePolicyProcessor(bp);

			PolicyResult result = null;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddErrorContextProcessor_Using_AsyncFunc_With_Token(bool shouldWork)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				await Task.Delay(1);
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(
				new BulkErrorProcessor()
				.WithErrorContextProcessorOf<int>(fn));

			PolicyResult result;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Apply_Delay_When_Configured_WithDelayBetweenRetries(bool firstExceptionDelay)
		{
			int firstErrorRetryCount = 0;
			int secondErrorRetryCount = 0;

			Exception errorToHandle;
			if (firstExceptionDelay)
			{
				errorToHandle = new InvalidCastException();
			}
			else
			{
				errorToHandle = new InvalidOperationException();
			}

			TimeSpan func(int retryCount, Exception ex)
			{
				switch (ex)
				{
					case InvalidCastException _:
						firstErrorRetryCount = retryCount;
						break;
					case InvalidOperationException _:
						secondErrorRetryCount = retryCount;
						break;
				}
				return TimeSpan.FromTicks(1);
			}
			var bp = new BulkErrorProcessor().WithDelayBetweenRetries(func);
			var pr = new DefaultRetryProcessor(bp);
			pr.Retry(() => throw errorToHandle, 2);
			if (firstExceptionDelay)
			{
				Assert.That(firstErrorRetryCount, Is.EqualTo(1));
				Assert.That(secondErrorRetryCount, Is.EqualTo(0));
			}
			else
			{
				Assert.That(secondErrorRetryCount, Is.EqualTo(1));
				Assert.That(firstErrorRetryCount, Is.EqualTo(0));
			}
		}

		[Test]
		public void Should_CreateDelayProcessor_When_ConfiguredWithTimeSpanDelay()
		{
			var bp = new BulkErrorProcessor().WithDelayBetweenRetries(TimeSpan.FromTicks(1));
			Assert.That(bp.Count, Is.EqualTo(1));
			Assert.That(bp.ElementAt(0), Is.TypeOf<DelayErrorProcessor>());
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddErrorContextProcessor_Using_DefaultErrorProcessor(bool shouldWork)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				m = pi.Param;
			}

			var ep = new DefaultErrorProcessor<int>(action);

			var processor = new SimplePolicyProcessor(new BulkErrorProcessor()
				.WithErrorContextProcessor(ep));

			PolicyResult result;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public async Task Should_Cancel_ProcessAsync_When_ErrorProcessor_Cancels_Using_Same_Token()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				void actionToHandle(Exception ex, CancellationToken ct) => SyncActionThatCanceledOnOuterAndThrowOnInner(ex, ct, cancelTokenSource);

				var processor = new BulkErrorProcessor().WithErrorProcessorOf(actionToHandle);

				var result = await processor.ProcessAsync(new Exception(), token: cancelTokenSource.Token);
				Assert.That(result.ProcessErrors.FirstOrDefault().ErrorStatus, Is.EqualTo(BulkErrorProcessor.ProcessStatus.Canceled));
				Assert.That(result.IsCanceled, Is.True);
			}
		}

		[Test]
		[TestCase(CancellationTests.TestCancellationMode.OperationCanceled)]
		[TestCase(CancellationTests.TestCancellationMode.Aggregate)]
		public void Should_Cancel_Process_When_ErrorProcessor_Cancels_Using_Same_Token(CancellationTests.TestCancellationMode cancellationMode)
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				Action<Exception, CancellationToken> actionToHandle = null;
				if (cancellationMode == CancellationTests.TestCancellationMode.OperationCanceled)
				{
					actionToHandle = (ex, ct) => SyncActionThatCanceledOnOuterAndThrowOnInner(ex, ct, cancelTokenSource);
				}
				else
				{
					actionToHandle = (ex, ct) => SyncActionThatCanceledOnOuterAndThrowOnInnerAndThrowAgregateExc(ex, ct, cancelTokenSource);
				}

				var processor = new BulkErrorProcessor().WithErrorProcessorOf(actionToHandle);

				var result = processor.Process(new Exception(), token: cancelTokenSource.Token);
				Assert.That(result.ProcessErrors.FirstOrDefault().ErrorStatus, Is.EqualTo(BulkErrorProcessor.ProcessStatus.Canceled));
				Assert.That(result.IsCanceled, Is.True);
			}
		}

		[Test]
		public void Should_Process_Handle_Processor_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var testProcessor1 = new TestErrorProcessor();

			var originalException = new InvalidOperationException("Test exception");
			var processorException = new ArgumentException("Processor exception");

			testProcessor1.SetThrowException(processorException);
			bulkProcessor.AddProcessor(testProcessor1);

			// Act
			var result = bulkProcessor.Process(originalException);

			// Assert
			Assert.That(result.HandlingError, Is.SameAs(originalException));
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(1));

			var errorProcessorException = result.ProcessErrors.First();
			Assert.That(errorProcessorException.InnerException, Is.SameAs(processorException));
			Assert.That(errorProcessorException.ErrorProcessor, Is.SameAs(testProcessor1));
			Assert.That(errorProcessorException.ErrorStatus, Is.EqualTo(BulkErrorProcessor.ProcessStatus.Faulted));
		}

		[Test]
		public void Should_Process_Handle_Cancellation_Token_Between_Processors()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var testProcessor1 = new TestErrorProcessor();
			var testProcessor2 = new TestErrorProcessor();

			var originalException = new InvalidOperationException("Test exception");
			using (var cts = new CancellationTokenSource())
			{
				testProcessor1.SetCancelAfterProcessing(cts);
				bulkProcessor.AddProcessor(testProcessor1);
				bulkProcessor.AddProcessor(testProcessor2);

				// Act
				var result = bulkProcessor.Process(originalException, null, cts.Token);
				// Assert
				Assert.That(result.HandlingError, Is.SameAs(originalException));
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(testProcessor1.ProcessedExceptions.Count, Is.EqualTo(1));
				Assert.That(testProcessor2.ProcessedExceptions.Count, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_ProcessAsync_Handle_Cancellation_Between_Processors()
		{
			var bulkProcessor = new BulkErrorProcessor();
			var testProcessor1 = new TestErrorProcessor();
			var testProcessor2 = new TestErrorProcessor();

			var originalException = new InvalidOperationException("Test exception");
			using (var cts = new CancellationTokenSource())
			{
				testProcessor1.SetCancelAfterProcessingNonSync(cts);
				bulkProcessor.AddProcessor(testProcessor1);
				bulkProcessor.AddProcessor(testProcessor2);

				// Act
				var result = await bulkProcessor.ProcessAsync(originalException, null, false, cts.Token);
				// Assert
				Assert.That(result.HandlingError, Is.SameAs(originalException));
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(testProcessor1.ProcessedExceptionsAsync.Count, Is.EqualTo(1));
				Assert.That(testProcessor2.ProcessedExceptionsAsync.Count, Is.EqualTo(0));
			}
		}

		[Test]
		public void Should_Process_Handle_Cancellation_Token_Before_Processing()
		{
			// Arrange
			var originalException = new InvalidOperationException("Test exception");

			var bulkProcessor = new BulkErrorProcessor();
			var testProcessor1 = new TestErrorProcessor();

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				bulkProcessor.AddProcessor(testProcessor1);

				// Act
				var result = bulkProcessor.Process(originalException, null, cts.Token);

				// Assert
				Assert.That(result.HandlingError, Is.SameAs(originalException));
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.ProcessErrors.Count(), Is.EqualTo(1));
				Assert.That(result.IsCanceledBetweenProcessors, Is.True);
				Assert.That(result.CancellationException, Is.Not.Null);
			}
		}

		[Test]
		public async Task Should_ProcessAsync_Handle_Processor_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var testProcessor1 = new TestErrorProcessor();

			var originalException = new InvalidOperationException("Test exception");
			var processorException = new ArgumentException("Processor exception");

			testProcessor1.SetThrowExceptionNonSync(processorException);
			bulkProcessor.AddProcessor(testProcessor1);

			// Act
			var result = await bulkProcessor.ProcessAsync(originalException);

			// Assert
			Assert.That(result.HandlingError, Is.SameAs(originalException));
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(1));

			var errorProcessorException = result.ProcessErrors.First();
			Assert.That(errorProcessorException.InnerException, Is.SameAs(processorException));
			Assert.That(errorProcessorException.ErrorProcessor, Is.SameAs(testProcessor1));
			Assert.That(errorProcessorException.ErrorStatus, Is.EqualTo(BulkErrorProcessor.ProcessStatus.Faulted));
		}

		[Test]
		public async Task Should_ProcessAsync_Handle_Cancellation_Token_Before_Processing()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var testProcessor = new TestErrorProcessor();
			var originalException = new InvalidOperationException("Test exception");
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				bulkProcessor.AddProcessor(testProcessor);

				// Act
				var result = await bulkProcessor.ProcessAsync(originalException, null, false, cts.Token);

				// Assert
				Assert.That(result.HandlingError, Is.SameAs(originalException));
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.ProcessErrors.Count(), Is.EqualTo(1));
				Assert.That(result.IsCanceledBetweenProcessors, Is.True);
				Assert.That(result.CancellationException, Is.Not.Null);
			}
		}

		[Test]
		public async Task Should_ProcessAsync_Handle_Null_Exception()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var testProcessor1 = new TestErrorProcessor();

		    bulkProcessor.AddProcessor(testProcessor1);

		    // Act
		    var result = await bulkProcessor.ProcessAsync(null);

		    // Assert
		    Assert.That(result.HandlingError, Is.Null);
		    Assert.That(testProcessor1.ProcessedExceptionsAsync, Contains.Item(null));
		}

		/// <summary>
		/// Tests for WithTypedErrorProcessorOf methods
		/// </summary>
		[Test]
		public void Should_WithTypedErrorProcessorOf_Action_WithoutCancellationToken_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    bulkProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
		        (InvalidOperationException ex, ProcessingErrorInfo _) =>
		        {
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		        });

		    var result = bulkProcessor.Process(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_Action_WithCancellationToken_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    bulkProcessor.WithTypedErrorProcessorOf<InvalidOperationException>(
		        (InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken ct) =>
		        {
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		            Assert.That(ct, Is.Not.Null);
		        });

		    var result = bulkProcessor.Process(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_Action_WithCancellationType_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    bulkProcessor.WithTypedErrorProcessorOf(
		        (InvalidOperationException ex, ProcessingErrorInfo _) =>
		        {
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		        },
		        CancellationType.Precancelable);

		    var result = bulkProcessor.Process(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_AsyncFunc_WithoutCancellationToken_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    bulkProcessor.WithTypedErrorProcessorOf(
		        async (InvalidOperationException ex, ProcessingErrorInfo _) =>
		        {
		            await Task.Delay(1);
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		        });

		    var result = await bulkProcessor.ProcessAsync(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_AsyncFunc_WithCancellationType_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    bulkProcessor.WithTypedErrorProcessorOf(
		        async (InvalidOperationException ex, ProcessingErrorInfo _) =>
		        {
		            await Task.Delay(1);
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		        },
		        CancellationType.Precancelable);

		    var result = await bulkProcessor.ProcessAsync(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public async Task Should_WithTypedErrorProcessorOf_AsyncFunc_WithCancellationToken_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    bulkProcessor.WithTypedErrorProcessorOf(
		        async (InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken ct) =>
		        {
		            await Task.Delay(1);
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		            Assert.That(ct, Is.Not.Null);
		        });

		    var result = await bulkProcessor.ProcessAsync(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithTypedErrorProcessor_HandleException()
		{
		    // Arrange
		    var bulkProcessor = new BulkErrorProcessor();
		    var exceptionHandled = false;
		    var testException = new InvalidOperationException("Test exception");

		    // Act
		    var typedProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>(
		        (InvalidOperationException ex, ProcessingErrorInfo _) =>
		        {
		            exceptionHandled = true;
		            Assert.That(ex.Message, Is.EqualTo("Test exception"));
		        });

		    bulkProcessor.WithTypedErrorProcessor(typedProcessor);

		    var result = bulkProcessor.Process(testException);

		    // Assert
		    Assert.That(exceptionHandled, Is.True);
		    Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Handle_Only_Inner_Exception_Of_Specified_Type()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var innerExceptionHandled = false;

			// Create an exception with a specific inner exception type
			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);

			// Act - Add processor for InvalidOperationException
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					(InvalidOperationException ex, ProcessingErrorInfo _) =>
					{
						innerExceptionHandled = true;
						Assert.That(ex.Message, Is.EqualTo("Inner exception"));
					}));

			// Process exception with matching inner exception type
			var result1 = bulkProcessor.Process(mainException);

			// Assert
			Assert.That(innerExceptionHandled, Is.True);
			Assert.That(result1.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public async Task Should_WithInnerErrorProcessor_Async_Handle_Only_Inner_Exception_Of_Specified_Type()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var innerExceptionHandled = false;

			// Create an exception with a specific inner exception type
			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);

			// Act - Add async processor for InvalidOperationException
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					async (InvalidOperationException ex, ProcessingErrorInfo _) =>
					{
						await Task.Delay(1);
						innerExceptionHandled = true;
						Assert.That(ex.Message, Is.EqualTo("Inner exception"));
					}));

			// Process exception with matching inner exception type
			var result = await bulkProcessor.ProcessAsync(mainException);

			// Assert
			Assert.That(innerExceptionHandled, Is.True);
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Handle_Exception_Without_Matching_Inner_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var processorCalled = false;

			// Create an exception without the target inner exception type
			var nonMatchingInnerException = new ArgumentException("Wrong type");
			var mainException = new AggregateException("Main exception", nonMatchingInnerException);

			// Act - Add processor for InvalidOperationException (but we have ArgumentException as inner)
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					(InvalidOperationException _, ProcessingErrorInfo __) =>
					{
						processorCalled = true; // Should not be called
					}));

			// Process exception with non-matching inner exception type
			var result = bulkProcessor.Process(mainException);

			// Assert - Processor should not be called since inner exception type doesn't match
			Assert.That(processorCalled, Is.False);
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Handle_Exception_Without_Inner_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var processorCalled = false;

			// Create an exception without any inner exception
			var mainException = new InvalidOperationException("Main exception without inner");

			// Act - Add processor for any type (it shouldn't be called since there's no inner exception)
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<ArgumentException>(
					(ArgumentException _, ProcessingErrorInfo __) =>
					{
						processorCalled = true; // Should not be called
					}));

			// Process exception without inner exception
			bulkProcessor.Process(mainException);

			// Assert - Processor should not be called since there's no inner exception of the target type
			Assert.That(processorCalled, Is.False);
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Work_With_Multiple_Inner_Exception_Types()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var invalidOperationExceptionHandled = false;
			var argumentExceptionHandled = false;

			// Create an exception with multiple inner exceptions where only one matches
			var innerException1 = new InvalidOperationException("First inner");
			var mainException = new AggregateException("Main exception", innerException1);

			// Act - Add processors for both types
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					(InvalidOperationException ex, ProcessingErrorInfo _) =>
					{
						invalidOperationExceptionHandled = true;
						Assert.That(ex.Message, Is.EqualTo("First inner"));
					}));

			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<ArgumentException>(
					(ArgumentException _, ProcessingErrorInfo __) =>
					{
						argumentExceptionHandled = true; // Should not be called
					}));

			// Process exception with matching inner exception type
			var result = bulkProcessor.Process(mainException);

			// Assert - Only the matching processor should be called
			Assert.That(invalidOperationExceptionHandled, Is.True);
			Assert.That(argumentExceptionHandled, Is.False);
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_With_CancellationType_Handle_Inner_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var innerExceptionHandled = false;

			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);

			// Act - Add processor with CancellationType.Precancelable
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					(InvalidOperationException ex, ProcessingErrorInfo _) =>
					{
						innerExceptionHandled = true;
						Assert.That(ex.Message, Is.EqualTo("Inner exception"));
					},
					CancellationType.Precancelable));

			var result = bulkProcessor.Process(mainException);

			// Assert
			Assert.That(innerExceptionHandled, Is.True);
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public async Task Should_WithInnerErrorProcessor_Async_With_CancellationToken_Handle_Inner_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var innerExceptionHandled = false;

			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);

			// Act - Add async processor with CancellationToken overload
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					async (InvalidOperationException ex, ProcessingErrorInfo _, CancellationToken ct) =>
					{
						await Task.Delay(1, ct);
						innerExceptionHandled = true;
						Assert.That(ex.Message, Is.EqualTo("Inner exception"));
					}));

			var result = await bulkProcessor.ProcessAsync(mainException);

			// Assert
			Assert.That(innerExceptionHandled, Is.True);
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public async Task Should_WithInnerErrorProcessor_Async_With_CancellationType_Handle_Inner_Exception()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var innerExceptionHandled = false;

			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);

			// Act - Add async processor with CancellationType.Precancelable
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					async (InvalidOperationException ex, ProcessingErrorInfo _) =>
					{
						await Task.Delay(1);
						innerExceptionHandled = true;
						Assert.That(ex.Message, Is.EqualTo("Inner exception"));
					},
					CancellationType.Precancelable));

			var result = await bulkProcessor.ProcessAsync(mainException);

			// Assert
			Assert.That(innerExceptionHandled, Is.True);
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Propagate_ProcessingErrorInfo()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			ProcessingErrorInfo capturedErrorInfo = null;

			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);
			var processingErrorContext = new RetryProcessingErrorContext(1);

			// Act
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					(InvalidOperationException _, ProcessingErrorInfo pi) =>
					{
						capturedErrorInfo = pi;
					}));

			var result = bulkProcessor.Process(mainException, processingErrorContext);

			// Assert
			Assert.That(capturedErrorInfo, Is.Not.Null);
			Assert.That(capturedErrorInfo.PolicyKind, Is.EqualTo(PolicyAlias.Retry));
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Handle_Exception_From_Inner_Processor()
		{
			// Arrange
			var bulkProcessor = new BulkErrorProcessor();
			var processorException = new ArgumentException("Processor error");

			var innerException = new InvalidOperationException("Inner exception");
			var mainException = new AggregateException("Main exception", innerException);

			// Act
			bulkProcessor.WithInnerErrorProcessor(
				new DefaultInnerErrorProcessor<InvalidOperationException>(
					(InvalidOperationException _, ProcessingErrorInfo __) =>
					{
						throw processorException;
					}));

			var result = bulkProcessor.Process(mainException);

			// Assert
			Assert.That(result.ProcessErrors.Count(), Is.EqualTo(1));
			Assert.That(result.ProcessErrors.First().InnerException, Is.EqualTo(processorException));
		}

		internal class TestErrorProcessor : IErrorProcessor
		{
			public List<Exception> ProcessedExceptions { get; } = new List<Exception>();
			public List<Exception> ProcessedExceptionsAsync { get; } = new List<Exception>();
			public List<ProcessingErrorInfo> ReceivedContexts { get; } = new List<ProcessingErrorInfo>();
			public List<ProcessingErrorInfo> ReceivedContextsAsync { get; } = new List<ProcessingErrorInfo>();
			public List<bool> ConfigAwaitValues { get; } = new List<bool>();

			private Exception _throwException;
			private Exception _throwExceptionAsync;
			private CancellationTokenSource _cancelAfterProcessing ;
			private CancellationTokenSource _cancelAfterProcessingAsync;

			public void SetCancelAfterProcessing(CancellationTokenSource cts) => _cancelAfterProcessing = cts;
			public void SetCancelAfterProcessingNonSync(CancellationTokenSource cts) => _cancelAfterProcessingAsync = cts;

			public void SetThrowException(Exception exception) => _throwException = exception;
			public void SetThrowExceptionNonSync(Exception exception) => _throwExceptionAsync = exception;

			public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
			{
				ProcessedExceptions.Add(error);
				if (catchBlockProcessErrorInfo != null)
					ReceivedContexts.Add(catchBlockProcessErrorInfo);

				if (_throwException != null)
					throw _throwException;

				// Cancel after processing to simulate cancellation between processors
				_cancelAfterProcessing?.Cancel();

				return error;
			}

			public Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
			{
				ProcessedExceptionsAsync.Add(error);
				ConfigAwaitValues.Add(configAwait);
				if (catchBlockProcessErrorInfo != null)
					ReceivedContextsAsync.Add(catchBlockProcessErrorInfo);

				if (_throwExceptionAsync != null)
					throw _throwExceptionAsync;

				// Cancel after processing to simulate cancellation between processors
				_cancelAfterProcessingAsync?.Cancel();

				return Task.FromResult(error);
			}
		}

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
		public static void SyncActionThatCanceledOnOuterAndThrowOnInner(Exception exception, CancellationToken outerToken, CancellationTokenSource outerTokenSource)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.
		{
			using (var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(outerToken))
			{
				var innerToken = cancelTokenSource.Token;
				outerTokenSource.Cancel();
				innerToken.ThrowIfCancellationRequested();
			}
		}

		public static void SyncActionThatCanceledOnOuterAndThrowOnInnerAndThrowAgregateExc(Exception exception, CancellationToken outerToken, CancellationTokenSource outerTokenSource)
		{
			ActionThatCanceledOnOuterAndThrowOnInner(exception, outerToken, outerTokenSource).Wait();
		}

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
		public static async Task ActionThatCanceledOnOuterAndThrowOnInner(Exception exception, CancellationToken outerToken, CancellationTokenSource outerTokenSource)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.
		{
			await Task.Delay(TimeSpan.FromTicks(1));
			using (var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(outerToken))
			{
				var innerToken = cancelTokenSource.Token;
				outerTokenSource.Cancel();
				innerToken.ThrowIfCancellationRequested();
			}
		}
	}
}