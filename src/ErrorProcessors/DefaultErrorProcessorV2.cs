using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class DefaultErrorProcessorV2 : ErrorProcessorBase<Unit>
	{
		internal DefaultErrorProcessorV2(){}

		public DefaultErrorProcessorV2(Action<Exception> actionProcessor)
		{
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		public DefaultErrorProcessorV2(Action<Exception, CancellationToken> actionProcessor)
		{
			SetSyncRunner((Exception exc, Unit _, CancellationToken ct) => actionProcessor(exc, ct));
		}

		public DefaultErrorProcessorV2(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), actionCancellationType);
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc));
		}

		public DefaultErrorProcessorV2(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor, CancellationType funcCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc), funcCancellationType );
		}

		public DefaultErrorProcessorV2(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		public DefaultErrorProcessorV2(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), actionCancellationType);
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		public DefaultErrorProcessorV2(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType funcAndActionCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc), funcAndActionCancellationType);
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), funcAndActionCancellationType);
		}

		protected override Func<ProcessingErrorInfo, Unit> ParameterConverter => (_) => Unit.Default;
	}
}
;