using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	internal class CatchBlockSyncHandlersTests
	{
		[Test]
		public void Should_DefaultPolicyProcessorCatchBlockSyncHandler_Be_Cancelable()
		{
			var bulkProcessor = new BulkErrorProcessor(PolicyAlias.Simple);
			bulkProcessor.AddProcessor(new BasicErrorProcessor());
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var handler = new DefaultPolicyProcessorCatchBlockSyncHandler(new PolicyResult(),
																		     bulkProcessor,
																			 new PolicyProcessor.ExceptionFilter(),
																			 cancelTokenSource.Token);
				var res1 = handler.Handle(new Exception("Test1"), EmptyErrorContext.Default());
				Assert.AreEqual(HandleCatchBlockResult.Success, res1);

				cancelTokenSource.Cancel();
				var res2 = handler.Handle(new Exception("Test2"), EmptyErrorContext.Default());
				Assert.AreEqual(HandleCatchBlockResult.Canceled, res2);
			}
		}
	}
}
