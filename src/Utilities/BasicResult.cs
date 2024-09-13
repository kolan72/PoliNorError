using System;

namespace PoliNorError
{
	internal sealed class BasicResult
	{
		private BasicResult(){}

		public static BasicResult Success() => new BasicResult();

		public static BasicResult Failure(Exception error) => new BasicResult() { IsFailed = true, Error = error };

		public static BasicResult Canceled() => new BasicResult() { IsCanceled = true };

		public bool IsFailed { get; private set; }

		public bool IsCanceled { get; private set; }

		public bool IsBasicSuccess => !IsFailed && !IsCanceled;

		public Exception Error { get; private set; }
}
}
