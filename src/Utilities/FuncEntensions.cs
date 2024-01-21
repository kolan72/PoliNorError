using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class FuncEntensions
	{
		public static Func<CancellationToken, Task> ToTaskReturnFunc(this Action<CancellationToken> action)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return Task.FromCanceled(ct);
				}
				action(ct);
				return Task.CompletedTask;
			};
		}

		public static Func<CancellationToken, Task<T>> ToTaskReturnFunc<T>(this Func<CancellationToken, T> func)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return Task.FromCanceled<T>(ct);
				}
				return Task.FromResult(func(ct));
			};
		}

		public static Func<CancellationToken, Task> ToAsyncFunc(this Action<CancellationToken> action)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return Task.FromCanceled(ct);
				}
				action(ct);
				return Task.CompletedTask;
			};
		}

		public static Func<CancellationToken, Task<T>> ToAsyncFunc<T>(this Func<CancellationToken, T> func)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return Task.FromCanceled<T>(ct);
				}
				return Task.FromResult(func(ct));
			};
		}

		public static Action<CancellationToken> ToSyncFunc(this Func<CancellationToken, Task> func)
		{
			return (ct) => Task.Run(() => func(ct), ct).Wait(ct);
		}

		public static Func<CancellationToken, T> ToSyncFunc<T>(this Func<CancellationToken, Task<T>> func)
		{
			return (ct) => { var t = Task.Run(() => func(ct), ct); t.Wait(ct); return t.Result; };
		}

		public static Func<CancellationToken, T> ToDefaultReturnFunc<T>(this Action<CancellationToken> action)
		{
			return (ct) => { action(ct); return default; };
		}

		public static Func<CancellationToken, Task<T>> ToDefaultReturnFunc<T>(this Func<CancellationToken, Task> func, bool configureAwait = false)
		{
			return async (ct) => { await func(ct).ConfigureAwait(configureAwait); return default; };
		}

		public static Action<T, CancellationToken> ToCancelableAction<T>(this Action<T> action, CancellationType convertType)
		{
			if (convertType == CancellationType.Precancelable)
				return action.ToPrecancelableAction();
			else
				return action.ToCancelableAction();
		}

		public static Action<T, CancellationToken> ToCancelableAction<T>(this Action<T> action)
		{
			return (ex, ct) => Task.Run(() => action(ex), ct).Wait(ct);
		}

		public static Action<T, K, CancellationToken> ToCancelableAction<T, K>(this Action<T, K> action)
		{
			return (ex, k, ct) => Task.Run(() => action(ex, k), ct).Wait(ct);
		}

		public static Action<CancellationToken> ToCancelableAction(this Action action, CancellationType convertType, bool throwIfCanceled = false)
		{
			if (convertType == CancellationType.Precancelable)
				return action.ToPrecancelableAction(throwIfCanceled);
			else
				return action.ToCancelableAction();
		}

		public static Action<CancellationToken> ToCancelableAction(this Action action)
		{
			return (ct) => Task.Run(action, ct).Wait(ct);
		}

		public static Action<T, K, CancellationToken> ToCancelableAction<T, K>(this Func<T, K, Task> func)
		{
			return (e, k, ct) => func(e, k).Wait(ct);
		}

		public static Action<CancellationToken> ToPrecancelableAction(this Action action, bool throwIfCanceled = false)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					if (throwIfCanceled)
						throw new OperationCanceledException(ct);
					else
						return;
				}
				action();
			};
		}

		public static Action<T, CancellationToken> ToPrecancelableAction<T>(this Action<T> action)
		{
			return (e, ct) =>
			{
				if (ct.IsCancellationRequested)
					return;
				action(e);
			};
		}

		public static Action<T, K, CancellationToken> ToPrecancelableAction<T, K>(this Action<T, K> action)
		{
			return (e, k, ct) =>
			{
				if (ct.IsCancellationRequested)
					return;
				action(e, k);
			};
		}

		public static Action<T, K, CancellationToken> ToPrecancelableAction<T, K>(this Func<T, K, Task> func)
		{
			return (e, k, ct) =>
			{
				if (ct.IsCancellationRequested)
					return;
				func(e, k).Wait();
			};
		}

		public static Func<T, CancellationToken, Task> ToCancelableFunc<T>(this Func<T, Task> fnTask, CancellationType convertType)
		{
			return (convertType == CancellationType.Precancelable) ? fnTask.ToPrecancelableFunc() : fnTask.ToCancelableFunc();
		}

		public static Func<T, CancellationToken, Task> ToCancelableFunc<T>(this Func<T, Task> fnTask)
		{
			return (t, ct) => fnTask(t).WithCancellation(ct);
		}

		public static Func<T, K, CancellationToken, Task> ToCancelableFunc<T, K>(this Func<T, K, Task> fnTask)
		{
			return (t, k, ct) => fnTask(t,k).WithCancellation(ct);
		}

		public static Func<CancellationToken, Task> ToCancelableFunc(this Func<Task> fallbackAsync, CancellationType convertType, bool throwIfCanceled = false)
		{
			return (convertType == CancellationType.Precancelable) ? fallbackAsync.ToPrecancelableFunc(throwIfCanceled) : fallbackAsync.ToCancelableFunc();
		}

		public static Func<CancellationToken, T> ToCancelableFunc<T>(this Func<T> func)
		{
			return (ct) => { var t = Task.Run(() => func(), ct); t.Wait(ct); return t.Result; };
		}

		public static Func<CancellationToken, Task<T>> ToCancelableFunc<T>(this Func<Task<T>> fnTask)
		{
			return (ct) => fnTask().WithCancellation(ct);
		}

		public static Func<CancellationToken, Task> ToCancelableFunc(this Func<Task> fnTask)
		{
			return (ct) => fnTask().WithCancellation(ct);
		}

		public static Func<CancellationToken, T> ToPrecancelableFunc<T>(this Func<T> fnTask, bool throwIfCanceled = false)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					if (throwIfCanceled)
						throw new OperationCanceledException(ct);
					else
						return default;
				}
				return fnTask();
			};
		}

		public static Func<CancellationToken, Task> ToPrecancelableFunc(this Func<Task> fnTask, bool throwIfCanceled = false)
		{
			return async (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					if (throwIfCanceled)
						throw new OperationCanceledException(ct);
					else
						return;
				}
				await fnTask();
			};
		}

		public static Func<T, K, CancellationToken, Task> ToPrecancelableFunc<T, K>(this Func<T, K, Task> fnTask)
		{
			return async (t, k, ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return;
				}
				await fnTask(t, k);
			};
		}

		public static Func<CancellationToken, Task<T>> ToPrecancelableFunc<T>(this Func<Task<T>> fnTask, bool throwIfCanceled = false)
		{
			return async (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					if (throwIfCanceled)
						throw new OperationCanceledException(ct);
					else
						return default;
				}
				return await fnTask();
			};
		}

		public static Func<T, CancellationToken, Task> ToPrecancelableFunc<T>(this Func<T, Task> func)
		{
			return async (t, ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return;
				}
				await func(t);
			};
		}
	}
}
