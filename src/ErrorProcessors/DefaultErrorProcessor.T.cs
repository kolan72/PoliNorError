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

		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo<TParam>, CancellationToken> actionProcessor)
		{
			_errorProcessor = DefaultErrorProcessorT.Create(actionProcessor);
		}

		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo<TParam>> actionProcessor, CancellationType cancellationType)
		{
			_errorProcessor = DefaultErrorProcessorT.Create(actionProcessor, cancellationType);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor)
		{
			_errorProcessor = DefaultErrorProcessorT.Create(funcProcessor);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor, CancellationType cancellationType)
		{
			_errorProcessor = DefaultErrorProcessorT.Create(funcProcessor, cancellationType);
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
			var action = ConvertToNonGenericAction(actionProcessor);
			var res = new DefaultErrorProcessorT();
			res.SetSyncRunner(action);
			return res;
		}

		public static DefaultErrorProcessorT Create<TParam>(Action<Exception, ProcessingErrorInfo<TParam>, CancellationToken> actionProcessor)
		{
			void action(Exception ex, ProcessingErrorInfo pi, CancellationToken token)
			{
				if (pi is ProcessingErrorInfo<TParam> gpi)
					actionProcessor(ex, gpi, token);
			}
			var res = new DefaultErrorProcessorT();
			res.SetSyncRunner(action);
			return res;
		}

		public static DefaultErrorProcessorT Create<TParam>(Action<Exception, ProcessingErrorInfo<TParam>> actionProcessor, CancellationType cancellationType)
		{
			var action = ConvertToNonGenericAction(actionProcessor);
			var res = new DefaultErrorProcessorT();
			res.SetSyncRunner(action, cancellationType);
			return res;
		}

		public static DefaultErrorProcessorT Create<TParam>(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor)
		{
			var func = ConvertToNonGenericFunc(funcProcessor);
			var res = new DefaultErrorProcessorT();
			res.SetAsyncRunner(func);
			return res;
		}

		public static DefaultErrorProcessorT Create<TParam>(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor, CancellationType cancellationType)
		{
			var func = ConvertToNonGenericFunc(funcProcessor);
			var res = new DefaultErrorProcessorT();
			res.SetAsyncRunner(func, cancellationType);
			return res;
		}

		private static Action<Exception, ProcessingErrorInfo> ConvertToNonGenericAction<TParam>(Action<Exception, ProcessingErrorInfo<TParam>> actionProcessor)
		{
			return (Exception ex, ProcessingErrorInfo pi) =>
			{
				if (pi is ProcessingErrorInfo<TParam> gpi)
					actionProcessor(ex, gpi);
			};
		}

		private static Func<Exception, ProcessingErrorInfo, Task> ConvertToNonGenericFunc<TParam>(Func<Exception, ProcessingErrorInfo<TParam>, Task> funcProcessor)
		{
			return (Exception ex, ProcessingErrorInfo pi) =>
			{
				if (pi is ProcessingErrorInfo<TParam> gpi)
					return funcProcessor(ex, gpi);
				else
					return Task.CompletedTask;
			};
		}

		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
