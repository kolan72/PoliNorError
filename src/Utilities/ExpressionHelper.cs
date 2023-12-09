using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PoliNorError
{
	internal static class ExpressionHelper
	{
		public static Expression<Func<Exception, bool>> GetTypedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			return (exc) => exc.GetType() == typeof(TException) && (func == null || func((TException)exc));
		}
	}
}
