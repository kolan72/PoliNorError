using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class ErrorProcessorFromSyncRunner<T> : IErrorProcessorRunner<T>
	{
		private readonly Action<Exception, T> _notCancelableAction;
		private readonly Action<Exception, T, CancellationToken> _cancelableAction;

		private readonly Func<Exception, T, CancellationToken, Task> _cancelableFunc;

		public ErrorProcessorFromSyncRunner(Action<Exception, T> action)
		{
			_notCancelableAction = action;
		}

		public ErrorProcessorFromSyncRunner(Action<Exception, T> action, CancellationType convertToCancelableFuncType)
		{
			_notCancelableAction = action;
			_cancelableAction = GetCancelableAction(action, convertToCancelableFuncType);
			if (convertToCancelableFuncType == CancellationType.Cancelable)
			{
				_cancelableFunc = (ex, t, ct) => Task.Run(() => action(ex, t), ct).WithCancellation(ct);
			}
		}

		public ErrorProcessorFromSyncRunner(Action<Exception, T, CancellationToken> cancelableAction)
		{
			_cancelableAction = cancelableAction;
		}

		private static Action<Exception, T, CancellationToken> GetCancelableAction(Action<Exception, T> action, CancellationType convertToCancelableFuncType)
		{
			switch (convertToCancelableFuncType)
			{
				case CancellationType.Precancelable : return action.ToPrecancelableAction();
				case CancellationType.Cancelable: return action.ToCancelableAction();
				default : return action.ToPrecancelableAction();
			}
		}

		public ErrorProcessorRunResult Run(Exception error, T t, CancellationToken token = default)
		{
			if (token == default)
			{
				return RunIfNoToken(error, t);
			}
			else
			{
				return RunSyncIfTokenExists(error, t, token);
			}
		}

		public async Task<ErrorProcessorRunResult> RunAsync(Exception error, T t, bool configAwait = false, CancellationToken token = default)
		{
			if (token == default)
			{
				return RunIfNoToken(error, t);
			}
			else if (CancelableFuncExists)
			{
				await _cancelableFunc(error, t, token).ConfigureAwait(configAwait);
				return ErrorProcessorRunResult.CancelableFuncTokenExists;
			}
			else
			{
				return RunSyncIfTokenExists(error, t, token);
			}
		}

		private ErrorProcessorRunResult RunIfNoToken(Exception error, T t)
		{
			if (NotCancelableActionExists)
			{
				_notCancelableAction(error, t);
				return ErrorProcessorRunResult.NotCancelableActionNoToken;
			}
			else
			{
				_cancelableAction(error, t, default);
				return ErrorProcessorRunResult.CancelableActionNoToken;
			}
		}

		private ErrorProcessorRunResult RunSyncIfTokenExists(Exception error, T t, CancellationToken token)
		{
			if (CancelableActionExists)
			{
				_cancelableAction(error, t, token);
				return ErrorProcessorRunResult.CancelableActionTokenExists;
			}
			else
			{
				_notCancelableAction(error, t);
				return ErrorProcessorRunResult.NotCancelableActionTokenExists;
			}
		}

		private bool NotCancelableActionExists => _notCancelableAction != null;

		private bool CancelableActionExists => _cancelableAction != null;

		private bool CancelableFuncExists => _cancelableFunc != null;
	}
}
