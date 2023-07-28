using System;
using System.Threading;

namespace PoliNorError
{
	internal sealed class FallbackFuncExecResult<T> : FallbackFuncExecResult
	{
		private FallbackFuncExecResult() { }

		internal static FallbackFuncExecResult<T> FromFuncExecWithTokenResult(FallbackFuncExecResult funcExecWithTokenResult)
		{
			return new FallbackFuncExecResult<T>
			{
				IsCanceled = funcExecWithTokenResult.IsCanceled,
				Error = funcExecWithTokenResult.Error
			};
		}

		public void SetResult(T result)
		{
			Result = result;
		}

		public T Result { get; private set; }
	}

	internal class FallbackFuncExecResult
	{
		protected FallbackFuncExecResult() { }

		public static FallbackFuncExecResult Success() => new FallbackFuncExecResult();

		public static FallbackFuncExecResult FromErrorAndToken(OperationCanceledException exception, CancellationToken token)
		{
			if (exception.CancellationToken.Equals(token))
				return new FallbackFuncExecResult() { IsCanceled = true };

			return FromError(exception);
		}

		public static FallbackFuncExecResult FromErrorAndToken(AggregateException exception, CancellationToken token)
		{
			if (exception.HasCanceledException(token))
				return new FallbackFuncExecResult() { IsCanceled = true };

			return FromError(exception);
		}

		public static FallbackFuncExecResult FromError(Exception exception) => new FallbackFuncExecResult() { Error = exception };

		public bool IsCanceled { get; protected set; }
		public Exception Error { get; protected set; }

		public bool IsSuccess => Error is null && !IsCanceled;
	}
}
