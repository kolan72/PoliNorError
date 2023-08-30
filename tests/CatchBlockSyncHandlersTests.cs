using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class CatchBlockSyncHandlersTests
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
				var res1 = handler.Handle(new Exception("Test1"), EmptyErrorContext.Default());
				Assert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = handler.Handle(new Exception("Test2"), EmptyErrorContext.Default());
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
				var res1 = await handler.HandleAsync(new Exception("Test1"), EmptyErrorContext.Default());
				Assert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = await handler.HandleAsync(new Exception("Test2"), EmptyErrorContext.Default());
				Assert.AreEqual(HandleCatchBlockResult.Canceled, res2);
			}
		}
	}
}
