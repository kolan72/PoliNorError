using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class ErrorProcessorTests
	{
		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		public async Task Should_BasicErrorProcessor_Sync_And_Async_Part_Work(bool sync, bool aSync)
		{
			int m = 0;
			async Task asyncFunc(Exception _) { await Task.Delay(1); ++m; }

			int n = 0;
			void syncAction(Exception _) => ++n;

			var processedException = new Exception();

			if (sync && aSync)
			{
				var processor = new BasicErrorProcessor(asyncFunc, syncAction);
				processor.Process(processedException);
				Assert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException);
				Assert.IsTrue(m == 1);
			}
			else if (sync)
			{
				var processor = new BasicErrorProcessor(syncAction);
				processor.Process(processedException);
				Assert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException);
				Assert.IsTrue(n == 2);

				Assert.IsTrue(m == 0);
			}
			else
			{
				var processor = new BasicErrorProcessor(asyncFunc);
				processor.Process(processedException);
				Assert.IsTrue(m == 1);

				await processor.ProcessAsync(processedException);
				Assert.IsTrue(m == 2);

				Assert.IsTrue(n == 0);
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		public async Task Should_DefaultErrorProcessor_Sync_And_Async_Part_Work(bool sync, bool aSync)
		{
			int infoCounter = 0;
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple);

			int m = 0;
			async Task asyncFunc(Exception _, ProcessingErrorInfo info) { await Task.Delay(1); ++m; if (info.PolicyKind == PolicyAlias.Simple) ++infoCounter; }

			int n = 0;
			void syncAction(Exception _, ProcessingErrorInfo info) { ++n; if (info.PolicyKind == PolicyAlias.Simple) ++infoCounter; }

			var processedException = new Exception();

			if (sync && aSync)
			{
				var processor = new DefaultErrorProcessor(asyncFunc, syncAction);
				processor.Process(processedException, errorInfo);
				Assert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException, errorInfo);
				Assert.IsTrue(m == 1);
			}
			else if (sync)
			{
				var processor = new DefaultErrorProcessor(syncAction);
				processor.Process(processedException, errorInfo);
				Assert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException, errorInfo);
				Assert.IsTrue(n == 2);

				Assert.IsTrue(m == 0);
			}
			else
			{
				var processor = new DefaultErrorProcessor(asyncFunc);
				processor.Process(processedException, errorInfo);
				Assert.IsTrue(m == 1);

				await processor.ProcessAsync(processedException, errorInfo);
				Assert.IsTrue(m == 2);

				Assert.IsTrue(n == 0);
			}

			Assert.AreEqual(2, infoCounter);
		}
	}
}
