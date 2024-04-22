using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.TryCatch
{
	public abstract class TryCatchBase : ITryCatch
	{
		protected TryCatchBase(ITryCatch tryCatch)
		{
			TryCatch = tryCatch;
		}

		protected TryCatchBase(){}

		protected ITryCatch TryCatch { get; set; }

		public int CatchBlockCount => TryCatch.CatchBlockCount;

		public bool HasCatchBlockForAll => TryCatch.HasCatchBlockForAll;

		public TryCatchResult Execute(Action action, CancellationToken token = default) => TryCatch.Execute(action, token);

		public TryCatchResult<T> Execute<T>(Func<T> func, CancellationToken token = default) => TryCatch.Execute(func, token);

		public Task<TryCatchResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
									=> TryCatch.ExecuteAsync(func, configureAwait, token);

		public Task<TryCatchResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
									=> TryCatch.ExecuteAsync(func, configureAwait, token);
	}
}
