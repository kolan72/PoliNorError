using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class BasicErrorProcessor : ErrorProcessorBase<Unit>
	{
		internal BasicErrorProcessor(){}

		public BasicErrorProcessor(Action<Exception> actionProcessor)
		{
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		public BasicErrorProcessor(Action<Exception, CancellationToken> actionProcessor)
		{
			SetSyncRunner((Exception exc, Unit _, CancellationToken ct) => actionProcessor(exc, ct));
		}

		public BasicErrorProcessor(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), actionCancellationType);
		}

		public BasicErrorProcessor(Func<Exception, Task> funcProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc));
		}

		public BasicErrorProcessor(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
		}

		public BasicErrorProcessor(Func<Exception, Task> funcProcessor, CancellationType funcCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc), funcCancellationType );
		}

		public BasicErrorProcessor(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		public BasicErrorProcessor(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), actionCancellationType);
		}

		public BasicErrorProcessor(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		public BasicErrorProcessor(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType funcAndActionCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc), funcAndActionCancellationType);
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), funcAndActionCancellationType);
		}

		protected override Func<ProcessingErrorInfo, Unit> ParameterConverter => (_) => Unit.Default;
	}
}
;