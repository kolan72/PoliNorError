using System;
using System.Threading;

namespace PoliNorError
{
	internal sealed class FuncExecWithTokenResult<T> : FuncExecWithTokenResult
	{
		private FuncExecWithTokenResult() { }

		internal static FuncExecWithTokenResult<T> FromFuncExecWithTokenResult(FuncExecWithTokenResult funcExecWithTokenResult)
		{
			return new FuncExecWithTokenResult<T>
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

	internal class FuncExecWithTokenResult
	{
		protected FuncExecWithTokenResult() { }

		public static FuncExecWithTokenResult Success() => new FuncExecWithTokenResult();

		public static FuncExecWithTokenResult FromErrorAndToken(OperationCanceledException exception, CancellationToken token)
		{
			if (exception.CancellationToken.Equals(token))
				return new FuncExecWithTokenResult() { IsCanceled = true };

			return FromError(exception);
		}

		public static FuncExecWithTokenResult FromErrorAndToken(AggregateException exception, CancellationToken token)
		{
			if (exception.HasCanceledException(token))
				return new FuncExecWithTokenResult() { IsCanceled = true };

			return FromError(exception);
		}

		public static FuncExecWithTokenResult FromError(Exception exception) => new FuncExecWithTokenResult() { Error = exception };

		public bool IsCanceled { get; protected set; }
		public Exception Error { get; protected set; }

		public bool IsSuccess => Error is null && !IsCanceled;
	}
}
