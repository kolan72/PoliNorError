using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Defines methods for executing delegates with help of added <see cref="CatchBlockHandler"/> objects.
	/// </summary>
	public interface ITryCatch
	{
		/// <summary>
		/// Executes the delegate and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <param name="action">A delegate to execute.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult"></see></returns>
		TryCatchResult Execute(Action action, CancellationToken token = default);

		/// <summary>
		/// Executes the generic delegate and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <typeparam name="T">The type of return value of the generic delegate.</typeparam>
		/// <param name="func">A delegate to execute.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult{T}"></see></returns>
		TryCatchResult<T> Execute<T>(Func<T> func, CancellationToken token = default);

		/// <summary>
		/// Executes the delegate asynchronously and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <param name="func">A delegate to execute.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult"></see></returns>
		Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		/// Executes the generic delegate asynchronously and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <typeparam name="T">The type of return value of the generic delegate.</typeparam>
		/// <param name="func">A delegate to execute.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult{T}"></see></returns>
		Task<TryCatchResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default);

		/// <summary>
		/// A number of <see cref="CatchBlockHandler"/> added.
		/// </summary>
		int CatchBlockCount { get; }
	}

	public class TryCatch : ITryCatch
	{
		private readonly SimplePolicy _simplePolicy;

		internal TryCatch(IEnumerable<CatchBlockHandler> catchBlockHandlers)
		{
			_simplePolicy = CatchBlockHandlerCollectionWrapper.Wrap(catchBlockHandlers);
			CatchBlockCount = catchBlockHandlers.Count();
		}

		public TryCatchResult Execute(Action action, CancellationToken token = default)
		{
			return new TryCatchResult(_simplePolicy.Handle(action, token), CatchBlockCount);
		}

		public TryCatchResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
		{
			return new TryCatchResult<T>(_simplePolicy.Handle(func, token), CatchBlockCount);
		}

		public async Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			return new TryCatchResult(await _simplePolicy.HandleAsync(func, configureAwait, token).ConfigureAwait(configureAwait), CatchBlockCount);
		}

		public async Task<TryCatchResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			return new TryCatchResult<T>(await _simplePolicy.HandleAsync(func, configureAwait, token).ConfigureAwait(configureAwait), CatchBlockCount);
		}

		public int CatchBlockCount { get; }
	}
}
