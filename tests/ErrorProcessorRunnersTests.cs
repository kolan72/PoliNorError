using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class ErrorProcessorRunnersTests
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ErrorProcessorFromSyncRunner_Ctor_With_Not_Cancelable_Action_Always_Processed_As_NotCancelable(bool withToken)
		{
			int i = 0;
			void act(Exception _, Unit __) => ++i;

			var notCancelableRunner = new ErrorProcessorFromSyncRunner<Unit>(act);
			var ex = new Exception();

			if (!withToken)
			{
				var resAsyncWithNoCancelToken = await notCancelableRunner.RunAsync(ex, Unit.Default, false);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableActionNoToken, resAsyncWithNoCancelToken);
				ClassicAssert.AreEqual(1, i);

				var resSyncWithNoCancelToken = notCancelableRunner.Run(ex, Unit.Default);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableActionNoToken, resSyncWithNoCancelToken);
				ClassicAssert.AreEqual(2, i);
			}
			else
			{
				var cancelSource = new CancellationTokenSource();

				var resSyncWithCancelToken = notCancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableActionTokenExists, resSyncWithCancelToken);
				ClassicAssert.AreEqual(1, i);

				var resAsyncWithCancelToken = await notCancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableActionTokenExists, resAsyncWithCancelToken);
				ClassicAssert.AreEqual(2, i);

				cancelSource.Dispose();
			}
		}

		[Test]
		public async Task Should_ErrorProcessorFromSyncRunner_Ctor_With_Cancelable_Action_Always_Processed_As_Cancelable()
		{
			int i = 0;
			void act(Exception _, Unit __) => ++i;
			var ex = new Exception();
			var cancelSource = new CancellationTokenSource();

			var cancelableRunner = new ErrorProcessorFromSyncRunner<Unit>(act, CancellationType.Cancelable);

			var resAsyncWithCancelTokenCancelable = await cancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
			ClassicAssert.AreEqual(ErrorProcessorRunResult.CancelableFuncTokenExists, resAsyncWithCancelTokenCancelable);
			ClassicAssert.AreEqual(1, i);

			var resSyncWithCancelTokenCancelable = cancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
			ClassicAssert.AreEqual(ErrorProcessorRunResult.CancelableActionTokenExists, resSyncWithCancelTokenCancelable);
			ClassicAssert.AreEqual(2, i);

			cancelSource.Dispose();
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ErrorProcessorFromSyncRunner_Ctor_With_Action_With_Token_Always_Processed_As_Action(bool aSync)
		{
			int i = 0;
			void act(Exception _, Unit __, CancellationToken ___) => ++i;
			var ex = new Exception();
			var cancelSource = new CancellationTokenSource();
			var cancelableRunner = new ErrorProcessorFromSyncRunner<Unit>(act);

			const ErrorProcessorRunResult runResult = ErrorProcessorRunResult.CancelableActionTokenExists;
			if (aSync)
			{
				var resAsyncWithCancelTokenCancelable = await cancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
				ClassicAssert.AreEqual(runResult, resAsyncWithCancelTokenCancelable);
			}
			else
			{
				var resSyncWithCancelTokenCancelable = cancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
				ClassicAssert.AreEqual(runResult, resSyncWithCancelTokenCancelable);
			}

			ClassicAssert.AreEqual(1, i);
			cancelSource.Dispose();
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ErrorProcessorFromAsyncRunner_Ctor_With_Not_Cancelable_Func_Always_Processed_As_NotCancelable(bool withToken)
		{
			int i = 0;
			async Task act(Exception _, Unit __)  { await Task.Delay(1); ++i; }

			var notCancelableRunner = new ErrorProcessorFromAsyncRunner<Unit>(act);
			var ex = new Exception();

			if (!withToken)
			{
				var resAsyncWithNoCancelToken = await notCancelableRunner.RunAsync(ex, Unit.Default, false);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableFuncNoToken, resAsyncWithNoCancelToken);
				ClassicAssert.AreEqual(1, i);

				var resSyncWithNoCancelToken = notCancelableRunner.Run(ex, Unit.Default);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableActionNoToken, resSyncWithNoCancelToken);
				ClassicAssert.AreEqual(2, i);
			}
			else
			{
				var cancelSource = new CancellationTokenSource();

				var resSyncWithCancelToken = notCancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableActionTokenExists, resSyncWithCancelToken);
				ClassicAssert.AreEqual(1, i);

				var resAsyncWithCancelToken = await notCancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
				ClassicAssert.AreEqual(ErrorProcessorRunResult.NotCancelableFuncTokenExists, resAsyncWithCancelToken);
				ClassicAssert.AreEqual(2, i);

				cancelSource.Dispose();
			}
		}

		[Test]
		public async Task Should_ErrorProcessorFromAsyncRunner_Ctor_With_Cancelable_Func_Always_Processed_As_Cancelable()
		{
			int i = 0;
			async Task act(Exception _, Unit __) { await Task.Delay(1); ++i; }

			var ex = new Exception();
			var cancelSource = new CancellationTokenSource();

			var cancelableRunner = new ErrorProcessorFromAsyncRunner<Unit>(act, CancellationType.Cancelable);

			var resAsyncWithCancelTokenCancelable = await cancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
			ClassicAssert.AreEqual(ErrorProcessorRunResult.CancelableFuncTokenExists, resAsyncWithCancelTokenCancelable);
			ClassicAssert.AreEqual(1, i);

			var resSyncWithCancelTokenCancelable = cancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
			ClassicAssert.AreEqual(ErrorProcessorRunResult.CancelableActionTokenExists, resSyncWithCancelTokenCancelable);
			ClassicAssert.AreEqual(2, i);

			cancelSource.Dispose();
		}
	}
}
