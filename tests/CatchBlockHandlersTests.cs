using NUnit.Framework;
using NUnit.Framework.Legacy;
using PoliNorError.TryCatch;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

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
				ClassicAssert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = handler.Handle(new Exception("Test2"), EmptyErrorContext.Default);
				ClassicAssert.AreEqual(HandleCatchBlockResult.Canceled, res2);
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
				ClassicAssert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = await handler.HandleAsync(new Exception("Test2"), EmptyErrorContext.Default);
				ClassicAssert.AreEqual(HandleCatchBlockResult.Canceled, res2);
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
				ClassicAssert.AreEqual(result, handler.Handle(new Exception(), new RetryErrorContext(new RetryContext(retryCount))));
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
				ClassicAssert.AreEqual(result, await handler.HandleAsync(new Exception(), new RetryErrorContext(new RetryContext(retryCount))));
			}
		}

		[Test]
		[TestCase(PolicyAlias.Simple, true)]
		[TestCase(PolicyAlias.Simple, false)]
		[TestCase(PolicyAlias.Retry, true)]
		[TestCase(PolicyAlias.Retry, false)]
		public void Should_Exception_In_ErrorFilters_Be_Stored_In_CatchBlockErrors(PolicyAlias policyAlias, bool include)
		{
			IPolicyBase policy = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					policy = new SimplePolicy();
					break;
				case PolicyAlias.Retry:
					policy = new RetryPolicy(10);
					break;
			}

			bool testFn(Exception _) => throw new Exception("Error in filter");
			if (include)
			{
				((Policy)policy).IncludeError((Func<Exception, bool>)testFn);
			}
			else
			{
				((Policy)policy).ExcludeError((Func<Exception, bool>)testFn);
			}
			void action() => throw new Exception("Test");
			var res = policy.Handle(action);
			ClassicAssert.IsTrue(res.ErrorFilterUnsatisfied);
			ClassicAssert.NotNull(res.UnprocessedError);
			ClassicAssert.AreEqual(1, res.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(CatchBlockExceptionSource.ErrorFilter, res.CatchBlockErrors.FirstOrDefault().ExceptionSource);
			Assert.That(res.Errors.Count(), Is.EqualTo(1));
			Assert.That(res.IsFailed, Is.EqualTo(true));
			Assert.That(res.CatchBlockErrors.FirstOrDefault().IsCritical, Is.EqualTo(true));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CatchBlockHandler_InitByFilter_Correctly(bool forAll)
		{
			CatchBlockHandler handler;
			Exception errorToHandler;
			if (forAll)
			{
				handler = CatchBlockHandlerFactory.ForAllExceptions();
				errorToHandler = TestExceptionHolder.TestException;
			}
			else
			{
				handler = CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<NullReferenceException>());
				errorToHandler = new NullReferenceException();
			}
			var result = handler.ErrorFilter.GetCanHandle()(errorToHandler);
			Assert.That(result, Is.True);
		}

		[Test]
		public void Should_CatchBlockHandler_Initialized_Correctly_By_Including_ErrorSet()
		{
			var errorSet = ErrorSet.FromError<NullReferenceException>();
			var handler = CatchBlockHandlerFactory.FilterExceptionsByIncluding(errorSet);
			var result = handler.ErrorFilter.GetCanHandle()(new NullReferenceException());
			Assert.That(result, Is.True);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_CatchBlockHandler_Initialized_Correctly_By_Excluding_ErrorSet(bool canHandle)
		{
			var errorSet = ErrorSet.FromError<NullReferenceException>();
			var handler = CatchBlockHandlerFactory.FilterExceptionsByExcluding(errorSet);
			bool result;
			if (canHandle)
			{
				result = handler.ErrorFilter.GetCanHandle()(new InvalidOperationException());
			}
			else
			{
				result = handler.ErrorFilter.GetCanHandle()(new NullReferenceException());
			}
			Assert.That(result, Is.EqualTo(canHandle));
		}

		[Test]
		public void Should_NonEmptyCatchBlockFilter_ToCatchBlockHandler_Method_Create_Handler_Correctly()
		{
			var handler = NonEmptyCatchBlockFilter
							.CreateByIncluding<NullReferenceException>()
							.ToCatchBlockHandler();
			Assert.That(handler, Is.Not.Null);
			Assert.That(handler.CatchBlockFilter, Is.Not.Null);
		}

		[Test]
		public void Should_CatchBlockForAllHandler_ToTryCatch_Create_ITryCatch_WithOneCatchBlockHandler()
		{
			var tryCatch = CatchBlockHandlerFactory.ForAllExceptions()
							.ToTryCatch();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);
		}

		[Test]
		public void Should_CatchBlockFilteredHandler_ToTryCatch_Create_ITryCatch_WithOneCatchBlockHandler()
		{
			var tryCatch = NonEmptyCatchBlockFilter
						.CreateByIncluding<NullReferenceException>()
						.ToCatchBlockHandler()
						.ToTryCatch();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_CatchBlockFilteredHandler_ToTryCatchBuilder_Create_ITryCatch_WithOneCatchBlockHandler()
		{
			var builder = NonEmptyCatchBlockFilter
						.CreateByIncluding<NullReferenceException>()
						.ToCatchBlockHandler()
						.ToTryCatchBuilder();
			Assert.That(builder, Is.TypeOf<TryCatchBuilder>());
			var tryCatch = builder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
		}
	}
}
