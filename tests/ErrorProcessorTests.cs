using NUnit.Framework;
using NUnit.Framework.Legacy;
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
				ClassicAssert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException);
				ClassicAssert.IsTrue(m == 1);
			}
			else if (sync)
			{
				var processor = new BasicErrorProcessor(syncAction);
				processor.Process(processedException);
				ClassicAssert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException);
				ClassicAssert.IsTrue(n == 2);

				ClassicAssert.IsTrue(m == 0);
			}
			else
			{
				var processor = new BasicErrorProcessor(asyncFunc);
				processor.Process(processedException);
				ClassicAssert.IsTrue(m == 1);

				await processor.ProcessAsync(processedException);
				ClassicAssert.IsTrue(m == 2);

				ClassicAssert.IsTrue(n == 0);
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
				ClassicAssert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException, errorInfo);
				ClassicAssert.IsTrue(m == 1);
			}
			else if (sync)
			{
				var processor = new DefaultErrorProcessor(syncAction);
				processor.Process(processedException, errorInfo);
				ClassicAssert.IsTrue(n == 1);

				await processor.ProcessAsync(processedException, errorInfo);
				ClassicAssert.IsTrue(n == 2);

				ClassicAssert.IsTrue(m == 0);
			}
			else
			{
				var processor = new DefaultErrorProcessor(asyncFunc);
				processor.Process(processedException, errorInfo);
				ClassicAssert.IsTrue(m == 1);

				await processor.ProcessAsync(processedException, errorInfo);
				ClassicAssert.IsTrue(m == 2);

				ClassicAssert.IsTrue(n == 0);
			}

			ClassicAssert.AreEqual(2, infoCounter);
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(true, true)]
		[TestCase(false, true)]
		public void Should_DefaultErrorProcessor_TParam_Process_Only_ProcessingErrorInfo_TParam(bool isGeneric, bool withCancelType)
		{
			int i = 0;
			DefaultErrorProcessor<int> errPr = null;
			if (!withCancelType)
			{
				errPr = new DefaultErrorProcessor<int>((_, __) => i++);
			}
			else
			{
				errPr = new DefaultErrorProcessor<int>((_, __) => i++, CancellationType.Precancelable);
			}

			ProcessingErrorInfo piToTest = null;
			if (isGeneric)
			{
				piToTest = new ProcessingErrorInfo<int>(new ProcessingErrorContext<int>(PolicyAlias.NotSet, 1));
			}
			else
			{
				piToTest = new ProcessingErrorInfo(PolicyAlias.NotSet);
			}
			errPr.Process(new Exception(), piToTest);

			Assert.That(i, Is.EqualTo(isGeneric ? 1 : 0));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_DefaultErrorProcessor_TParam_Of_Action_With_TokenParam_Process_Only_ProcessingErrorInfo_TParam(bool isGeneric)
		{
			int i = 0;
			DefaultErrorProcessor<int> errPr = new DefaultErrorProcessor<int>((_, __,  ___) => i++);

			ProcessingErrorInfo piToTest = null;
			if (isGeneric)
			{
				piToTest = new ProcessingErrorInfo<int>(new ProcessingErrorContext<int>(PolicyAlias.NotSet, 1));
			}
			else
			{
				piToTest = new ProcessingErrorInfo(PolicyAlias.NotSet);
			}
			errPr.Process(new Exception(), piToTest);

			Assert.That(i, Is.EqualTo(isGeneric ? 1 : 0));
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(true, true)]
		[TestCase(false, true)]
		public async Task Should_DefaultErrorProcessor_TParam_ProcessAsync_Only_ProcessingErrorInfo_TParam(bool isGeneric, bool withCancelType)
		{
			int i = 0;
			DefaultErrorProcessor<int> errPr = null;
			if (!withCancelType)
			{
				errPr = new DefaultErrorProcessor<int>(async (_, __) => { await Task.Delay(1); i++; });
			}
			else
			{
				errPr = new DefaultErrorProcessor<int>(async (_, __) => { await Task.Delay(1); i++; }, CancellationType.Precancelable);
			}
			ProcessingErrorInfo piToTest = null;
			if (isGeneric)
			{
				piToTest = new ProcessingErrorInfo<int>(new ProcessingErrorContext<int>(PolicyAlias.NotSet, 1));
			}
			else
			{
				piToTest = new ProcessingErrorInfo(PolicyAlias.NotSet);
			}
			await errPr.ProcessAsync(new Exception(), piToTest);

			Assert.That(i, Is.EqualTo(isGeneric ? 1 : 0));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_DefaultErrorProcessor_TParam_Of_Action_With_TokenParam_ProcessAsync_Only_ProcessingErrorInfo_TParam(bool isGeneric)
		{
			int i = 0;
			DefaultErrorProcessor<int> errPr = new DefaultErrorProcessor<int>(async (_, __, ___) => { await Task.Delay(1); i++; });

			ProcessingErrorInfo piToTest = null;
			if (isGeneric)
			{
				piToTest = new ProcessingErrorInfo<int>(new ProcessingErrorContext<int>(PolicyAlias.NotSet, 1));
			}
			else
			{
				piToTest = new ProcessingErrorInfo(PolicyAlias.NotSet);
			}
			await errPr.ProcessAsync(new Exception(), piToTest);
			Assert.That(i, Is.EqualTo(isGeneric ? 1 : 0));
		}
	}
}
