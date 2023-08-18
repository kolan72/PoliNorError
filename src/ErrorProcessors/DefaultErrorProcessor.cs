using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DefaultErrorProcessor : ErrorProcessorBase<ProcessingErrorInfo>
	{
		internal DefaultErrorProcessor()
		{
		}

		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			SetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			SetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType actionCancellationType)
		{
			SetSyncRunner(actionProcessor, actionCancellationType);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			SetAsyncRunner(funcProcessor);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			SetAsyncRunner(funcProcessor);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType funcCancellationType)
		{
			SetAsyncRunner(funcProcessor, funcCancellationType);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			SetAsyncRunner(funcProcessor);
			SetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType actionCancellationType)
		{
			SetAsyncRunner(funcProcessor);
			SetSyncRunner(actionProcessor, actionCancellationType);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			SetAsyncRunner(funcProcessor);
			SetSyncRunner(actionProcessor);
		}

		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType funcAndActionCancellationType)
		{
			SetAsyncRunner(funcProcessor, funcAndActionCancellationType);
			SetSyncRunner(actionProcessor, funcAndActionCancellationType);
		}

		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
