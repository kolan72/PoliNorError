using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static PoliNorError.CatchBlockFilter;

namespace PoliNorError
{
	public abstract partial class PolicyProcessor
	{
		public class ExceptionFilter
		{
			public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => FilterSet.IncludedErrorFilters;

			public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => FilterSet.ExcludedErrorFilters;

			public ExceptionFilter IncludeErrorSet(IErrorSet errorSet)
			{
				this.AddIncludedErrorSet(errorSet);
				return this;
			}

			public ExceptionFilter ExcludeErrorSet(IErrorSet errorSet)
			{
				this.AddExcludedErrorSet(errorSet);
				return this;
			}

			public ExceptionFilter IncludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
			{
				return IncludeError<TException>(null, errorType);
			}

			public ExceptionFilter IncludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
			{
				switch (errorType)
				{
					case ErrorType.Error:
						AddIncludedErrorFilter(func);
						return this;
					case ErrorType.InnerError:
						AddIncludedInnerErrorFilter(func);
						return this;
					default:
						throw new NotImplementedException();
				}
			}

			internal ExceptionFilterSet FilterSet { get; } = new ExceptionFilterSet();

			internal void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				FilterSet.IncludeFilter(handledErrorFilter);
			}

			internal void AddIncludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			internal void AddIncludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			internal void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				FilterSet.ExcludeFilter(handledErrorFilter);
			}

			internal void AddExcludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			internal void AddExcludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			internal void AppendFilter(ExceptionFilter exceptionFilter)
			{
				FilterSet.AppendFilter(exceptionFilter.FilterSet);
			}

			internal Func<Exception, bool> GetCanHandle()
			{
				return FilterSet.CompilePredicate();
			}
		}
	}
}
