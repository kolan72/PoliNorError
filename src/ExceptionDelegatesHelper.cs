using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class ExceptionDelegatesHelper
	{
		private readonly Action<Exception, CancellationToken> _onBeforeProcessError;
		private readonly Func<Exception, CancellationToken, Task> _onBeforeProcessErrorAsync;

		private readonly bool _noDelegates;

		public ExceptionDelegatesHelper(Action<Exception, CancellationToken> onBeforeProcessError, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync)
		{
			_onBeforeProcessError = onBeforeProcessError;
			_onBeforeProcessErrorAsync = onBeforeProcessErrorAsync;

			_noDelegates = _onBeforeProcessError == null && _onBeforeProcessErrorAsync == null;
			Delegate = GetCallDelegate();
			DelegateAsync = GetCallDelegateNotSync();
		}

		private Func<Exception, CancellationToken, bool, Task> GetCallDelegateNotSync()
		{
			if (_noDelegates)
			{
				return (_, __, ___) => Task.CompletedTask;
			}

			if (_onBeforeProcessErrorAsync != null)
			{
				return async (ex, token, cw) => await _onBeforeProcessErrorAsync(ex, token).ConfigureAwait(cw);
			}
			else
			{
				return async (ex, token, cw) => { _onBeforeProcessError.Invoke(ex, token); await Task.CompletedTask.ConfigureAwait(cw); };
			}
		}

		private Action<Exception, CancellationToken> GetCallDelegate()
		{
			if (_noDelegates)
			{
				return (_, __) => Expression.Empty();
			}
			return _onBeforeProcessError ?? ((ex, token) => Task.Run(() => _onBeforeProcessErrorAsync(ex, token), token).Wait(token));
		}

		public Func<Exception, CancellationToken, bool, Task> DelegateAsync
		{
			get;
		}

		public Action<Exception, CancellationToken> Delegate
		{
			get;
		}
	}
}
