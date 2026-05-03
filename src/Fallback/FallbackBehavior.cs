using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Registers and provides typed fallback delegates for <see cref="FallbackPolicy"/>.
	/// </summary>
	/// <typeparam name="T">A return type of fallback delegate.</typeparam>
	public sealed class FallbackBehavior<T>
	{
		private FallbackBehavior() { }

		/// <summary>
		/// Creates a new instance of <see cref="FallbackBehavior{T}"/> from a synchronous fallback delegate.
		/// </summary>
		/// <param name="fallbackFunc">A fallback delegate.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public static FallbackBehavior<T> Create(Func<T> fallbackFunc, CancellationType convertType = CancellationType.Precancelable)
		{
			if (fallbackFunc == null)
			{
				return new FallbackBehavior<T>() { ExecutionMode = FallbackExecutionMode.None };
			}

			return new FallbackBehavior<T>
			{
				Fallback = (convertType == CancellationType.Precancelable) ? fallbackFunc.ToPrecancelableFunc(true) : fallbackFunc.ToCancelableFunc(),
				ExecutionMode = FallbackExecutionMode.Sync
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="FallbackBehavior{T}"/> from a synchronous cancelable fallback delegate.
		/// </summary>
		/// <param name="fallbackFunc">A fallback delegate.</param>
		/// <returns></returns>
		public static FallbackBehavior<T> Create(Func<CancellationToken, T> fallbackFunc)
		{
			if (fallbackFunc == null)
			{
				return new FallbackBehavior<T>() { ExecutionMode = FallbackExecutionMode.None };
			}

			return new FallbackBehavior<T>
			{
				Fallback = fallbackFunc,
				ExecutionMode = FallbackExecutionMode.Sync
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="FallbackBehavior{T}"/> from an asynchronous fallback delegate.
		/// </summary>
		/// <param name="fallbackAsync">An async fallback delegate.</param>
		/// <param name="convertType"><see cref="CancellationType"/></param>
		/// <returns></returns>
		public static FallbackBehavior<T> Create(Func<Task<T>> fallbackAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			if (fallbackAsync == null)
			{
				return new FallbackBehavior<T>() { ExecutionMode = FallbackExecutionMode.None };
			}

			return new FallbackBehavior<T>
			{
				AsyncFallback = (convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc(true) : fallbackAsync.ToCancelableFunc(),
				ExecutionMode = FallbackExecutionMode.Async
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="FallbackBehavior{T}"/> from an asynchronous cancelable fallback delegate.
		/// </summary>
		/// <param name="fallbackAsync">An async fallback delegate.</param>
		/// <returns></returns>
		public static FallbackBehavior<T> Create(Func<CancellationToken, Task<T>> fallbackAsync)
		{
			if (fallbackAsync == null)
			{
				return new FallbackBehavior<T>() { ExecutionMode = FallbackExecutionMode.None };
			}

			return new FallbackBehavior<T>
			{
				AsyncFallback = fallbackAsync,
				ExecutionMode = FallbackExecutionMode.Async
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="FallbackBehavior{T}"/> from both synchronous and asynchronous
		/// cancelable fallback delegates.
		/// </summary>
		/// <param name="fallbackFunc">A synchronous cancelable fallback delegate.</param>
		/// <param name="fallbackAsync">An asynchronous cancelable fallback delegate.</param>
		/// <returns>
		/// A <see cref="FallbackBehavior{T}"/> configured with the provided delegates and corresponding
		/// <see cref="ExecutionMode"/>.
		/// </returns>
		public static FallbackBehavior<T> Create(Func<CancellationToken, T> fallbackFunc, Func<CancellationToken, Task<T>> fallbackAsync)
		{
			FallbackExecutionMode executionMode = FallbackExecutionMode.None;
			if (fallbackFunc != null)
			{
				executionMode |= FallbackExecutionMode.Sync;
			}
			if (fallbackAsync != null)
			{
				executionMode |= FallbackExecutionMode.Async;
			}

			return new FallbackBehavior<T>
			{
				Fallback = fallbackFunc,
				AsyncFallback = fallbackAsync,
				ExecutionMode = executionMode
			};
		}

		/// <summary>
		/// Gets the synchronous fallback delegate.
		/// </summary>
		public Func<CancellationToken, T> Fallback { get; private set; }

		/// <summary>
		/// Gets the asynchronous fallback delegate.
		/// </summary>
		public Func<CancellationToken, Task<T>> AsyncFallback { get; private set; }

		/// <summary>
		/// Gets the execution mode(s) of the fallback behavior.
		/// </summary>
		/// <value>
		/// A bitwise combination of <see cref="FallbackExecutionMode"/> values indicating
		/// which fallback delegates are configured.
		/// </value>
		public FallbackExecutionMode ExecutionMode { get; private set; }

		/// <summary>
		/// Converts this <see cref="FallbackBehavior{T}"/> instance to a <see cref="FallbackPolicy"/>.
		/// </summary>
		/// <returns>A <see cref="FallbackPolicy"/> configured with the delegates from this behavior.</returns>
		public FallbackPolicy ToFallbackPolicy()
		{
			var funcProvider = FallbackFuncsProvider.Create();
			if (Fallback != null)
			{
				funcProvider.AddOrReplaceFallbackFunc(Fallback);
			}
			if (AsyncFallback != null)
			{
				funcProvider.AddOrReplaceAsyncFallbackFunc(AsyncFallback);
			}
			return funcProvider.ToFallbackPolicy();
		}
	}

	/// <summary>
	/// Specifies the execution mode(s) supported by a fallback behavior.
	/// This enumeration supports bitwise combination of values.
	/// </summary>
	[Flags]
	public enum FallbackExecutionMode
	{
		/// <summary>
		/// No fallback delegate is configured.
		/// </summary>
		None = 0,

		/// <summary>
		/// A synchronous fallback delegate is configured.
		/// </summary>
		Sync = 1,

		/// <summary>
		/// An asynchronous fallback delegate is configured.
		/// </summary>
		Async = 2
	}
}
