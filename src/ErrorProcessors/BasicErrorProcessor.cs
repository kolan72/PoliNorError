using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// A basic error processor that executes an action or async function when an exception occurs.
	/// It always consumes <see cref="Unit"/> as the processing parameter.
	/// </summary>
	public class BasicErrorProcessor : ErrorProcessorBase<Unit>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class.
		/// </summary>
		internal BasicErrorProcessor(){}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with a synchronous error-processing action.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		public BasicErrorProcessor(Action<Exception> actionProcessor)
		{
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with a cancellation token-aware synchronous error-processing action.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		public BasicErrorProcessor(Action<Exception, CancellationToken> actionProcessor)
		{
			SetSyncRunner((Exception exc, Unit _, CancellationToken ct) => actionProcessor(exc, ct));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with a synchronous error-processing action and the cancellation behavior.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		/// <param name="actionCancellationType">The cancellation behavior for the action.</param>
		public BasicErrorProcessor(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), actionCancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with an asynchronous error-processing function.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		public BasicErrorProcessor(Func<Exception, Task> funcProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with a cancellation token-aware asynchronous error-processing function.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		public BasicErrorProcessor(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with an asynchronous error-processing function and the cancellation behavior.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="funcCancellationType">The cancellation behavior for the async function.</param>
		public BasicErrorProcessor(Func<Exception, Task> funcProcessor, CancellationType funcCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc), funcCancellationType );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with both asynchronous and synchronous error-processing handlers.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error synchronously.</param>
		public BasicErrorProcessor(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with both asynchronous and synchronous error-processing handlers and the action's cancellation behavior.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error synchronously.</param>
		/// <param name="actionCancellationType">The cancellation behavior for the synchronous action.</param>
		public BasicErrorProcessor(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _, CancellationToken ct) => funcProcessor(exc, ct));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), actionCancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with both asynchronous and synchronous error-processing handlers, without cancellation.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error synchronously.</param>
		public BasicErrorProcessor(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc));
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicErrorProcessor"/> class
		/// with both asynchronous and synchronous error-processing handlers and shared cancellation behavior.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error synchronously.</param>
		/// <param name="funcAndActionCancellationType">The shared cancellation behavior for both handlers.</param>
		public BasicErrorProcessor(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType funcAndActionCancellationType)
		{
			SetAsyncRunner((Exception exc, Unit _) => funcProcessor(exc), funcAndActionCancellationType);
			SetSyncRunner((Exception exc, Unit _) => actionProcessor(exc), funcAndActionCancellationType);
		}

		/// <summary>
		/// Gets the parameter converter that transforms <see cref="ProcessingErrorInfo"/> into <see cref="Unit.Default"/>.
		/// </summary>
		protected override Func<ProcessingErrorInfo, Unit> ParameterConverter => (_) => Unit.Default;
	}
}
;