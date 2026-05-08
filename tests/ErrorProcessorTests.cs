using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class ErrorProcessorTests
	{
		private readonly Exception testException = new Exception("Test exception");
		private readonly ProcessingErrorInfo testErrorInfo = new ProcessingErrorInfo(new ProcessingErrorContext());

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
			DefaultErrorProcessor<int> errPr = new DefaultErrorProcessor<int>((_, __, ___) => i++);

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

		[Test]
		[TestCase(true, true, true)]
		[TestCase(false, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, false, true)]
		[TestCase(true, true, false)]
		[TestCase(false, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		public async Task Should_DefaultTypedErrorProcessor_Of_Action_With_TokenParam_Process_Only_Typed_Exception(bool errCanBeProcessed, bool isSync, bool withCancelType)
		{
			int i = 0;

			DefaultTypedErrorProcessor<ArgumentException> processor;
			if (withCancelType)
			{
				processor = new DefaultTypedErrorProcessor<ArgumentException>((ex, _) => { if (ex.ParamName == "Test") i++; }, CancellationType.Precancelable);
			}
			else
			{
				processor = new DefaultTypedErrorProcessor<ArgumentException>((ex, _, __) => { if (ex.ParamName == "Test") i++; });
			}

			Exception exToTest = null;

			if (errCanBeProcessed)
			{
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
				exToTest = new ArgumentException("", "Test");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			}
			else
			{
				exToTest = new Exception("");
			}

			if (isSync)
			{
				processor.Process(exToTest, new ProcessingErrorInfo(PolicyAlias.NotSet));
			}
			else
			{
				await processor.ProcessAsync(exToTest, new ProcessingErrorInfo(PolicyAlias.NotSet));
			}
			Assert.That(i, Is.EqualTo(errCanBeProcessed ? 1 : 0));
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(false, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, false, true)]
		[TestCase(true, true, false)]
		[TestCase(false, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		public async Task Should_DefaultTypedErrorProcessor_Of_Func_With_TokenParam_Process_Only_Typed_Exception(bool errCanBeProcessed, bool isSync, bool withCancelType)
		{
			int i = 0;

			DefaultTypedErrorProcessor<ArgumentException> processor;
			if (withCancelType)
			{
				processor = new DefaultTypedErrorProcessor<ArgumentException>(async (ex, _) => { await Task.Delay(1); if (ex.ParamName == "Test") i++; }, CancellationType.Precancelable);
			}
			else
			{
				processor = new DefaultTypedErrorProcessor<ArgumentException>(async (ex, _) => { await Task.Delay(1); if (ex.ParamName == "Test") i++; });
			}

			Exception exToTest = null;

			if (errCanBeProcessed)
			{
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
				exToTest = new ArgumentException("", "Test");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			}
			else
			{
				exToTest = new Exception("");
			}

			if (isSync)
			{
				processor.Process(exToTest, new ProcessingErrorInfo(PolicyAlias.NotSet));
			}
			else
			{
				await processor.ProcessAsync(exToTest, new ProcessingErrorInfo(PolicyAlias.NotSet));
			}
			Assert.That(i, Is.EqualTo(errCanBeProcessed ? 1 : 0));
		}

		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public async Task Should_DefaultTypedErrorProcessor_Of_Func_With_CancelToken_Process_Only_Typed_Exception(bool errCanBeProcessed, bool isSync)
		{
			int i = 0;

			var processor = new DefaultTypedErrorProcessor<ArgumentException>(async (ex, _,  __) => { await Task.Delay(1); if (ex.ParamName == "Test") i++; });

			Exception exToTest = null;

			if (errCanBeProcessed)
			{
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
				exToTest = new ArgumentException("", "Test");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			}
			else
			{
				exToTest = new Exception("");
			}

			if (isSync)
			{
				processor.Process(exToTest, new ProcessingErrorInfo(PolicyAlias.NotSet));
			}
			else
			{
				await processor.ProcessAsync(exToTest, new ProcessingErrorInfo(PolicyAlias.NotSet));
			}
			Assert.That(i, Is.EqualTo(errCanBeProcessed ? 1 : 0));
		}

		#region DefaultErrorProcessorConstructorTests
		[Test]
		public void Should_DefaultErrorProcessor_Internal_Constructor_Create_Instance()
		{
			var processor = new DefaultErrorProcessor();
			Assert.That(processor, Is.Not.Null);
			Assert.That(processor, Is.InstanceOf<DefaultErrorProcessor>());
		}

		[Test]
		public void Should_DefaultErrorProcessor_Action_Constructor_Set_Sync_Runner()
		{
			bool actionCalled = false;
			var processor = new DefaultErrorProcessor((ex, errorInfo) =>
			{
				actionCalled = true;
				Assert.That(ex, Is.SameAs(testException));
				Assert.That(errorInfo, Is.SameAs(testErrorInfo));
			});

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);
		}

		[Test]
		public void Should_DefaultErrorProcessor_Action_With_CancellationToken_Constructor_Set_Sync_Runner()
		{
			bool actionCalled = false;
			var processor = new DefaultErrorProcessor((ex, errorInfo, cancellationToken) =>
			{
				actionCalled = true;
				Assert.That(ex, Is.SameAs(testException));
				Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				Assert.That(cancellationToken, Is.EqualTo(default(CancellationToken)));
			});

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);
		}

		[Test]
		public void Should_DefaultErrorProcessor_Action_With_CancellationType_Constructor_Set_Sync_Runner()
		{
			bool actionCalled = false;
			var processor = new DefaultErrorProcessor((ex, errorInfo) =>
			{
				actionCalled = true;
				Assert.That(ex, Is.SameAs(testException));
				Assert.That(errorInfo, Is.SameAs(testErrorInfo));
			}, CancellationType.Precancelable);

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_Constructor_Set_Async_Runner()
		{
			bool funcCalled = false;
			var processor = new DefaultErrorProcessor(async (ex, errorInfo) =>
			{
				funcCalled = true;
				await Task.Delay(1);
				Assert.That(ex, Is.SameAs(testException));
				Assert.That(errorInfo, Is.SameAs(testErrorInfo));
			});

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_With_CancellationToken_Constructor_Set_Async_Runner()
		{
			bool funcCalled = false;
			var processor = new DefaultErrorProcessor(async (ex, errorInfo, cancellationToken) =>
			{
				funcCalled = true;
				await Task.Delay(1);
				Assert.That(ex, Is.SameAs(testException));
				Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				Assert.That(cancellationToken, Is.EqualTo(default(CancellationToken)));
			});

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_With_CancellationType_Constructor_Set_Async_Runner()
		{
			bool funcCalled = false;
			var processor = new DefaultErrorProcessor(async (ex, errorInfo) =>
			{
				funcCalled = true;
				await Task.Delay(1);
				Assert.That(ex, Is.SameAs(testException));
				Assert.That(errorInfo, Is.SameAs(testErrorInfo));
			}, CancellationType.Precancelable);

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_And_Action_Constructor_Set_Both_Runners()
		{
			bool actionCalled = false;
			bool funcCalled = false;

			var processor = new DefaultErrorProcessor(
				async (ex, errorInfo) =>
				{
					funcCalled = true;
					await Task.Delay(1);
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				},
				(ex, errorInfo) =>
				{
					actionCalled = true;
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				});

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);

			// Reset for async test
			actionCalled = false;
			funcCalled = false;

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_And_Action_With_CancellationType_Constructor_Set_Both_Runners()
		{
			bool actionCalled = false;
			bool funcCalled = false;

			var processor = new DefaultErrorProcessor(
				async (ex, errorInfo) =>
				{
					funcCalled = true;
					await Task.Delay(1);
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				},
				(ex, errorInfo) =>
				{
					actionCalled = true;
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				},
				CancellationType.Precancelable);

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);

			// Reset for async test
			actionCalled = false;
			funcCalled = false;

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_And_Action_Constructor_Set_Both_Runners_Async()
		{
			bool actionCalled = false;
			bool funcCalled = false;

			var processor = new DefaultErrorProcessor(
				async (ex, errorInfo) =>
				{
					funcCalled = true;
					await Task.Delay(1);
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				},
				(ex, errorInfo) =>
				{
					actionCalled = true;
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				});

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);
		}

		[Test]
		public async Task Should_DefaultErrorProcessor_Func_And_Action_With_CancellationType_Constructor_Set_Both_Runners_Async()
		{
			bool actionCalled = false;
			bool funcCalled = false;

			var processor = new DefaultErrorProcessor(
				async (ex, errorInfo) =>
				{
					funcCalled = true;
					await Task.Delay(1);
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				},
				(ex, errorInfo) =>
				{
					actionCalled = true;
					Assert.That(ex, Is.SameAs(testException));
					Assert.That(errorInfo, Is.SameAs(testErrorInfo));
				},
				CancellationType.Precancelable);

			await processor.ProcessAsync(testException, testErrorInfo);
			Assert.That(funcCalled, Is.True);

			processor.Process(testException, testErrorInfo);
			Assert.That(actionCalled, Is.True);
		}
		#endregion
	}
}