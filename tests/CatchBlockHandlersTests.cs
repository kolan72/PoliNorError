using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class CatchBlockHandlersTests
	{
		[Test]
		public void Should_PolicyProcessorCatchBlockSyncHandler_Be_Cancelable()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Simple);
			bulkProcessor.AddProcessor(new BasicErrorProcessor());
			var filter = new PolicyProcessor.ExceptionFilter();
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var handler = new PolicyProcessorCatchBlockSyncHandler<Unit>(new PolicyResult(),
																		     bulkProcessor,
																			 cancelTokenSource.Token,
																			 filter.GetCanHandle()
																			 );
				var res1 = handler.Handle(new Exception("Test1"), EmptyErrorContext.Default);
				Assert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = handler.Handle(new Exception("Test2"), EmptyErrorContext.Default);
				Assert.AreEqual(HandleCatchBlockResult.Canceled, res2);
			}
		}

		[Test]
		public async Task Should_PolicyProcessorCatchBlockAsyncHandler_Be_Cancelable()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Simple);
			bulkProcessor.AddProcessor(new BasicErrorProcessor());
			var filter = new PolicyProcessor.ExceptionFilter();
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var handler = new PolicyProcessorCatchBlockAsyncHandler<Unit>(new PolicyResult(),
																			 bulkProcessor,
																			 false,
																			 cancelTokenSource.Token,
																			 filter.GetCanHandle()
																			 );
				var res1 = await handler.HandleAsync(new Exception("Test1"), EmptyErrorContext.Default);
				Assert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = await handler.HandleAsync(new Exception("Test2"), EmptyErrorContext.Default);
				Assert.AreEqual(HandleCatchBlockResult.Canceled, res2);
			}
		}

		[Test]
		[TestCase(3, HandleCatchBlockResult.FailedByPolicyRules, true)]
		[TestCase(1, HandleCatchBlockResult.Success, true)]
		[TestCase(3, HandleCatchBlockResult.FailedByPolicyRules, false)]
		[TestCase(1, HandleCatchBlockResult.Success, false)]
		public async Task Should_PolicyProcessorCatchBlockHandler_When_ErrorContext_CanNotBeHandled_Returns_FailedByPolicyRules(int retryCount, HandleCatchBlockResult result, bool sync)
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Simple);
			bulkProcessor.AddProcessor(new BasicErrorProcessor());
			var filter = new PolicyProcessor.ExceptionFilter();

			if (sync)
			{
				var handler = new PolicyProcessorCatchBlockSyncHandler<RetryContext>(new PolicyResult(),
																 bulkProcessor,
																 default,
																 filter.GetCanHandle(),
																 (exCtx) => exCtx.Context.CurrentRetryCount < 2
																 );
				Assert.AreEqual(result, handler.Handle(new Exception(), new RetryErrorContext(new RetryContext(retryCount))));
			}
			else
			{
				var handler = new PolicyProcessorCatchBlockAsyncHandler<RetryContext>(new PolicyResult(),
															 bulkProcessor,
															 false,
															 default,
															 filter.GetCanHandle(),
															 (exCtx) => exCtx.Context.CurrentRetryCount < 2
															 );
				Assert.AreEqual(result, await handler.HandleAsync(new Exception(), new RetryErrorContext(new RetryContext(retryCount))));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Exception_In_ErrorFilters_Be_Stored_In_CatchBlockErrors(bool include)
		{
			var simplePolicy = new SimplePolicy();
			bool testFn(Exception _) => throw new Exception("Error in filter");
			if (include)
			{
				simplePolicy.IncludeError((Func<Exception, bool>)testFn);
			}
			else
			{
				simplePolicy.ExcludeError((Func<Exception, bool>)testFn);
			}
			void action() => throw new Exception("Test");
			var res = simplePolicy.Handle(action);
			Assert.IsTrue(res.ErrorFilterUnsatisfied);
			Assert.NotNull(res.UnprocessedError);
			Assert.AreEqual(1, res.CatchBlockErrors.Count());
			Assert.AreEqual(CatchBlockExceptionSource.ErrorFilter, res.CatchBlockErrors.FirstOrDefault().ExceptionSource);
		}
	}
}
