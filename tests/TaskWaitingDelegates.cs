using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal static class TaskWaitingDelegates
	{
		public static Func<int> GetFuncWithTaskWait(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return () => { GetActionWithTaskWait(sourceThatWillBeCanceled, canceledOnLinkedSource)(); return 1; };
		}

		public static Func<int> GetFuncWithTaskWaitAll(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return () => { GetActionWithTaskWaitAll(sourceThatWillBeCanceled, canceledOnLinkedSource)(); return 1; };
		}

		public static Action<int> GetActionWithParamWithTaskWait(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return (_) => GetActionWithTaskWait(sourceThatWillBeCanceled, canceledOnLinkedSource)();
		}

		public static Action<int> GetActionWithParamWithTaskWaitAll(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return (_) => GetActionWithTaskWaitAll(sourceThatWillBeCanceled, canceledOnLinkedSource)();
		}

		public static Action GetActionWithTaskWaitAll(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return () =>
			{
				using (var ctsOther = new CancellationTokenSource())
				{
					var otherTask = Task.Run(async () =>
					{
						await Task.Delay(1);
						ctsOther.Cancel();
						ctsOther.Token.ThrowIfCancellationRequested();
					});

					Task.WaitAll(otherTask,
								GetTaskThatCanBeThrowOnLinkedToken(sourceThatWillBeCanceled,
										   canceledOnLinkedSource));
				}
			};
		}

		public static Action GetActionWithTaskWait(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return () =>
			{
				if (canceledOnLinkedSource)
				{
					using (var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(sourceThatWillBeCanceled.Token))
					{
						var innerToken = cancelTokenSource.Token;
						GetCanceledTask(innerToken, sourceThatWillBeCanceled).Wait();
					}
				}
				else
				{
					GetCanceledTask(sourceThatWillBeCanceled.Token, sourceThatWillBeCanceled).Wait();
				}
			};
		}

		private async static Task GetCanceledTask(CancellationToken tokenThatThrow, CancellationTokenSource sourceThatWillBeCanceled)
		{
			await Task.Delay(1);
			sourceThatWillBeCanceled.Cancel();
			tokenThatThrow.ThrowIfCancellationRequested();
		}

		private async static Task GetTaskThatCanBeThrowOnLinkedToken(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			await Task.Delay(1);
			if (canceledOnLinkedSource)
			{
				using (var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(sourceThatWillBeCanceled.Token))
				{
					var innerToken = cancelTokenSource.Token;
					sourceThatWillBeCanceled.Cancel();
					innerToken.ThrowIfCancellationRequested();
				}
			}
			else
			{
				sourceThatWillBeCanceled.Cancel();
				sourceThatWillBeCanceled.Token.ThrowIfCancellationRequested();
			}
		}
	}
}
