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

		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
