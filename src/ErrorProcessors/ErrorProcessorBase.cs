using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Base class for typed error processors.
	/// </summary>
	/// <typeparam name="T">The type of the parameter used for error processing.</typeparam>
	public abstract class ErrorProcessorBase<T> : IErrorProcessor
	{
		private  IErrorProcessorRunner<T> _syncRunner;
		private IErrorProcessorRunner<T> _asyncRunner;

		/// <summary>
		/// Sets the synchronous error processor runner with a basic action.
		/// </summary>
		/// <param name="action">The action to execute when processing an error.</param>
		protected void SetSyncRunner(Action<Exception, T> action)
		{
			_syncRunner = new ErrorProcessorFromSyncRunner<T>(action);
		}

		/// <summary>
		/// Sets the synchronous error processor runner with a basic action and cancellation type.
		/// </summary>
		/// <param name="action">The action to execute when processing an error.</param>
		/// <param name="convertToCancelableFuncType">The cancellation type for the operation.</param>
		protected void SetSyncRunner(Action<Exception, T> action, CancellationType convertToCancelableFuncType)
		{
			_syncRunner =  new ErrorProcessorFromSyncRunner<T>(action, convertToCancelableFuncType);
		}

		/// <summary>
		/// Sets the synchronous error processor runner with a cancellation token-aware action.
		/// </summary>
		/// <param name="action">The action to execute when processing an error.</param>
		protected void SetSyncRunner(Action<Exception, T, CancellationToken> action)
		{
			_syncRunner = new ErrorProcessorFromSyncRunner<T>(action);
		}

		/// <summary>
		/// Sets the asynchronous error processor runner with a basic async function.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		protected void SetAsyncRunner(Func<Exception, T, Task> funcProcessor)
		{
			_asyncRunner = new ErrorProcessorFromAsyncRunner<T>(funcProcessor);
		}

		/// <summary>
		/// Sets the asynchronous error processor runner with a cancellation token-aware async function.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		protected void SetAsyncRunner(Func<Exception, T, CancellationToken, Task> funcProcessor)
		{
			_asyncRunner = new ErrorProcessorFromAsyncRunner<T>(funcProcessor);
		}

		/// <summary>
		/// Sets the asynchronous error processor runner with a basic async function and cancellation type.
		/// </summary>
		/// <param name="funcProcessor">The async function to execute when processing an error.</param>
		/// <param name="convertToCancelableFuncType">The cancellation type for the operation.</param>
		protected void SetAsyncRunner(Func<Exception, T, Task> funcProcessor, CancellationType convertToCancelableFuncType)
		{
			_asyncRunner = new ErrorProcessorFromAsyncRunner<T>(funcProcessor, convertToCancelableFuncType);
		}

		/// <summary>
		/// Processes the error synchronously.
		/// </summary>
		/// <param name="error">The error to process.</param>
		/// <param name="catchBlockProcessErrorInfo">Information about the error that triggered processing.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The processed exception.</returns>
		public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			if (NoRunners)
				return error;

			(_syncRunner ?? _asyncRunner).Run(error, ParameterConverter(catchBlockProcessErrorInfo), cancellationToken);
			return error;
		}

		/// <summary>
		/// Processes the error asynchronously.
		/// </summary>
		/// <param name="error">The error to process.</param>
		/// <param name="catchBlockProcessErrorInfo">Information about the error that triggered processing.</param>
		/// <param name="configAwait">How to await the operation.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The processed exception.</returns>
		public async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			if (NoRunners)
				return error;

			await(_asyncRunner ?? _syncRunner).RunAsync(error, ParameterConverter(catchBlockProcessErrorInfo), configAwait, cancellationToken).ConfigureAwait(configAwait);
			return error;
		}

		/// <summary>
		/// Gets the parameter converter that transforms <see cref="ProcessingErrorInfo"/> to the type parameter <typeparamref name="T"/>.
		/// </summary>
		protected abstract Func<ProcessingErrorInfo, T> ParameterConverter { get; }

		private bool NoRunners => _syncRunner == null && _asyncRunner == null;
	}
}
