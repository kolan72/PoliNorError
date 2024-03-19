using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	public interface ITryCatch
	{
		TryCatchResult Execute(Action action, CancellationToken token = default);
		TryCatchResult<T> Execute<T>(Func<T> func, CancellationToken token = default);

		Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default);
		Task<TryCatchResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default);

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
			return new TryCatchResult(_simplePolicy.Handle(action, token));
		}

		public TryCatchResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
		{
			return new TryCatchResult<T>(_simplePolicy.Handle(func, token));
		}

		public async Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			return new TryCatchResult(await _simplePolicy.HandleAsync(func, configureAwait, token).ConfigureAwait(configureAwait));
		}

		public async Task<TryCatchResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			return new TryCatchResult<T>(await _simplePolicy.HandleAsync(func, configureAwait, token).ConfigureAwait(configureAwait));
		}

		public int CatchBlockCount { get; }
	}
}
