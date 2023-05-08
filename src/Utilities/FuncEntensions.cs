using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class FuncEntensions
	{
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

		public static Action<T, CancellationToken> ToCancelableAction<T>(this Action<T> action, ConvertToCancelableFuncType convertType)
		{
			if (convertType == ConvertToCancelableFuncType.Precancelable)
				return action.ToPrecancelableAction();
			else
				return action.ToCancelableAction();
		}

		public static Action<T, CancellationToken> ToCancelableAction<T>(this Action<T> action)
		{
			return (ex, ct) => Task.Run(() => action(ex), ct).Wait(ct);
		}

		public static Action<CancellationToken> ToCancelableAction(this Action action)
		{
			return (ct) => Task.Run(action, ct).Wait(ct);
		}

		public static Action<CancellationToken> ToCancelableAction(this Action action, ConvertToCancelableFuncType convertType)
		{
			if (convertType == ConvertToCancelableFuncType.Precancelable)
				return action.ToPrecancelableAction();
			else
				return action.ToCancelableAction();
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

		public static Action<CancellationToken> ToPrecancelableAction(this Action action)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
					return;
				action();
			};
		}

		public static Func<T, CancellationToken, Task> ToCancelableFunc<T>(this Func<T, Task> fnTask, ConvertToCancelableFuncType convertType)
		{
			return (convertType == ConvertToCancelableFuncType.Precancelable) ? fnTask.ToPrecancelableFunc() : fnTask.ToCancelableFunc();
		}

		public static Func<T, CancellationToken, Task> ToCancelableFunc<T>(this Func<T, Task> fnTask)
		{
			return (t, ct) => fnTask(t).WithCancellation(ct);
		}

		public static Func<CancellationToken, Task> ToCancelableFunc(this Func<Task> fallbackAsync, ConvertToCancelableFuncType convertType)
		{
			return (convertType == ConvertToCancelableFuncType.Precancelable) ? fallbackAsync.ToPrecancelableFunc() : fallbackAsync.ToCancelableFunc();
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

		public static Func<CancellationToken, T> ToPrecancelableFunc<T>(this Func<T> fnTask)
		{
			return (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return default;
				}
				return fnTask();
			};
		}

		public static Func<CancellationToken, Task> ToPrecancelableFunc(this Func<Task> fnTask)
		{
			return async (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
					return;
				}
				await fnTask();
			};
		}

		public static Func<CancellationToken, Task<T>> ToPrecancelableFunc<T>(this Func<Task<T>> fnTask)
		{
			return async (ct) =>
			{
				if (ct.IsCancellationRequested)
				{
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

		//Use Apply methods if you want to convert many-args delegate to the one-arg one.
		public static Action<T2> Apply<T1, T2>(this Action<T1, T2> func, T1 t1)
					=> (t2) => func(t1, t2);

		public static Action<T2, T3> Apply<T1, T2, T3>(this Action<T1, T2, T3> func, T1 t1)
					=> (t2, t3) => func(t1, t2, t3);

		public static Func<T2, R> Apply<T1, T2, R>(this Func<T1, T2, R> func, T1 t1)
					=> t2 => func(t1, t2);

		public static Func<T2, T3, R> Apply<T1, T2, T3, R>(this Func<T1, T2, T3, R> func, T1 t1)
					=> (t2, t3) => func(t1, t2, t3);

		public static Action<T3> BulkApply<T1, T2, T3>(this Action<T1, T2, T3> func, T1 t1, T2 t2) => func.Apply(t1).Apply(t2);

		public static Func<T3, R> BulkApply<T1, T2, T3, R>(this Func<T1, T2, T3, R> func, T1 t1, T2 t2) => func.Apply(t1).Apply(t2);
	}
}
