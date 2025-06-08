using System;
using System.Linq;
using System.Threading;

namespace PoliNorError
{
	internal static class ExceptionExtensions
	{
		public static bool HasCanceledException(this AggregateException ae, CancellationToken token) => ae.Flatten().InnerExceptions
																														.Any(ie => ie is OperationCanceledException operationCanceledException && operationCanceledException.CancellationToken.Equals(token));

		public static OperationCanceledException GetCancellationException(this AggregateException aggregateException)
		{
			return aggregateException.Flatten()
								.InnerExceptions
								.OfType<OperationCanceledException>()
								.FirstOrDefault()
								??
								new OperationCanceledException();
		}

		internal static bool DataContainsKeyStringWithValue<TValue>(this Exception exception, string key, TValue value)
		{
			return exception.Data.Contains(key) && exception.Data[key].GetType() == typeof(TValue) && ((TValue)exception.Data[key]).Equals(value);
		}
	}
}
