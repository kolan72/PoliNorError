using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicy
	{
		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="actionProcessor">The action delegate that processes the exception and error info.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(actionProcessor);
		}

		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="actionProcessor">The action delegate that processes the exception, error info, and cancellation token.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(actionProcessor);
		}

		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="actionProcessor">The action delegate that processes the exception and error info.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(actionProcessor, cancellationType);
		}

		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="funcProcessor">The function that asynchronously processes the exception and error info.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(funcProcessor);
		}

		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="funcProcessor">The function that asynchronously processes the exception and error info.</param>
		/// <param name="cancellationType">The cancellation type.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(funcProcessor, cancellationType);
		}

		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="funcProcessor">The function that asynchronously processes the exception, error info, and cancellation token.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(funcProcessor);
		}

		/// <summary>Adds a typed error processor to the fallback policy.</summary>
		/// <typeparam name="TException">The type of the exception to process.</typeparam>
		/// <param name="errorProcessor">The default typed error processor instance.</param>
		/// <returns>The fallback policy with the added error processor.</returns>
		public new FallbackPolicy WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessor<FallbackPolicy, TException>(errorProcessor);
		}
	}
}