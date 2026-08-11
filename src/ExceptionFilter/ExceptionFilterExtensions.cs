using System;
using static PoliNorError.ErrorSet;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	internal static class ExceptionFilterExtensions
	{
		internal static void AddIncludedErrorSet(this ExceptionFilter errorFilter, IErrorSet errorSet)
		{
			errorSet.Items.ActionForAll((item) => errorFilter.AddIncludedError(item));
		}

		internal static void AddExcludedErrorSet(this ExceptionFilter errorFilter, IErrorSet errorSet)
		{
			errorSet.Items.ActionForAll((item) => errorFilter.AddExcludedError(item));
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

		internal static bool ShouldPropagateFilterUnsatisfied(this ExceptionFilter errorFilter, Exception originalEx, bool rethrowIfErrorFilterUnsatisfied, out bool filterAccepted, out Exception filterException)
		{
			filterAccepted = false;
			filterException = null;
			try
			{
				var filterResult = errorFilter.GetCanHandle()(originalEx);
				if (!filterResult)
				{
					if (rethrowIfErrorFilterUnsatisfied)
					{
						originalEx.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY] = true;
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					filterAccepted = true;
					return false;
				}
			}
			catch (Exception fe)
			{
				filterException = fe;
				return false;
			}
		}
	}
}
