using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal static class TaskWaitingDelegates
	{
		public static Action GetFuncWithTaskWait(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
		{
			return () =>
			{
				if (canceledOnLinkedSource)
				{
					using (var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(sourceThatWillBeCanceled.Token))
					{
						var innerToken = cancelTokenSource.Token;
						GetTask(innerToken).Wait();
					}
				}
				else
				{
					GetTask(sourceThatWillBeCanceled.Token).Wait();
				}
				async Task GetTask(CancellationToken token)
				{
					await Task.Delay(1);
					sourceThatWillBeCanceled.Cancel();
					token.ThrowIfCancellationRequested();
				}
			};
		}

		public static Action GetFuncWithTaskWaitAll(CancellationTokenSource sourceThatWillBeCanceled, bool canceledOnLinkedSource)
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

					async Task GetTask()
					{
						await Task.Delay(1);
						if (canceledOnLinkedSource)
						{
							using (var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ctsOther.Token))
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
					Task.WaitAll(otherTask, GetTask());
				}
			};
		}
	}
}
