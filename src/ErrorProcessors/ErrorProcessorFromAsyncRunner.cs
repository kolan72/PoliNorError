﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.ErrorProcessors
{
	internal class ErrorProcessorFromAsyncRunner<T>
	{
		private readonly Func<Exception, T, Task> _notCancelableFunc;
		private readonly Func<Exception, T, CancellationToken, Task> _cancelableFunc;

		private readonly Action<Exception, T, CancellationToken> _cancelableAction;
		private readonly Action<Exception, T> _notCancelableAction;

		public ErrorProcessorFromAsyncRunner(Func<Exception, T, Task> notCancelableFunc)
		{
			_notCancelableFunc = notCancelableFunc;
			_notCancelableAction = (ex, t) => notCancelableFunc(ex, t).Wait();
		}

		public ErrorProcessorFromAsyncRunner(Func<Exception, T, Task> notCancelableFunc, CancellationType convertToCancelableFuncType)
		{
			_notCancelableFunc = notCancelableFunc;
			_cancelableFunc = GetCancelableFunc(notCancelableFunc, convertToCancelableFuncType);
			_cancelableAction = GetCancelableAction(notCancelableFunc, convertToCancelableFuncType);
		}

		public ErrorProcessorFromAsyncRunner(Func<Exception, T, CancellationToken, Task> cancelableFunc)
		{
			_cancelableFunc = cancelableFunc;
			_cancelableAction = (ex, t, tok) => cancelableFunc(ex, t, tok).Wait(tok);
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
				if (NotCancelableFuncExists)
				{
					await _notCancelableFunc(error, t).ConfigureAwait(configAwait);
					return ErrorProcessorRunResul.NotCancelableFuncNoToken;
				}
				else
				{
					await _cancelableFunc(error, t, default).ConfigureAwait(configAwait);
					return ErrorProcessorRunResul.CancelableFuncNoToken;
				}
			}
			else
			{
				if (CancelableFuncExists)
				{
					await _cancelableFunc(error, t, token).ConfigureAwait(configAwait);
					return ErrorProcessorRunResul.CancelableFuncTokenExists;
				}
				else
				{
					await _notCancelableFunc(error, t).ConfigureAwait(configAwait);
					return ErrorProcessorRunResul.NotCancelableFuncTokenExists;
				}
			}
		}

		private static Func<Exception, T, CancellationToken, Task> GetCancelableFunc(Func<Exception, T, Task> func, CancellationType convertToCancelableFuncType)
		{
			switch (convertToCancelableFuncType)
			{
				case CancellationType.Precancelable: return func.ToPrecancelableFunc();
				case CancellationType.Cancelable: return func.ToCancelableFunc();
				default: return func.ToPrecancelableFunc();
			}
		}

		private static Action<Exception, T, CancellationToken> GetCancelableAction(Func<Exception, T, Task> func, CancellationType convertToCancelableFuncType)
		{
			switch (convertToCancelableFuncType)
			{
				case CancellationType.Precancelable: return func.ToPrecancelableAction();
				case CancellationType.Cancelable: return func.ToCancelableAction();
				default: return func.ToPrecancelableAction();
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

		private bool NotCancelableFuncExists => _notCancelableFunc != null;

		private bool CancelableFuncExists => _cancelableFunc != null;

		private bool NotCancelableActionExists => _notCancelableAction != null;

		private bool CancelableActionExists => _cancelableAction != null;
	}
}