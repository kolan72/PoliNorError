using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.ErrorProcessorFromActionRunner;

namespace PoliNorError.Tests
{
	public class ErrorProcessorFromActionRunnerTests
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Ctor_With_Not_Cancelable_Action_Always_Processed_As_NotCancelable(bool withToken)
		{
			int i = 0;
			void act(Exception _) => ++i;

			var notCancelableRunner = new ErrorProcessorFromActionRunner(act);
			var ex = new Exception();

			if (!withToken)
			{
				var resAsyncWithNoCancelToken = await notCancelableRunner.ProcessAsync(ex);
				Assert.AreEqual(ErrorProcessResulType.NotCancelableActionNoToken, resAsyncWithNoCancelToken);
				Assert.AreEqual(1, i);

				var resSyncWithNoCancelToken = notCancelableRunner.Process(ex);
				Assert.AreEqual(ErrorProcessResulType.NotCancelableActionNoToken, resSyncWithNoCancelToken);
				Assert.AreEqual(2, i);
			}
			else
			{
				var cancelSource = new CancellationTokenSource();

				var resSyncWithCancelToken = notCancelableRunner.Process(ex, cancelSource.Token);
				Assert.AreEqual(ErrorProcessResulType.NotCancelableActionTokenExists, resSyncWithCancelToken);
				Assert.AreEqual(1, i);

				var resAsyncWithCancelToken = await notCancelableRunner.ProcessAsync(ex, cancelSource.Token);
				Assert.AreEqual(ErrorProcessResulType.NotCancelableActionTokenExists, resAsyncWithCancelToken);
				Assert.AreEqual(2, i);

				cancelSource.Dispose();
			}
		}

		[Test]
		public async Task Should_Ctor_With_Cancelable_Action_Always_ProcessedAsync_As_CancelableFunc()
		{
			int i = 0;
			void act(Exception _) => ++i;
			var ex = new Exception();
			var cancelSource = new CancellationTokenSource();

			var cancelableRunner = new ErrorProcessorFromActionRunner(act, CancellationType.Cancelable);
			var resAsyncWithCancelTokenCancelable = await cancelableRunner.ProcessAsync(ex, cancelSource.Token);
			Assert.AreEqual(ErrorProcessResulType.CancelableFunc, resAsyncWithCancelTokenCancelable);
			Assert.AreEqual(1, i);

			cancelSource.Dispose();
		}
	}
}
