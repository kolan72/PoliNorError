using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class TaskExtensions
	{
		public static Task WithCancellation(this Task task, CancellationToken cancelToken)
		{
			var tcs = new TaskCompletionSource<object>();
			var reg = cancelToken.Register(() => tcs.TrySetCanceled(cancelToken));
			task.ContinueWith(ant =>
			{
				reg.Dispose();
				if (ant.IsCanceled)
					tcs.TrySetCanceled(cancelToken);
				else if (ant.IsFaulted)
					tcs.TrySetException(ant.Exception.InnerException);
				else
					tcs.TrySetResult(null);
			});
			return tcs.Task;
		}

		public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancelToken)
		{
			var tcs = new TaskCompletionSource<T>();
			var reg = cancelToken.Register(() => tcs.TrySetCanceled(cancelToken));
			task.ContinueWith(ant =>
			{
				reg.Dispose();
				if (ant.IsCanceled)
					tcs.TrySetCanceled(cancelToken);
				else if (ant.IsFaulted)
					tcs.TrySetException(ant.Exception.InnerException);
				else
					tcs.TrySetResult(ant.Result);
			});
			return tcs.Task;
		}
	}
}
