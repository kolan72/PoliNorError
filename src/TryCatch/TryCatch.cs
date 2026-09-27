using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	public class TryCatch : ITryCatch
	{
		private readonly SimplePolicy _simplePolicy;

		internal TryCatch(IReadOnlyList<CatchBlockHandler> catchBlockHandlers, bool hasCatchBlockForAll)
		{
			_simplePolicy = CatchBlockHandlerCollectionWrapper.Wrap(catchBlockHandlers);
			CatchBlockCount = catchBlockHandlers.Count;
			HasCatchBlockForAll = hasCatchBlockForAll;
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

		public bool HasCatchBlockForAll { get; }
	}
}
