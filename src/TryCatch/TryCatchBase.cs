using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	/// <summary>
	/// Base class for executing delegates with exception handling using the <see cref="ITryCatch"/> interface.
	/// Delegates execution to the wrapped <see cref="TryCatch"/> instance, which is configured with
	/// <see cref="CatchBlockHandler"/> objects to catch and handle exceptions.
	/// </summary>
	public abstract class TryCatchBase : ITryCatch
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TryCatchBase"/> class with the specified <see cref="ITryCatch"/> instance.
		/// </summary>
		/// <param name="tryCatch">The <see cref="ITryCatch"/> instance to delegate execution to.</param>
		protected TryCatchBase(ITryCatch tryCatch)
		{
			TryCatch = tryCatch;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TryCatchBase"/> class.
		/// </summary>
		protected TryCatchBase(){}

		/// <summary>
		/// The <see cref="ITryCatch"/> instance that performs the actual delegate execution.
		/// </summary>
		protected ITryCatch TryCatch { get; set; }

		/// <summary>
		/// A number of <see cref="CatchBlockHandler"/> added.
		/// </summary>
		public int CatchBlockCount => TryCatch.CatchBlockCount;

		/// <summary>
		/// Indicates whether <see cref="ITryCatch"/> has a <see cref="CatchBlockForAllHandler"/> handler.
		/// </summary>
		public bool HasCatchBlockForAll => TryCatch.HasCatchBlockForAll;

		/// <summary>
		/// Executes the delegate and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <param name="action">A delegate to execute.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult"></see></returns>
		public TryCatchResult Execute(Action action, CancellationToken token = default) => TryCatch.Execute(action, token);

		/// <summary>
		/// Executes the generic delegate and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <typeparam name="T">The type of return value of the generic delegate.</typeparam>
		/// <param name="func">A delegate to execute.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult{T}"></see></returns>
		public TryCatchResult<T> Execute<T>(Func<T> func, CancellationToken token = default) => TryCatch.Execute(func, token);

		/// <summary>
		/// Executes the delegate asynchronously and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <param name="func">A delegate to execute.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult"></see></returns>
		public Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
									=> TryCatch.ExecuteAsync(func, configureAwait, token);

		/// <summary>
		/// Executes the generic delegate asynchronously and attempts to catch exceptions using <see cref="CatchBlockHandler"/> objects.
		/// </summary>
		/// <typeparam name="T">The type of return value of the generic delegate.</typeparam>
		/// <param name="func">A delegate to execute.</param>
		/// <param name="configureAwait">Specifies whether the asynchronous execution should attempt to continue on the captured context.</param>
		/// <param name="token"><see cref="CancellationToken"></see></param>
		/// <returns><see cref="TryCatchResult{T}"></see></returns>
		public Task<TryCatchResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
									=> TryCatch.ExecuteAsync(func, configureAwait, token);
	}
}
