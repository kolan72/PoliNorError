using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class RetryPolicy : IWithInnerErrorProcessor<RetryPolicy>
	{
		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An inner error processor.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">An inner error processor.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An inner error processor.</param>
		/// <param name="cancellationType">Cancellation type for the error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<RetryPolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="innerErrorProcessor">An inner error processor.</param>
		/// <returns>Current Retry policy.</returns>
		public RetryPolicy WithInnerErrorProcessor<TException>(DefaultInnerErrorProcessor<TException> innerErrorProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessor<RetryPolicy, TException>(innerErrorProcessor);
		}
	}
}
