using System;

namespace PoliNorError
{
	public struct RetryCountInfo
	{
		public const int DEFAULT_RETRY_COUNT = 1;

		private const int REAL_INFINITE_RETRY_COUNT = int.MaxValue - 1;

		private readonly Func<int, bool> _canRetryFunc;

		internal RetryCountInfo(int retryCount = DEFAULT_RETRY_COUNT, Func<int, bool> canRetryInner = null, int startTryCount = 0)
		{
			IsInfinite = retryCount >= REAL_INFINITE_RETRY_COUNT;
			IsLimited = !IsInfinite;

			var innerRetryCount = RetryCount = CorrectRetryCount(IsLimited);

			StartTryCount = startTryCount;

			_canRetryFunc = GetRetryInnerFunc(startTryCount, innerRetryCount, canRetryInner);

			int CorrectRetryCount(bool isLimited)
			{
				return isLimited ? CorrectRetries(retryCount) : REAL_INFINITE_RETRY_COUNT;
			}
		}

		private static Func<int, bool> GetRetryInnerFunc(int startTryCount, int retryCount, Func<int, bool> canRetryInner = null)
		{
			bool func(int nr) { return (nr - startTryCount) < retryCount && nr < REAL_INFINITE_RETRY_COUNT; }

			return canRetryInner ?? func;
		}

		public static RetryCountInfo Limited(int retryCount, Action<RetryCountInfoOptions> action = null)
		{
			return GetCountInfo(CorrectRetries(retryCount), action);
		}

		public static RetryCountInfo Infinite(Action<RetryCountInfoOptions> action = null)
		{
			return GetCountInfo(CorrectRetries(REAL_INFINITE_RETRY_COUNT), action);
		}

		private static RetryCountInfo GetCountInfo(int realCount,  Action<RetryCountInfoOptions> action = null)
		{
			var rco = new RetryCountInfoOptions();
			action?.Invoke(rco);
			return new RetryCountInfo(realCount, rco.CanRetryInner, rco.StartTryCount);
		}

		public bool CanRetry(int numOfCurRetry) => _canRetryFunc(numOfCurRetry);

		public int RetryCount { get; }

		public int StartTryCount { get; }

		public bool IsInfinite { get; }

		private bool IsLimited { get; }

		private static int CorrectRetries(int retryCount)
		{
			if (retryCount > 0)
			{
				return retryCount == int.MaxValue ? REAL_INFINITE_RETRY_COUNT : retryCount;
			}
			else
			{
				return DEFAULT_RETRY_COUNT;
			}
		}
	}
}
