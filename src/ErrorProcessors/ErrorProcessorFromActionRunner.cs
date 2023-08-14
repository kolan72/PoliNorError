using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class ErrorProcessorFromActionRunner
	{
		private readonly Action<Exception> _notCancelableAction;
		private readonly Action<Exception, CancellationToken> _cancelableAction;

		private readonly Func<Exception, CancellationToken, Task> _cancelableFunc;

		public ErrorProcessorFromActionRunner(Action<Exception> action)
		{
			_notCancelableAction = action;
		}

		public ErrorProcessorFromActionRunner(Action<Exception> action, CancellationType convertToCancelableFuncType)
		{
			_notCancelableAction = action;
			_cancelableAction = GetCancelableAction(action, convertToCancelableFuncType);
			if (convertToCancelableFuncType == CancellationType.Cancelable)
			{
				_cancelableFunc = (ex, ct) => Task.Run(() => action(ex), ct).WithCancellation(ct);
			}
		}

		public ErrorProcessorFromActionRunner(Action<Exception, CancellationToken> cancelableAction)
		{
			_cancelableAction = cancelableAction;
		}

		private static Action<Exception, CancellationToken> GetCancelableAction(Action<Exception> action, CancellationType convertToCancelableFuncType)
		{
			switch (convertToCancelableFuncType)
			{
				case CancellationType.Precancelable : return action.ToPrecancelableAction();
				case CancellationType.Cancelable: return action.ToCancelableAction();
				default : return action.ToPrecancelableAction();
			}
		}

		public ErrorProcessResulType Process(Exception error, CancellationToken token = default)
		{
			if (token == default)
			{
				return ProcessIfNoToken(error);
			}
			else
			{
				return ProcessByAction(error, token);
			}
		}

		public async Task<ErrorProcessResulType> ProcessAsync(Exception error, CancellationToken token = default)
		{
			if (token == default)
			{
				return ProcessIfNoToken(error);
			}
			else if (CancelableFuncExists)
			{
				await _cancelableFunc(error, token);
				return ErrorProcessResulType.CancelableFunc;
			}
			else
			{
				return ProcessByAction(error, token);
			}
		}

		private ErrorProcessResulType ProcessIfNoToken(Exception error)
		{
			if (NotCancelableActionExists)
			{
				_notCancelableAction(error);
				return ErrorProcessResulType.NotCancelableActionNoToken;
			}
			else
			{
				_cancelableAction(error, default);
				return ErrorProcessResulType.CancelableActionNoToken;
			}
		}

		private ErrorProcessResulType ProcessByAction(Exception error, CancellationToken token)
		{
			if (CancelableActionExists)
			{
				_cancelableAction(error, token);
				return ErrorProcessResulType.CancelableActionTokenExists;
			}
			else
			{
				_notCancelableAction(error);
				return ErrorProcessResulType.NotCancelableActionTokenExists;
			}
		}

		private bool NotCancelableActionExists => _notCancelableAction != null;

		private bool CancelableActionExists => _cancelableAction != null;

		private bool CancelableFuncExists => _cancelableFunc != null;

		internal enum ErrorProcessResulType
		{
			CancelableActionNoToken,
			CancelableActionTokenExists,
			NotCancelableActionNoToken,
			NotCancelableActionTokenExists,
			CancelableFunc
		}
	}
}
