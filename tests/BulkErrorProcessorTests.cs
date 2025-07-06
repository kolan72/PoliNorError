using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
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
	}
}