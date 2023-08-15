using NUnit.Framework;
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
		public async Task Should_Ctor_With_Not_Cancelable_Action_Always_Processed_As_NotCancelable(bool withToken)
		{
			int i = 0;
			void act(Exception _, Unit __) => ++i;

			var notCancelableRunner = new ErrorProcessorFromSyncRunner<Unit>(act);
			var ex = new Exception();

			if (!withToken)
			{
				var resAsyncWithNoCancelToken = await notCancelableRunner.RunAsync(ex, Unit.Default, false);
				Assert.AreEqual(ErrorProcessorRunResul.NotCancelableActionNoToken, resAsyncWithNoCancelToken);
				Assert.AreEqual(1, i);

				var resSyncWithNoCancelToken = notCancelableRunner.Run(ex, Unit.Default);
				Assert.AreEqual(ErrorProcessorRunResul.NotCancelableActionNoToken, resSyncWithNoCancelToken);
				Assert.AreEqual(2, i);
			}
			else
			{
				var cancelSource = new CancellationTokenSource();

				var resSyncWithCancelToken = notCancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
				Assert.AreEqual(ErrorProcessorRunResul.NotCancelableActionTokenExists, resSyncWithCancelToken);
				Assert.AreEqual(1, i);

				var resAsyncWithCancelToken = await notCancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
				Assert.AreEqual(ErrorProcessorRunResul.NotCancelableActionTokenExists, resAsyncWithCancelToken);
				Assert.AreEqual(2, i);

				cancelSource.Dispose();
			}
		}

		[Test]
		public async Task Should_Ctor_With_Cancelable_Action_Always_ProcessedAsync_As_CancelableFunc()
		{
			int i = 0;
			void act(Exception _, Unit __) => ++i;
			var ex = new Exception();
			var cancelSource = new CancellationTokenSource();

			var cancelableRunner = new ErrorProcessorFromSyncRunner<Unit>(act, CancellationType.Cancelable);

			var resAsyncWithCancelTokenCancelable = await cancelableRunner.RunAsync(ex, Unit.Default, false, cancelSource.Token);
			Assert.AreEqual(ErrorProcessorRunResul.CancelableFuncTokenExists, resAsyncWithCancelTokenCancelable);
			Assert.AreEqual(1, i);

			var resSyncWithCancelTokenCancelable = cancelableRunner.Run(ex, Unit.Default, cancelSource.Token);
			Assert.AreEqual(ErrorProcessorRunResul.CancelableActionTokenExists, resSyncWithCancelTokenCancelable);
			Assert.AreEqual(2, i);

			cancelSource.Dispose();
		}
	}
}
