using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Default implementation of an error processor that handles exceptions without type parameters.
	/// </summary>
	public class DefaultErrorProcessor : ErrorProcessorBase<ProcessingErrorInfo>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with no processors.
		/// </summary>
		internal DefaultErrorProcessor()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with a synchronous error processor.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			SetSyncRunner(actionProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with a cancellation token-aware synchronous error processor.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			SetSyncRunner(actionProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with a synchronous error processor and cancellation behavior.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		/// <param name="actionCancellationType">The cancellation type for the action processor.</param>
		public DefaultErrorProcessor(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType actionCancellationType)
		{
			SetSyncRunner(actionProcessor, actionCancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with an asynchronous error processor.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			SetAsyncRunner(funcProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with a cancellation token-aware asynchronous error processor.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			SetAsyncRunner(funcProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with an asynchronous error processor and cancellation behavior.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="funcCancellationType">The cancellation type for the function processor.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType funcCancellationType)
		{
			SetAsyncRunner(funcProcessor, funcCancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with both synchronous and asynchronous error processors.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			SetAsyncRunner(funcProcessor);
			SetSyncRunner(actionProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with both synchronous and asynchronous error processors and cancellation behavior.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		/// <param name="actionCancellationType">The cancellation type for the action processor.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType actionCancellationType)
		{
			SetAsyncRunner(funcProcessor);
			SetSyncRunner(actionProcessor, actionCancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with both synchronous and asynchronous error processors.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			SetAsyncRunner(funcProcessor);
			SetSyncRunner(actionProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultErrorProcessor"/> class with both synchronous and asynchronous error processors sharing the same cancellation behavior.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="actionProcessor">The action to execute when processing an error.</param>
		/// <param name="funcAndActionCancellationType">The cancellation type for both the function and action processors.</param>
		public DefaultErrorProcessor(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType funcAndActionCancellationType)
		{
			SetAsyncRunner(funcProcessor, funcAndActionCancellationType);
			SetSyncRunner(actionProcessor, funcAndActionCancellationType);
		}

		/// <summary>
		/// Gets the parameter converter that transforms <see cref="ProcessingErrorInfo"/> to itself.
		/// </summary>
		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
