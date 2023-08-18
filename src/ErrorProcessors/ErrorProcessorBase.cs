using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract class ErrorProcessorBase<T> : IErrorProcessor
	{
		private  IErrorProcessorRunner<T> _syncRunner;
		private IErrorProcessorRunner<T> _asyncRunner;

		protected void SetSyncRunner(Action<Exception, T> action)
		{
			_syncRunner = new ErrorProcessorFromSyncRunner<T>(action);
		}

		protected void SetSyncRunner(Action<Exception, T> action, CancellationType convertToCancelableFuncType)
		{
			_syncRunner =  new ErrorProcessorFromSyncRunner<T>(action, convertToCancelableFuncType);
		}

		protected void SetSyncRunner(Action<Exception, T, CancellationToken> action)
		{
			_syncRunner = new ErrorProcessorFromSyncRunner<T>(action);
		}

		protected void SetAsyncRunner(Func<Exception, T, Task> funcProcessor)
		{
			_asyncRunner = new ErrorProcessorFromAsyncRunner<T>(funcProcessor);
		}

		protected void SetAsyncRunner(Func<Exception, T, CancellationToken, Task> funcProcessor)
		{
			_asyncRunner = new ErrorProcessorFromAsyncRunner<T>(funcProcessor);
		}

		protected void SetAsyncRunner(Func<Exception, T, Task> funcProcessor, CancellationType convertToCancelableFuncType)
		{
			_asyncRunner = new ErrorProcessorFromAsyncRunner<T>(funcProcessor, convertToCancelableFuncType);
		}

		public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			if (NoRunners)
				return error;

			(_syncRunner ?? _asyncRunner).Run(error, ParameterConverter(catchBlockProcessErrorInfo), cancellationToken);
			return error;
		}

		public async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			if (NoRunners)
				return error;

			await(_asyncRunner ?? _syncRunner).RunAsync(error, ParameterConverter(catchBlockProcessErrorInfo), configAwait, cancellationToken);
			return error;
		}

		protected abstract Func<ProcessingErrorInfo, T> ParameterConverter { get; }

		private bool NoRunners => _syncRunner == null && _asyncRunner == null;
	}
}
