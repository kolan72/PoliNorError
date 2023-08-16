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

		public ErrorProcessorRunResul Run(Exception error, T t, CancellationToken token = default)
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

		public async Task<ErrorProcessorRunResul> RunAsync(Exception error, T t, bool configAwait = false, CancellationToken token = default)
		{
			if (token == default)
			{
				return RunIfNoToken(error, t);
			}
			else if (CancelableFuncExists)
			{
				await _cancelableFunc(error, t, token).ConfigureAwait(configAwait);
				return ErrorProcessorRunResul.CancelableFuncTokenExists;
			}
			else
			{
				return RunSyncIfTokenExists(error, t, token);
			}
		}

		private ErrorProcessorRunResul RunIfNoToken(Exception error, T t)
		{
			if (NotCancelableActionExists)
			{
				_notCancelableAction(error, t);
				return ErrorProcessorRunResul.NotCancelableActionNoToken;
			}
			else
			{
				_cancelableAction(error, t, default);
				return ErrorProcessorRunResul.CancelableActionNoToken;
			}
		}

		private ErrorProcessorRunResul RunSyncIfTokenExists(Exception error, T t, CancellationToken token)
		{
			if (CancelableActionExists)
			{
				_cancelableAction(error, t, token);
				return ErrorProcessorRunResul.CancelableActionTokenExists;
			}
			else
			{
				_notCancelableAction(error, t);
				return ErrorProcessorRunResul.NotCancelableActionTokenExists;
			}
		}

		private bool NotCancelableActionExists => _notCancelableAction != null;

		private bool CancelableActionExists => _cancelableAction != null;

		private bool CancelableFuncExists => _cancelableFunc != null;
	}
}
