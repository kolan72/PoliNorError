using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class DefaultErrorProcessorV2 : IErrorProcessor
	{
		private readonly IErrorProcessorRunner<Unit> _syncRunner;
		private readonly IErrorProcessorRunner<Unit> _asyncRunner;

		private readonly bool _noRunners = false;

		internal DefaultErrorProcessorV2()
		{
			_noRunners = true;
		}

		public DefaultErrorProcessorV2(Action<Exception> actionProcessor)
		{
			_syncRunner = GetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessorV2(Action<Exception, CancellationToken> actionProcessor)
		{
			void processor(Exception exc, Unit _, CancellationToken ct) => actionProcessor(exc, ct);
			_syncRunner = new ErrorProcessorFromSyncRunner<Unit>(processor);
		}

		public DefaultErrorProcessorV2(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			_syncRunner = GetSyncRunner(actionProcessor, actionCancellationType);
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor);
		}

		public DefaultErrorProcessorV2(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor);
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor, CancellationType  funcCancellationType)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor, funcCancellationType);
		}

		public DefaultErrorProcessorV2(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor);
			_syncRunner = GetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessorV2(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor);
			_syncRunner = GetSyncRunner(actionProcessor, actionCancellationType);
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor);
			_syncRunner = GetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType funcAndActionCancellationType)
		{
			_asyncRunner = GetAsyncRunner(funcProcessor, funcAndActionCancellationType);
			_syncRunner = GetSyncRunner(actionProcessor, funcAndActionCancellationType);
		}

		public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			if (_noRunners)
				return error;

			(_syncRunner ?? _asyncRunner).Run(error, Unit.Default, cancellationToken);
			return error;
		}

		public async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			if (_noRunners)
				return error;

			await (_asyncRunner ?? _syncRunner).RunAsync(error, Unit.Default, configAwait, cancellationToken);
			return error;
		}

		private IErrorProcessorRunner<Unit> GetSyncRunner(Action<Exception> actionProcessor)
		{
			void processor(Exception exc, Unit _) => actionProcessor(exc);
			return new ErrorProcessorFromSyncRunner<Unit>(processor);
		}

		private IErrorProcessorRunner<Unit> GetSyncRunner(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			void processor(Exception exc, Unit _) => actionProcessor(exc);
			return new ErrorProcessorFromSyncRunner<Unit>(processor, actionCancellationType);
		}

		private IErrorProcessorRunner<Unit> GetAsyncRunner(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			Task processorAsync(Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct);
			return new ErrorProcessorFromAsyncRunner<Unit>(processorAsync);
		}

		private IErrorProcessorRunner<Unit> GetAsyncRunner(Func<Exception, Task> funcProcessor, CancellationType funcCancellationType)
		{
			Task processorAsync(Exception exc, Unit _) => funcProcessor(exc);
			return new ErrorProcessorFromAsyncRunner<Unit>(processorAsync, funcCancellationType);
		}

		private IErrorProcessorRunner<Unit> GetAsyncRunner(Func<Exception, Task> funcProcessor)
		{
			Task processorAsync(Exception exc, Unit _) => funcProcessor(exc);
			return new ErrorProcessorFromAsyncRunner<Unit>(processorAsync);
		}
	}
}
;