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
	}
}
