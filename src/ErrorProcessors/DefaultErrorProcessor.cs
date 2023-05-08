using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DefaultErrorProcessor : IErrorProcessor
	{
		protected ExceptionDelegatesHelper _exceptionDelegatesHelper;

		public DefaultErrorProcessor() : this(null, null){}

		public DefaultErrorProcessor(Action<Exception, CancellationToken> onBeforeProcessError) : this(onBeforeProcessError, null){}

		public DefaultErrorProcessor(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) : this(null, onBeforeProcessErrorAsync){}

		public DefaultErrorProcessor(Action<Exception, CancellationToken> onBeforeProcessError, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => _exceptionDelegatesHelper = new ExceptionDelegatesHelper(onBeforeProcessError, onBeforeProcessErrorAsync);

		public Exception Process(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			_exceptionDelegatesHelper.Delegate(error, cancellationToken);
			return error;
		}

		public async Task<Exception> ProcessAsync(Exception error, CatchBlockProcessErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			await _exceptionDelegatesHelper.DelegateAsync(error, cancellationToken, configAwait).ConfigureAwait(configAwait);
			return error;
		}
	}
}
