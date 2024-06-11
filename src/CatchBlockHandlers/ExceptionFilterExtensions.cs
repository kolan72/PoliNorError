using System;
using static PoliNorError.ErrorSet;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	internal static class ExceptionFilterExtensions
	{
		internal static void AddIncludedErrorSet(this ExceptionFilter errorFilter, IErrorSet errorSet)
		{
			foreach (var item in errorSet.Items)
			{
				errorFilter.AddIncludedError(item);
			}
		}

		internal static void AddExcludedErrorSet(this ExceptionFilter errorFilter, IErrorSet errorSet)
		{
			foreach (var item in errorSet.Items)
			{
				errorFilter.AddExcludedError(item);
			}
		}

		internal static void AddIncludedError(this ExceptionFilter errorFilter, ErrorSetItem errorSetItem)
		{
			if (errorSetItem.ErrorKind == ErrorSetItem.ItemType.Error)
			{
				errorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(errorSetItem.ErrorType));
			}
			else if (errorSetItem.ErrorKind == ErrorSetItem.ItemType.InnerError)
			{
				errorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(errorSetItem.ErrorType));
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		internal static void AddExcludedError(this ExceptionFilter errorFilter, ErrorSetItem errorSetItem)
		{
			if (errorSetItem.ErrorKind == ErrorSetItem.ItemType.Error)
			{
				errorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(errorSetItem.ErrorType));
			}
			else if (errorSetItem.ErrorKind == ErrorSetItem.ItemType.InnerError)
			{
				errorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(errorSetItem.ErrorType));
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
