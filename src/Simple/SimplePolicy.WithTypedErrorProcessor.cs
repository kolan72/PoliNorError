using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class SimplePolicy
	{
		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<SimplePolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<SimplePolicy, TException>(actionProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="actionProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<SimplePolicy, TException>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<SimplePolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <param name="cancellationType">A cancellation type.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<SimplePolicy, TException>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="funcProcessor">A delegate for error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<SimplePolicy, TException>(funcProcessor);
		}

		/// <summary>
		/// Adds an error processor for handling exception only if it has the <typeparamref name="TException"/> type.
		/// </summary>
		/// <typeparam name="TException">A type of the handled exception.</typeparam>
		/// <param name="errorProcessor">A default typed error processor.</param>
		/// <returns>A Simple policy.</returns>
		public SimplePolicy WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessor<SimplePolicy, TException>(errorProcessor);
		}
	}
}
