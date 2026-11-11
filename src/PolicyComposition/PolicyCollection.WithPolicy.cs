using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class PolicyCollection
	{
		/// <summary>
		/// Adds a retry policy with fixed retry count to the collection.
		/// </summary>
		/// <param name="retryCount">Number of times to retry the operation.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithRetry(int retryCount, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds a retry policy with fixed retry count and configurable delay strategy to the collection.
		/// </summary>
		/// <param name="retryCount">Number of times to retry the operation.</param>
		/// <param name="retryDelay">Strategy for calculating delay between retries.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithRetry(int retryCount, RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, retryDelay, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds a wait and retry policy with fixed retry count and constant delay interval to the collection.
		/// </summary>
		/// <param name="retryCount">Number of times to retry the operation.</param>
		/// <param name="delay">Fixed time span to wait between each retry attempt.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithWaitAndRetry(int retryCount, TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds a wait and retry policy with fixed retry count and dynamic delay calculated by a delegate function.
		/// </summary>
		/// <param name="retryCount">Number of times to retry the operation.</param>
		/// <param name="delayOnRetryFunc">Delegate to calculate delay based on retry attempt number and exception.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithWaitAndRetry(int retryCount, Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryCount, delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds an infinite retry policy with configurable error handling to the collection.
		/// </summary>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithInfiniteRetry(ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds an infinite retry policy with configurable error handling and delay strategy to the collection.
		/// </summary>
		/// <param name="retryDelay">Strategy for calculating delay between retries.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithInfiniteRetry(RetryDelay retryDelay, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(retryDelay, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds an infinite retry policy with wait and configurable error handling using a constant delay interval to the collection.
		/// </summary>
		/// <param name="delay">Fixed time span to wait between each retry attempt.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithWaitAndInfiniteRetry(TimeSpan delay, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(delay, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds an infinite retry policy with wait and configurable error handling using a dynamic delay function to the collection.
		/// </summary>
		/// <param name="delayOnRetryFunc">Delegate to calculate delay based on retry attempt number and exception.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="failedIfSaveErrorThrows">Whether to fail if saving error throws an exception.</param>
		/// <param name="errorSaver">Optional delegate for saving errors during retry processing.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithWaitAndInfiniteRetry(Func<int, Exception, TimeSpan> delayOnRetryFunc, ErrorProcessorParam policyParams = null, bool failedIfSaveErrorThrows = false, RetryErrorSaverParam errorSaver = null)
		{
			return this.WithRetryInner(delayOnRetryFunc, policyParams, failedIfSaveErrorThrows, errorSaver);
		}

		/// <summary>
		/// Adds a fallback policy with synchronous error handling action delegate to the collection.
		/// </summary>
		/// <param name="fallback">The fallback action to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Action<CancellationToken> fallback, ErrorProcessorParam policyParams = null)
		{
			return WithFallback(fallback, false, policyParams);
		}

		/// <summary>
		/// Adds a fallback policy with synchronous error handling action delegate to the collection.
		/// </summary>
		/// <param name="fallback">The fallback action to execute when the primary operation fails.</param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Whether to use generic fallback for generic delegates.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Action<CancellationToken> fallback, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallback, policyParams, onlyGenericFallbackForGenericDelegate);
		}

		/// <summary>
		/// Adds a fallback policy with synchronous error handling action delegate to the collection.
		/// </summary>
		/// <param name="fallback">The fallback action to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="convertType">Cancellation type conversion strategy.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Action fallback, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return WithFallback(fallback, false, policyParams, convertType);
		}

		/// <summary>
		/// Adds a fallback policy with synchronous error handling action delegate to the collection.
		/// </summary>
		/// <param name="fallback">The fallback action to execute when the primary operation fails.</param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Whether to use generic fallback for generic delegates.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="convertType">Cancellation type conversion strategy.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Action fallback, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallback, policyParams, convertType, onlyGenericFallbackForGenericDelegate);
		}

		/// <summary>
		/// Adds a fallback policy with asynchronous error handling task delegate to the collection.
		/// </summary>
		/// <param name="fallbackAsync">The fallback async function to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Func<CancellationToken, Task> fallbackAsync, ErrorProcessorParam policyParams = null)
		{
			return WithFallback(fallbackAsync, false, policyParams);
		}

		/// <summary>
		/// Adds a fallback policy with asynchronous error handling task delegate to the collection.
		/// </summary>
		/// <param name="fallbackAsync">The fallback async function to execute when the primary operation fails.</param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Whether to use generic fallback for generic delegates.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Func<CancellationToken, Task> fallbackAsync, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallbackAsync, policyParams, onlyGenericFallbackForGenericDelegate);
		}

		/// <summary>
		/// Adds a fallback policy with asynchronous error handling task delegate to the collection.
		/// </summary>
		/// <param name="fallbackAsync">The fallback async function to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="convertType">Cancellation type conversion strategy.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Func<Task> fallbackAsync, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return WithFallback(fallbackAsync, false, policyParams, convertType);
		}

		/// <summary>
		/// Adds a fallback policy with asynchronous error handling task delegate to the collection.
		/// </summary>
		/// <param name="fallbackAsync">The fallback async function to execute when the primary operation fails.</param>
		/// <param name="onlyGenericFallbackForGenericDelegate">Whether to use generic fallback for generic delegates.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="convertType">Cancellation type conversion strategy.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(Func<Task> fallbackAsync, bool onlyGenericFallbackForGenericDelegate, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallbackAsync, policyParams, convertType, onlyGenericFallbackForGenericDelegate);
		}

		/// <summary>
		/// Adds a fallback policy with synchronous error handling function returning a value of type T to the collection.
		/// </summary>
		/// <typeparam name="T">The return type of the fallback function.</typeparam>
		/// <param name="fallbackFunc">The fallback function to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback<T>(Func<CancellationToken, T> fallbackFunc, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams);
		}

		/// <summary>
		/// Adds a fallback policy built from a provider of fallback function delegates to the collection.
		/// </summary>
		/// <param name="funcsProvider">A <see cref="FallbackFuncsProvider"/> containing fallback function delegates.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback(FallbackFuncsProvider funcsProvider)
		{
			return WithPolicy(new FallbackPolicy(funcsProvider));
		}

		/// <summary>
		/// Adds a fallback policy built from the pre-configured <paramref name="fallbackBehavior"/> to the collection.
		/// </summary>
		/// <typeparam name="T">A return type of the fallback delegate.</typeparam>
		/// <param name="fallbackBehavior">A pre-configured <see cref="FallbackBehavior{T}"/> to be added as a <see cref="FallbackPolicy"/>.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="fallbackBehavior"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="fallbackBehavior"/> has no fallback delegates configured (<see cref="FallbackExecutionMode.None"/>).</exception>
		public PolicyCollection WithFallback<T>(FallbackBehavior<T> fallbackBehavior)
		{
			if (fallbackBehavior == null)
			{
				throw new ArgumentNullException(nameof(fallbackBehavior));
			}
			if (fallbackBehavior.ExecutionMode == FallbackExecutionMode.None)
			{
				throw new InvalidOperationException($"A {nameof(FallbackBehavior<T>)} with {nameof(FallbackExecutionMode.None)} execution mode has no fallback delegates to add.");
			}
			return WithPolicy(fallbackBehavior.ToFallbackPolicy());
		}

		/// <summary>
		/// Adds a fallback policy with synchronous error handling function returning a value of type T to the collection.
		/// </summary>
		/// <typeparam name="T">The return type of the fallback function.</typeparam>
		/// <param name="fallbackFunc">The fallback function to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="convertType">Cancellation type conversion strategy.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback<T>(Func<T> fallbackFunc, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		/// <summary>
		/// Adds a fallback policy with asynchronous error handling function returning a value of type T to the collection.
		/// </summary>
		/// <typeparam name="T">The return type of the fallback function.</typeparam>
		/// <param name="fallbackFunc">The fallback async function to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback<T>(Func<CancellationToken, Task<T>> fallbackFunc, ErrorProcessorParam policyParams = null)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams);
		}

		/// <summary>
		/// Adds a fallback policy with asynchronous error handling function returning a value of type T to the collection.
		/// </summary>
		/// <typeparam name="T">The return type of the fallback function.</typeparam>
		/// <param name="fallbackFunc">The fallback async function to execute when the primary operation fails.</param>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <param name="convertType">Cancellation type conversion strategy.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithFallback<T>(Func<Task<T>> fallbackFunc, ErrorProcessorParam policyParams = null, CancellationType convertType = CancellationType.Precancelable)
		{
			return this.WithFallbackInner(fallbackFunc, policyParams, convertType);
		}

		/// <summary>
		/// Adds a simple policy to the collection.
		/// </summary>
		/// <param name="policyParams">Optional parameters for customizing error handling behavior.</param>
		/// <returns>The current <see cref="PolicyCollection"/> instance.</returns>
		public PolicyCollection WithSimple(ErrorProcessorParam policyParams = null)
		{
			return this.WithSimpleInner(policyParams);
		}
	}
}
