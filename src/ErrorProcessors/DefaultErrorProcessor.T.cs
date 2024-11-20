using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DefaultErrorProcessor<TParam> : IErrorProcessor
	{
		private readonly DefaultErrorProcessorT _errorProcessor;
		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo<TParam>> actionProcessor)
		{
			_errorProcessor = DefaultErrorProcessorT.Create(actionProcessor);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor)
		{
			_errorProcessor = DefaultErrorProcessorT.Create(funcProcessor);
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

	internal class DefaultErrorProcessorT : ErrorProcessorBase<ProcessingErrorInfo>
	{
		public static DefaultErrorProcessorT Create<TParam>(Action<Exception, ProcessingErrorInfo<TParam>> actionProcessor)
		{
			void action(Exception ex, ProcessingErrorInfo pi)
			{
				if (pi is ProcessingErrorInfo<TParam> gpi)
					actionProcessor(ex, gpi);
			}
			var res = new DefaultErrorProcessorT();
			res.SetSyncRunner(action);
			return res;
		}

		public static DefaultErrorProcessorT Create<TParam>(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor)
		{
			Task func(Exception ex, ProcessingErrorInfo pi)
			{
				if (pi is ProcessingErrorInfo<TParam> gpi)
					return funcProcessor(ex, gpi);
				else
					return Task.CompletedTask;
			}
			var res = new DefaultErrorProcessorT();
			res.SetAsyncRunner(func);
			return res;
		}

		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
