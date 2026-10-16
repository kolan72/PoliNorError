using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class FallbackPolicyBase : IWithInnerErrorProcessor<FallbackPolicyBase>
	{
		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor action.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor action with cancellation token.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor action.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An async inner error processor function.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An async inner error processor function.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An async inner error processor function with cancellation token.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor action with processing error info.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor action with processing error info and cancellation token.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor action with processing error info.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An async inner error processor function with processing error info.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An async inner error processor function with processing error info.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An async inner error processor function with processing error info and cancellation token.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="innerErrorProcessor">An inner error processor.</param>
		/// <returns>Current Fallback policy.</returns>
		public FallbackPolicyBase WithInnerErrorProcessor<TException>(DefaultInnerErrorProcessor<TException> innerErrorProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessor<FallbackPolicyBase, TException>(innerErrorProcessor);
		}
	}
}