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
				throw new ArgumentNullException(nameof(fallbackFunc));
			}

			var provider = new FallbackBehavior<T>();
			provider.Fallback = (convertType == CancellationType.Precancelable) ? fallbackFunc.ToPrecancelableFunc(true) : fallbackFunc.ToCancelableFunc();
			provider.HasFallback = true;
			return provider;
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
				throw new ArgumentNullException(nameof(fallbackFunc));
			}

			return new FallbackBehavior<T>
			{
				Fallback = fallbackFunc,
				HasFallback = true
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
				throw new ArgumentNullException(nameof(fallbackAsync));
			}

			return new FallbackBehavior<T>
			{
				AsyncFallback = (convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc(true) : fallbackAsync.ToCancelableFunc(),
				HasAsyncFallback = true
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
				throw new ArgumentNullException(nameof(fallbackAsync));
			}

			return new FallbackBehavior<T>
			{
				AsyncFallback = fallbackAsync,
				HasAsyncFallback = true
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
		/// Gets a value indicating whether this instance has a synchronous fallback delegate.
		/// </summary>
		public bool HasFallback { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance has an asynchronous fallback delegate.
		/// </summary>
		public bool HasAsyncFallback { get; private set; }
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
