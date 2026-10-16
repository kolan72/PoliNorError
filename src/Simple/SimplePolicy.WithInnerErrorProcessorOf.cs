using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class SimplePolicy : IWithInnerErrorProcessor<SimplePolicy>
	{
		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<SimplePolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling inner exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of inner exception.</typeparam>
		/// <param name="innerErrorProcessor">An inner error processor.</param>
		/// <returns>Current Simple policy.</returns>
		public SimplePolicy WithInnerErrorProcessor<TException>(DefaultInnerErrorProcessor<TException> innerErrorProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessor<SimplePolicy, TException>(innerErrorProcessor);
		}
	}
}
