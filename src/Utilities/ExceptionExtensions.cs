using System;
using System.Linq;
using System.Threading;

namespace PoliNorError
{
	internal static class ExceptionExtensions
	{
		public static bool IsOperationCanceledWithRequestedToken(this AggregateException _,
														   CancellationToken token) => token.IsCancellationRequested;

		public static bool HasCanceledException(this AggregateException ae, CancellationToken token) => ae.Flatten().InnerExceptions
																														.Any(ie => ie is OperationCanceledException operationCanceledException && operationCanceledException.CancellationToken.Equals(token));


		public static OperationCanceledException GetCancellationException(this AggregateException aggregateException, CancellationToken token = default)
		{
			// Fast path: check direct inner exceptions first (most common case)
			var innerExceptions = aggregateException.InnerExceptions;

			for (int i = 0; i < innerExceptions.Count; i++)
			{
				if (innerExceptions[i] is OperationCanceledException oce && oce.CancellationToken.Equals(token))
				{
					return oce;
				}
			}

			// Slow path: check nested aggregate exceptions (rare case)
			// This call won't be inlined but happens infrequently
			return GetCancellationExceptionSlow(aggregateException, token);
		}

		private static OperationCanceledException GetCancellationExceptionSlow(AggregateException aggregateException, CancellationToken token)
		{
			// Check if there are nested AggregateExceptions
			var flattenedExceptions = aggregateException.Flatten().InnerExceptions;

			for (int i = 0; i < flattenedExceptions.Count; i++)
			{
				if (flattenedExceptions[i] is OperationCanceledException oce && oce.CancellationToken.Equals(token))
				{
					return oce;
				}
			}

			return new ServiceOperationCanceledException();
		}

		internal static bool DataContainsKeyStringWithValue<TValue>(this Exception exception, string key, TValue value)
		{
			return exception.Data.Contains(key) && exception.Data[key].GetType() == typeof(TValue) && ((TValue)exception.Data[key]).Equals(value);
		}
	}
}
