using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DefaultTypedErrorProcessor<TException> : IErrorProcessor where TException : Exception
	{
		private readonly DefaultTypedErrorProcessorT<TException> _errorProcessor;

		public DefaultTypedErrorProcessor(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_errorProcessor = DefaultTypedErrorProcessorT<TException>.Create(actionProcessor);
		}

		public DefaultTypedErrorProcessor(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_errorProcessor = DefaultTypedErrorProcessorT<TException>.Create(actionProcessor, cancellationType);
		}

		public DefaultTypedErrorProcessor(Func<TException, ProcessingErrorInfo, Task> funcProcessor)
		{
			_errorProcessor = DefaultTypedErrorProcessorT<TException>.Create(funcProcessor);
		}

		public DefaultTypedErrorProcessor(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_errorProcessor = DefaultTypedErrorProcessorT<TException>.Create(funcProcessor, cancellationType);
		}

		public DefaultTypedErrorProcessor(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_errorProcessor = DefaultTypedErrorProcessorT<TException>.Create(funcProcessor);
		}

		public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			return _errorProcessor.Process(error, catchBlockProcessErrorInfo, cancellationToken);
		}

		public async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			return await _errorProcessor.ProcessAsync(error, catchBlockProcessErrorInfo, configAwait, cancellationToken).ConfigureAwait(configAwait);
		}
	}

	internal class DefaultTypedErrorProcessorT<TException> : ErrorProcessorBase<ProcessingErrorInfo> where TException : Exception
	{
		public static DefaultTypedErrorProcessorT<TException> Create(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			var action = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.TryAsExact);

			var res = new DefaultTypedErrorProcessorT<TException>();
			res.SetSyncRunner(action);
			return res;
		}

		public static DefaultTypedErrorProcessorT<TException> Create(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			var action = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.TryAsExact);

			var res = new DefaultTypedErrorProcessorT<TException>();
			res.SetSyncRunner(action, cancellationType);
			return res;
		}

		public static DefaultTypedErrorProcessorT<TException> Create(Func<TException, ProcessingErrorInfo, Task> funcProcessor)
		{
			var func = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.TryAsExact);

			var res = new DefaultTypedErrorProcessorT<TException>();
			res.SetAsyncRunner(func);
			return res;
		}

		public static DefaultTypedErrorProcessorT<TException> Create(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			var func = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.TryAsExact);

			var res = new DefaultTypedErrorProcessorT<TException>();
			res.SetAsyncRunner(func, cancellationType);
			return res;
		}

		public static DefaultTypedErrorProcessorT<TException> Create(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			var func = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.TryAsExact);

			var res = new DefaultTypedErrorProcessorT<TException>();
			res.SetAsyncRunner(func);
			return res;
		}

		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
