using System;

namespace PoliNorError
{
	internal static class ConvertExceptionDelegates
	{
		public static bool ToInnerException<TException>(Exception exception, out TException typedException) where TException : Exception
		{
			if (exception.InnerException?.GetType() == typeof(TException))
			{
				typedException = (TException)exception.InnerException;
				return true;
			}
			else
			{
				typedException = null;
				return false;
			}
		}

		public static bool TryCast<TException>(Exception exception, out TException typedException) where TException : Exception
		{
			TException probe = typedException = exception as TException;
			return probe != null;
		}

		public static bool TryAsExact<TException>(Exception exception, out TException typedException) where TException : Exception
		{
			typedException = exception?.GetType() == typeof(TException) ? (TException)exception : null;
			return typedException != null;
		}
	}
}
