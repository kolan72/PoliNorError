using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	internal static class ExpressionHelper
	{
		public static Expression<Func<Exception, bool>> GetTypedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			return (exc) => exc.GetType() == typeof(TException) && (func == null || func((TException)exc));
		}

		public static Expression<Func<Exception, bool>> GetTypedInnerErrorFilter<TInnerException>(Func<TInnerException, bool> func = null) where TInnerException : Exception
		{
			return (exc) => (exc.InnerException != null) && exc.InnerException.GetType() == typeof(TInnerException) && (func == null || func((TInnerException)exc.InnerException));
		}
	}
}
